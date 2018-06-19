using System;
using System.IO;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace KakiViewer
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!(e.Parameter is string filePath) || string.IsNullOrEmpty(filePath)) return;

            ApplicationView.GetForCurrentView().Title = filePath;

            var firstFile = await StorageFile.GetFileFromPathAsync(filePath);
            if (firstFile == null) return;

            using (var stream = await firstFile.OpenReadAsync())
            {
                var bi = new BitmapImage();
                bi.SetSource(stream);
                Image.Source = bi;
            }
        }
    }
}