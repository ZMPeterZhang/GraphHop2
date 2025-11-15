#r "nuget: Microsoft.Web.WebView2, 1.0.3595.46"

using Eto.Forms;
using Eto.Drawing;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Rhino.UI;
using Rhino.Commands;  // Added for Result type
using System;
using System.IO;
using System.Threading.Tasks;

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
        _webView.Dock = System.Windows.Forms.DockStyle.Fill;

        // Wrap it in Eto control - FIXED: Use proper Eto wrapper
        // Note: On Windows, Eto.Forms can host native WinForms controls
        var nativeControl = Eto.Forms.Platform.Instance.CreateControl(_webView);
        Content = nativeControl;

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

// Command to show the form - FIXED: Added Rhino.Commands namespace
public class ShowWebView2Command : Rhino.Commands.Command
{
    public override string EnglishName => "ShowWebView2";

    protected override Result RunCommand(Rhino.RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
        var form = new WebView2Form();
        // FIXED: Show without arguments, or use ShowModal for modal dialog
        form.Show();
        return Result.Success;  // FIXED: Now Result is recognized from Rhino.Commands
    }
}