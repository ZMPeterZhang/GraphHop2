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
    public EtoFileSelector(List<string> files)
    {
        Title = "Select a Grasshopper File";
        Padding = new Padding(10);
        
        var listBox = new ListBox();
        files.ForEach(f => listBox.Items.Add(Path.GetFileName(f)));

        var openButton = new Button { Text = "Open" };
        openButton.Click += (sender, e) =>
        {
            if (listBox.SelectedIndex != -1)
            {
                string selectedFilePath = files[listBox.SelectedIndex];
                OpenFile(selectedFilePath);
                Close();
            }
            else
            {
                MessageBox.Show("Please select a file.", MessageBoxType.Information);
            }
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
                listBox,
            }
        };
        
        PositiveButtons.Add(openButton);
        NegativeButtons.Add(cancelButton);
        
        DefaultButton = openButton;
        AbortButton = cancelButton;
    }
    
private void OpenFile(string filePath)
{
    if (!File.Exists(filePath))
    {
        Rhino.RhinoApp.WriteLine($"File not found: {filePath}");
        return;
    }

    // Load Grasshopper if not already running
    Rhino.RhinoApp.RunScript("_Grasshopper _Load", false);

    // Open the file via the command-line API, under the Document menu
    string cmd = $"-_Grasshopper _Document _Open \"{filePath}\" _Enter";
    Rhino.RhinoApp.RunScript(cmd, false);

    Rhino.RhinoApp.WriteLine($"Opened: {Path.GetFileName(filePath)}");
}

}

// To run this script in Rhino, you would typically call:
GrasshopperFileLister.Run();
