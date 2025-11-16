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
        
        string selectedFile = etoForm.SelectedFile;

        if (!string.IsNullOrEmpty(selectedFile))
        {
            OpenFile(selectedFile);
        }
    }

    private static void OpenFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            RhinoApp.WriteLine($"File not found: {filePath}");
            return;
        }
        
        var io = new Grasshopper.Kernel.GH_DocumentIO(filePath);
        if (io.Open())
        {
            var doc = io.Document;
            if (doc != null)
            {
                // Add the new document to the server. This makes it appear in the GH window.
                Grasshopper.Instances.DocumentServer.AddDocument(doc);
                RhinoApp.WriteLine($"Successfully opened: {Path.GetFileName(filePath)}");
                return;
            }
        }
        
        RhinoApp.WriteLine($"Failed to open: {Path.GetFileName(filePath)}");
    }
}

public class EtoFileSelector : Dialog<string>
{
    public string SelectedFile { get; private set; }

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
                SelectedFile = files[listBox.SelectedIndex];
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
            SelectedFile = null;
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
        
        // This is needed to handle button clicks from Positive/Negative buttons
        this.DefaultButton.Click += (s, e) => openButton.PerformClick();
    }
}


// To run this script in Rhino, you would typically call:
GrasshopperFileLister.Run();
