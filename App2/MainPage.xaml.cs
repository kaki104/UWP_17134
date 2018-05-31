using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DesignMode.DesignMode2Enabled) return;

            var rootFolder = await StorageFolder.GetFolderFromPathAsync(@"c:\");
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
            GetSubDirectories(rootFolder,folderNode);
            DirectoryTreeView.RootNodes.Add(folderNode);
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
            var folder = await StorageFolder.GetFolderFromPathAsync(model.Path);

            var files = await folder.GetFilesAsync();
            if (files == null || files.Any() == false) return;
            foreach (var file in files)
            {
                FileGridView.Items.Add(file.Name);
            }

            GetSubDirectories(folder, node);
            Debug.WriteLine("DirectoryTreeView_ItemInvoked");
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
    }
}
