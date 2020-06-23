using Microsoft.UI.Xaml.Controls;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace BroadFileSystemAccess
{
    public class MainViewModel : ObservableObject
    {
        public ICommand CreateCommand { get; set; }

        public ICommand OpenCommand { get; set; }

        public ICommand InvokeItemCommand { get; set; }

        public ICommand ExpandingCommand { get; set; }

        public ICommand CollapsedCommand { get; set; }

        private IList<FolderModel> folders;
        /// <summary>
        /// 폴더들
        /// </summary>
        public IList<FolderModel> Folders
        {
            get => folders;
            set => SetProperty(ref folders, value);
        }

        private FolderModel currentFolder;
        /// <summary>
        /// 현재폴더
        /// </summary>
        public FolderModel CurrentFolder
        {
            get => currentFolder;
            set => SetProperty(ref currentFolder, value);
        }

        private IList<FileModel> files;
        /// <summary>
        /// 파일들
        /// </summary>
        public IList<FileModel> Files
        {
            get => files;
            set => SetProperty(ref files, value);
        }

        private FileModel currentFile;
        /// <summary>
        /// 현재파일
        /// </summary>
        public FileModel CurrentFile
        {
            get => currentFile;
            set => SetProperty(ref currentFile, value);
        }

        private bool isBusy;
        /// <summary>
        /// IsBusy
        /// </summary>
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public MainViewModel()
        {
            if (DesignMode.DesignMode2Enabled)
            {
                return;
            }

            Init();
        }

        private void Init()
        {
            InvokeItemCommand = new AsyncCommand<object>(OnInvokeItemAsync, CanInvokeItem);
            ExpandingCommand = new AsyncCommand<object>(OnExpandingAsync, CanExpanding);
            CollapsedCommand = new AsyncCommand<object>(OnCollapsedAsync, CanCollapsed);

            OpenCommand = new AsyncCommand(OnOpenAsync, CanOpen);
            CreateCommand = new AsyncCommand(OnCreateAsync, CanCreate);

            PropertyChanged += MainViewModel_PropertyChanged;
        }

        private bool CanCollapsed(object obj)
        {
            return !isBusy;
        }

        private async Task OnCollapsedAsync(object obj)
        {
            if(obj is TreeViewCollapsedEventArgs args
                && args.Item is FolderModel folder)
            {
                IsBusy = true;
                await Task.Delay(100);
                CurrentFolder = folder;
                IsBusy = false;
            }
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(IsBusy):
                case nameof(CurrentFolder):
                case nameof(CurrentFile):
                    CheckButtons();
                    break;
            }
        }

        private bool CanExpanding(object arg)
        {
            return !IsBusy;
        }

        private async Task OnExpandingAsync(object obj)
        {
            if (obj is TreeViewExpandingEventArgs args
                && args.Item is FolderModel folder)
            {
                //순차 처리
                //IsBusy = true;
                //foreach (FolderModel f in folder.SubFolders)
                //{
                //    await GetSubDirectoriesAsync(f);
                //}
                //IsBusy = false;

                //다중 Task 처리
                IsBusy = true;

                await DispatcherRunAsync(async () => 
                {
                    var tasks = from f in folder.SubFolders
                                select GetSubDirectoriesAsync(f);
                    await Task.WhenAll(tasks);
                    await GetFilesAsync(folder);
                    CurrentFolder = folder;
                });

                IsBusy = false;
            }

        }

        private void CheckButtons()
        {
            ((AsyncCommand)CreateCommand).RaiseCanExecuteChanged();
            ((AsyncCommand)OpenCommand).RaiseCanExecuteChanged();
            ((AsyncCommand<object>)InvokeItemCommand).RaiseCanExecuteChanged();
            ((AsyncCommand<object>)ExpandingCommand).RaiseCanExecuteChanged();
            ((AsyncCommand<object>)CollapsedCommand).RaiseCanExecuteChanged();
        }

        private async Task OnCreateAsync()
        {
            if (CurrentFolder == null) return;
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(CurrentFolder.Path);
                var file = await folder.CreateFileAsync("TextFile.tmp", CreationCollisionOption.OpenIfExists);
                if (file == null) return;
                await GetFilesAsync(CurrentFolder);
                var msg = new MessageDialog("Successful creation or opening operation");
                await msg.ShowAsync();
            }
            catch(UnauthorizedAccessException)
            {
                var msg = new MessageDialog("You don't have write permission to the folder.");
                await msg.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private bool CanCreate(object obj)
        {
            return !IsBusy && CurrentFolder != null;
        }

        private bool CanOpen(object obj)
        {
            return !IsBusy && CurrentFile != null;
        }

        private async Task OnOpenAsync()
        {
            if (CurrentFile == null) return;
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(CurrentFile.Path);
                if (file == null) return;
                await Launcher.LaunchFileAsync(file);
                var msg = new MessageDialog("Successful opening operation");
                await msg.ShowAsync();
            }
            catch (FileNotFoundException ffe)
            {
                var msg = new MessageDialog(ffe.Message);
                await msg.ShowAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task OnInvokeItemAsync(object obj)
        {
            if (obj is TreeViewItemInvokedEventArgs args
                && args.InvokedItem is FolderModel folder)
            {
                await GetFilesAsync(folder);
            }
        }

        private bool CanInvokeItem(object obj)
        {
            return !IsBusy;
        }

        private async Task<FolderModel> GetFolderModelAsync(string path)
        {
            try
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(path);
                var folderModel = new FolderModel
                {
                    Name = folder.Name,
                    Path = folder.Path
                };
                await DispatcherRunAsync(
                    () =>
                    {
                        Task.WhenAll(GetSubDirectoriesAsync(folderModel), GetFilesAsync(folderModel));
                    });
                return folderModel;
            }
            catch (Exception)
            {
            }
            return null;
        }

        public async Task InitAsync()
        {
            if (DesignMode.DesignMode2Enabled)
            {
                return;
            }

            try
            {
                IsBusy = true;
                await DispatcherRunAsync(
                    async () => 
                    {
                        var drives = await GetInternalDrivesAsync();
                        var tasks = from drive in drives
                                    select GetFolderModelAsync(drive);
                        var results = await Task.WhenAll(tasks);
                        Folders = results.ToList();
                    });
                IsBusy = false;
            }
            catch (UnauthorizedAccessException)
            {
                //https://docs.microsoft.com/en-us/windows/uwp/files/file-access-permissions
                MessageDialog message = new MessageDialog("Please allow the app to access file systems.");
                _ = await message.ShowAsync();
                //https://docs.microsoft.com/en-us/windows/uwp/launch-resume/launch-settings-app
                _ = await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            IsBusy = false;
        }

        private async Task GetFilesAsync(FolderModel parentFolder)
        {
            if (parentFolder == null)
            {
                return;
            }

            Debug.WriteLine($"GetFilesAsync parentFolder : {parentFolder.Path}");

            if (Files == null)
            {
                Files = new ObservableCollection<FileModel>();
            }
            Files.Clear();

            StorageFolder pFolder = await StorageFolder.GetFolderFromPathAsync(parentFolder.Path);
            IReadOnlyList<StorageFile> files = await pFolder.GetFilesAsync();
            if (files.Any() == false)
            {
                return;
            } (from f in files
               select new FileModel
               {
                   Name = f.Name,
                   Path = f.Path
               })
             .ToList()
             .ForEach(f => Files.Add(f));
        }

        /// <summary>
        /// Recursive calls to search subdirectories
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="folderNode"></param>
        /// <param name="depth"></param>
        private async Task GetSubDirectoriesAsync(FolderModel parentFolder, int depth = 0)
        {
            //search only up to 1 depth
            if (parentFolder == null || depth == 1)
            {
                return;
            }

            try
            {
                //sub-directory lookup
                Debug.WriteLine($"GetSubDirectoriesAsync parentFolder : {parentFolder.Path}");
                StorageFolder pFolder = await StorageFolder.GetFolderFromPathAsync(parentFolder.Path);
                IReadOnlyList<StorageFolder> subFolders = await pFolder.GetFoldersAsync();
                //서브 폴더가 없거나 서브폴더 갯수가 100개 이상이면 종료
                if (subFolders.Any() == false
                    || subFolders.Count > 100)
                {
                    return;
                }

                if (parentFolder.SubFolders == null)
                {
                    parentFolder.SubFolders = new ObservableCollection<FolderModel>();
                }
                parentFolder.SubFolders.Clear();

                (from subFolder in subFolders
                 select new FolderModel
                 {
                     Name = subFolder.Name,
                     Path = subFolder.Path,
                 })
                 .ToList()
                 .ForEach(async f =>
                 {
                     parentFolder.SubFolders.Add(f);
                     await GetSubDirectoriesAsync(f, depth + 1);
                 });

                //foreach (var subFolder in subFolders)
                //{
                //    var subFolderModel = new FolderModel
                //    {
                //        Name = subFolder.Name,
                //        Path = subFolder.Path,
                //    };
                //    parentFolder.SubFolders.Add(subFolderModel);
                //    await GetSubDirectoriesAsync(subFolderModel, depth + 1);
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task<string> GetDriveLetterAsync(string path)
        {
            try
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(path);
                return folder.Name;
            }
            catch (Exception)
            {
            }
            return string.Empty;
        }

        private async Task<IList<string>> GetInternalDrivesAsync()
        {
            string driveLetters = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            var drives = driveLetters.Split(",");
            //ODD 같은 드라이브 제외
            StorageFolder removableDevices = KnownFolders.RemovableDevices;
            IReadOnlyList<StorageFolder> folders = await removableDevices.GetFoldersAsync();

            var removableLetters = from rDevice in folders
                                    where string.IsNullOrEmpty(rDevice.Path) == false
                                    select rDevice.Path.Substring(0, 1).ToUpper();
            var checkDrives = drives.Except(removableLetters);
            //테스크 생성
            var tasks = from driveLetter in checkDrives
                        select GetDriveLetterAsync(driveLetter + ":");
            var results = await Task.WhenAll(tasks);
            return results.Where(r => string.IsNullOrEmpty(r) == false).ToList();
        }

        private Task DispatcherRunAsync(Windows.UI.Core.DispatchedHandler agileCallback)
        {
            return Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, agileCallback).AsTask();
        }
    }
}
