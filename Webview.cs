#r "nuget: Microsoft.Web.WebView2, 1.0.3595.46"

using Eto.Forms;
using Eto.Drawing;
using Microsoft.Web.WebView2.WinForms;
using Rhino.UI;
using Rhino.Commands;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

// Initialize WebView2 Environment
public static class WebView2Helper
{
    public static async Task<CoreWebView2Environment> InitializeEnvironmentAsync()
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "RhinoWebView2Data"
        );
        return await CoreWebView2Environment.CreateAsync(null, userDataFolder);
    }
}

// Create the Form with WebView2
public class WebView2Form : Form
{
    private WebView2 _webView;

    public WebView2Form()
    {
        Title = "WebView2 Browser";
        ClientSize = new Size(800, 600);

        // Create WebView2 control
        _webView = new WebView2();
        _webView.Dock = DockStyle.Fill;

        // FIXED: Use NativeControlHost to wrap WinForms control in Eto.Forms
        var nativeHost = new NativeControlHost();
        nativeHost.CreateNativeControl = () => 
        {
            // Create the control and return its handle
            _webView.CreateControl();
            return _webView.Handle;
        };
        Content = nativeHost;

        // Initialize when form loads
        Load += async (sender, e) => await InitializeWebView2Async();
    }

    private async Task InitializeWebView2Async()
    {
        try
        {
            var environment = await WebView2Helper.InitializeEnvironmentAsync();
            await _webView.EnsureCoreWebView2Async(environment);
            _webView.Source = new Uri("https://www.google.com");
        }
        catch (Exception ex)
        {
            Rhino.RhinoApp.WriteLine($"Error initializing WebView2: {ex.Message}");
        }
    }
}

// Command to show the form
public class ShowWebView2Command : Rhino.Commands.Command
{
    public override string EnglishName => "ShowWebView2";

    protected override Result RunCommand(Rhino.RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
        var form = new WebView2Form();
        form.Show();
        return Result.Success;
    }
}