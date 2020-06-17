using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFolder _selectedFolder;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DesignMode.DesignMode2Enabled) return;
            try
            {
                var rootFolder = await StorageFolder.GetFolderFromPathAsync(@"C:\");
                if (rootFolder == null) return;
                var folderNode = new TreeViewNode
                {
                    Content = new DirectoryModel
                    {
                        Name = rootFolder.Name,
                        Path = rootFolder.Path,
                        HasSubDirectory = true
                    }
                };
                GetSubDirectories(rootFolder, folderNode);
                DirectoryTreeView.RootNodes.Add(folderNode);
            }
            catch (UnauthorizedAccessException uae)
            {
                //https://docs.microsoft.com/en-us/windows/uwp/files/file-access-permissions
                //https://docs.microsoft.com/en-us/windows/uwp/launch-resume/launch-settings-app
                var message = new MessageDialog("Please allow the app to access all file systems.");
                _ = await message.ShowAsync();
                _ = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private async void GetSubDirectories(StorageFolder folder, TreeViewNode folderNode, int depth = 0)
        {
            try
            {
                if (depth > 1) return;
                var subDirs = await folder.GetFoldersAsync();
                if (subDirs.Any() == false || folderNode.Children.Any()) return;
                foreach (var subDir in subDirs)
                {
                    var subDirNode = new TreeViewNode
                    {
                        Content = new DirectoryModel
                        {
                            Name = subDir.Name,
                            Path = subDir.Path,
                            HasSubDirectory = false
                        },
                    };
                    GetSubDirectories(subDir, subDirNode,depth + 1);
                    folderNode.Children.Add(subDirNode);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void DirectoryTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            if (!(args.InvokedItem is TreeViewNode node)) return;
            if (FileGridView.Items == null) return;
            FileGridView.Items.Clear();

            if (!(node.Content is DirectoryModel model)) return;
            _selectedFolder = await StorageFolder.GetFolderFromPathAsync(model.Path);
            if (_selectedFolder == null) return;
            var files = await _selectedFolder.GetFilesAsync();
            if (files == null || files.Any() == false) return;
            foreach (var file in files)
            {
                FileGridView.Items.Add(file.Name);
            }
            GetSubDirectories(_selectedFolder, node);
        }

        //private IList<string> GetListFromNode(TreeViewNode node)
        //{
        //    if (node == null) return null;
        //    if (node.Parent is TreeViewNode parentNode
        //        && parentNode.Content != null)
        //    {
        //        var parents = GetListFromNode(parentNode);
        //        var nodeString = node.Content.ToString();
        //        parents.Add(nodeString);
        //        return parents;
        //    }
        //    var list = new List<string>
        //    {
        //        node.Content.ToString()
        //    };
        //    return list;
        //}
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            switch (button.Content)
            {
                case "Create":
                    CreateFileInSelectedFolder();
                    break;
                case "Open":
                    OpenFile();
                    break;
            }
        }

        private async void OpenFile()
        {
            if (_selectedFolder == null) return;
            try
            {
                var file = await _selectedFolder.GetFileAsync("TextFile.tmp");
                if (file == null) return;
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

        private async void CreateFileInSelectedFolder()
        {
            if (_selectedFolder == null) return;
            var file = await _selectedFolder.CreateFileAsync("TextFile.tmp", CreationCollisionOption.OpenIfExists);
            if (file == null) return;
            var msg = new MessageDialog("Successful creation or opening operation");
            await msg.ShowAsync();
        }
    }
}
