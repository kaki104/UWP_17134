# UWP_17134
Windows SDK 17134에서 추가된 기능들 셈플 작성

. App2 (File Explorer UWP)
  - Environment
    + Visual Studio 2019 16.6 later
    + Windows 10 version 1903 later
    + UWP 
      - Target version : Windows 10, version 1903
      - Min version : Windows 10, version 1809
    + Microsoft.Xaml.Behaviors.Uwp.Managed 2.0.1 nuget
    + Microsoft.UI.Xaml 2.4.2 nuget
    + Refractored.MvvmHelpers 1.6.2 nuget package
  - BroadFileSystemAccess
    + All files that the user has access to
    + Settings > Privacy > File system > Allow access UWP app
    + Version 1803 – default is On
    + Version 1809 – default is Off
    + If you submit an app to the Store that declares this capability, you will need to supply additional descriptions of why your app needs this capability, and how it intends to use it.
    + This capability is not supported on Xbox
  - Key Point
    + Package.appxmanifest > Mouse Right Click > Open With… > XML (Text) Editor
    + Add rescap namespace
      - xmlns:rescap=http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities
    + Add rescap:Capability to th Capabilities section
      - <rescap:Capability Name="broadFileSystemAccess" />
    + Code
      - var folder = await StorageFolder.GetFolderFromPathAsync(path);
      - try-catch(UnauthorizedAccessException)
  
