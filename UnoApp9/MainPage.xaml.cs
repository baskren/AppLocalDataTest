using System.Text;
using Uno.Extensions;

namespace UnoApp9;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        HelloTextBlock.Text = $"DirectorySeparator: [{Path.DirectorySeparatorChar}] Alt:[{Path.AltDirectorySeparatorChar}]";
        HelloTextBlock.TextWrapping = TextWrapping.WrapWholeWords;

        /*
        var path = "this/is/the/path/file.A.text";
        path = Path.Combine("/", path);
        HelloTextBlock.Text = $"PATH: [{path}]  DirectoryName:[{Path.GetDirectoryName(path)}] FileName:[{Path.GetFileName(path)}] [{Path.GetFileName(path).Split('.').First()}] Extension:[{Path.GetExtension(path)}]";
        */

        var uri = new Uri("file://pizza/pie/cheese.zip?duplicate");
        var rootUri = new Uri("file://pizza/");
        HelloTextBlock.Text = $"URI: [{uri}]" +
            $"\n\t SCHEME:[{uri.Scheme}]" +
            $" \n\t host:[{uri.Host}]" +
            $" \n\t hostNameType:[{uri.HostNameType}]" +
            $" \n\t AbsolutePath:[{uri.AbsolutePath}] " +
            $"\n\t Suffix [{Path.GetExtension(uri.AbsolutePath)}]" +
            $"\n\t AbsoluteUri:[{uri.AbsoluteUri}]" +
            $"\n\t localPath:[{uri.LocalPath}] " +
            $"\n\t Segments:[{string.Join(", ", uri.Segments)}] " +
            $"\n\t Fragment:[{uri.Fragment}] " +
            $"\n\t Authority:[{uri.Authority}] " +
            $"\n\t Port:[{uri.Port}]" +
            $"\n\t DnsSafeHost[{uri.DnsSafeHost}]" +
            $"\n\t IsFile:[{uri.IsFile}]" +
            $"\n\t IsAbsoluteUri[{uri.IsAbsoluteUri}]" +
            $"\n\t IsDefaultPort[{uri.IsDefaultPort}]" +
            $"\n\t IsLoopback[{uri.IsLoopback}]" +
            $"\n\t IsUnc[{uri.IsUnc}]" +
            $"\n\t IsWellFormedOriginalString[{uri.IsWellFormedOriginalString()}]" +
            $"\n\t GetLeftPart(Scheme)[{uri.GetLeftPart(UriPartial.Scheme)}]" +
            $"\n\t GetLeftPart(Authority)[{uri.GetLeftPart(UriPartial.Authority)}]" +
            $"\n\t Path[{uri.GetLeftPart(UriPartial.Path)}]" +
            $"\n\t Query[{uri.GetLeftPart(UriPartial.Query)}]" +
            $"\n\t PathAndQuery[{uri.PathAndQuery}]" +
            $"\n\t Query[{uri.Query}]" +
            $"\n\t MakeRelativeUri[{rootUri.MakeRelativeUri(uri)}]";



        var installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
        /*
        HelloTextBlock.Text = 
        $"ApplicationDataPath = [{Windows.Storage.ApplicationData.Current.LocalFolder.Path}]\n" +
        $"ApplicationCachePath = [{Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path}]\n" +
        $"TemporaryStoragePath = [{Windows.Storage.ApplicationData.Current.TemporaryFolder.Path}]\n";
        */


    }


    async Task<string> EnumerateStorageFolderAsync(StorageFolder folder, int depth = 0)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Tabs(depth)}{folder.Name}/");
        depth++;

        IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
        foreach (StorageFile file in files)
            sb.AppendLine($"{Tabs(depth)}{file.Name}/");

        IReadOnlyList<StorageFolder> folders = await folder.GetFoldersAsync();
        foreach (StorageFolder subFolder in folders)
            sb.Append(await EnumerateStorageFolderAsync(subFolder, depth));

        return sb.ToString();
    }

    string Tabs(int count)
        => new string('\t', count);

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        var asm = this.GetType().Assembly;

        var cacheFontsFolderPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "P42.Utils.Cache", asm.GetName().Name, "FONTS");
        DirectoryExtensions.Assure(cacheFontsFolderPath);

        var fontFilePath = Path.Combine(cacheFontsFolderPath, "SixtyFour.ttf");

        using var resourceStream = asm.GetManifestResourceStream("UnoApp9.Resources.Fonts.Sixtyfour[BLED,SCAN].ttf");
        using var outputStream = File.OpenWrite(fontFilePath);
        await resourceStream.CopyToAsync(outputStream);
        outputStream.Close();


        var localPathFragment = fontFilePath.Replace(ApplicationData.Current.LocalFolder.Path, "").Replace('\\','/').Trim('/');
        HelloTextBlock.FontFamily = new FontFamily($"ms-appdata:///local/{localPathFragment}#SixtyFour Touching");
        HelloTextBlock.Text = await EnumerateStorageFolderAsync(ApplicationData.Current.LocalFolder);
        //HelloTextBlock.Text = await EnumerateStorageFolderAsync(Windows.ApplicationModel.Package.Current.InstalledLocation);
    }

    private async void Html_Button_Click(object sender, RoutedEventArgs e)
    {
        var asm = this.GetType().Assembly;

        var cacheHtmlFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "P42.Utils.Cache", asm.GetName().Name, "HTML");
        DirectoryExtensions.Assure(cacheHtmlFolderPath);

        var htmlPackageFilePath = Path.Combine(cacheHtmlFolderPath, "CltInstall.zip");

        using var resourceStream = asm.GetManifestResourceStream("UnoApp9.Resources.Html.CltInstall.zip");
        using var outputStream = File.OpenWrite(htmlPackageFilePath);
        await resourceStream.CopyToAsync(outputStream);
        outputStream.Close();

        var htmlContentFolderPath = htmlPackageFilePath[..^".zip".Length];
        System.IO.Compression.ZipFile.ExtractToDirectory(htmlPackageFilePath, htmlContentFolderPath, true);

        await WebView.EnsureCoreWebView2Async();

        var localPathFragment = htmlContentFolderPath.Replace(ApplicationData.Current.LocalFolder.Path, "").Replace('\\','/').Trim('/');
        WebView.Source = new Uri($"ms-appdata:///local/{localPathFragment}/index.html");  // does not work!
        // WebView.Source = new Uri($"file://{htmlContentFolderPath}/index.html");  // works!
        //WebView.Source = new Uri("https://slashdot.org");

        //WebView.NavigateToString("<html><body><p>Hello world!</p></body></html>");
    }


}


static class DirectoryExtensions
{
    public static DirectoryInfo Assure(string path)
    {
        if (!Path.IsPathRooted(path))
            throw new Exception("Path is not rooted");
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        path = "/";
        foreach (var part in parts)
        {
            path = Path.Combine(path, part);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        return new DirectoryInfo(path);
    }
}
