//
// !-r "Eto.dll"
// !-r "Rhino.UI.dll"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhino;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

public class GrasshopperFileLister
{
    public static void Run()
    {
        var folderDialog = new SelectFolderDialog();
        if (folderDialog.ShowDialog(Rhino.UI.RhinoEtoApp.MainWindow) != DialogResult.Ok)
        {
            RhinoApp.WriteLine("No folder selected. Exiting script.");
            return;
        }
        string folderPath = folderDialog.Directory;

        var ghFiles = Directory.GetFiles(folderPath, "*.gh")
                               .Concat(Directory.GetFiles(folderPath, "*.ghx"))
                               .ToList();

        if (ghFiles.Count == 0)
        {
            RhinoApp.WriteLine("No Grasshopper files (.gh, .ghx) found in the selected folder.");
            return;
        }

        var etoForm = new EtoFileSelector(ghFiles);
        etoForm.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
    }
}

public class EtoFileSelector : Dialog
{
    private readonly List<string> _files;
    private readonly ListBox _listBox;
    private readonly Label _detailsLabel;
    private readonly Label _statusLabel;
    
    public EtoFileSelector(List<string> files)
    {
        _files = files;
        
        Title = "Select a Grasshopper File";
        Padding = new Padding(10);
        
        _listBox = new ListBox
        {
            Height = 240,
            Font = new Font(SystemFont.Default, 11)
        };
        _listBox.SelectedIndexChanged += (sender, e) => UpdateDetails();
        files.ForEach(f => _listBox.Items.Add(Path.GetFileName(f)));
        
        _detailsLabel = new Label
        {
            Text = "Select a file to preview its details.",
            TextColor = Colors.Gray,
            Wrap = WrapMode.Word
        };
        
        _statusLabel = new Label
        {
            Text = "Solver will be disabled before opening a file.",
            TextColor = Colors.Gray
        };

        var openButton = new Button { Text = "Open" };
        openButton.Click += (sender, e) =>
        {
            TryOpenSelected();
        };

        var cancelButton = new Button { Text = "Cancel" };
        cancelButton.Click += (sender, e) =>
        {
            Close();
        };

        Content = new StackLayout
        {
            Spacing = 5,
            Items =
            {
                new Label { Text = "Please select a file to open:" },
                new Panel { Content = _listBox, Padding = new Padding(0,0,0,6) },
                new GroupBox
                {
                    Text = "Details",
                    Content = new StackLayout
                    {
                        Padding = new Padding(6),
                        Items = { _detailsLabel }
                    }
                },
                new GroupBox
                {
                    Text = "Status",
                    Content = new StackLayout
                    {
                        Padding = new Padding(6),
                        Items = { _statusLabel }
                    }
                }
            }
        };
        
        PositiveButtons.Add(openButton);
        NegativeButtons.Add(cancelButton);
        
        DefaultButton = openButton;
        AbortButton = cancelButton;
    }
    
    private void TryOpenSelected()
{
        if (_listBox.SelectedIndex == -1)
        {
            MessageBox.Show("Please select a file.", MessageBoxType.Information);
            return;
        }
        
        string selectedFilePath = _files[_listBox.SelectedIndex];
        if (OpenFile(selectedFilePath, out string message, out bool success))
        {
            _statusLabel.TextColor = Colors.DarkGreen;
        }
        else
        {
            _statusLabel.TextColor = Colors.IndianRed;
        }
        _statusLabel.Text = message;
    }
    
    private void UpdateDetails()
    {
        if (_listBox.SelectedIndex == -1)
        {
            _detailsLabel.Text = "Select a file to preview its details.";
            _detailsLabel.TextColor = Colors.Gray;
            return;
        }
        
        string filePath = _files[_listBox.SelectedIndex];
        var info = new FileInfo(filePath);
        _detailsLabel.TextColor = Colors.Black;
        _detailsLabel.Text = $"Name: {info.Name}\nModified: {info.LastWriteTime:g}\nLocation: {info.DirectoryName}";
    }
    
    private bool OpenFile(string filePath, out string message, out bool success)
    {
        success = false;
        
        if (!File.Exists(filePath))
    {
            message = $"File not found: {filePath}";
            return false;
    }

    // Load Grasshopper if not already running
    Rhino.RhinoApp.RunScript("_Grasshopper _Load", false);
        // Disable solver before opening per request
        Rhino.RhinoApp.RunScript("-_Grasshopper _Solver _Disable _Enter", false);

    // Open the file via the command-line API, under the Document menu
    string cmd = $"-_Grasshopper _Document _Open \"{filePath}\" _Enter";
        bool opened = Rhino.RhinoApp.RunScript(cmd, false);

        if (opened)
        {
            success = true;
            message = $"Opened: {Path.GetFileName(filePath)} (solver disabled)";
            Rhino.RhinoApp.WriteLine(message);
        }
        else
        {
            message = $"Failed to open: {Path.GetFileName(filePath)}";
            Rhino.RhinoApp.WriteLine(message);
        }
        
        return opened;
}

}

// To run this script in Rhino, you would typically call:
GrasshopperFileLister.Run();
