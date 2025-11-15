using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rhino;
using Rhino.UI;

public class GrasshopperFilePreview
{
    public static void Run()
    {
        string folderPath;
        using (var folderDialog = new FolderBrowserDialog())
        {
            folderDialog.Description = "Select Folder with Grasshopper Files";
            if (folderDialog.ShowDialog() != DialogResult.OK)
            {
                RhinoApp.WriteLine("No folder selected. Exiting script.");
                return;
            }
            folderPath = folderDialog.SelectedPath;
        }

        if (string.IsNullOrEmpty(folderPath))
        {
            RhinoApp.WriteLine("No folder selected. Exiting script.");
            return;
        }

        var ghFiles = Directory.GetFiles(folderPath, "*.gh")
                               .Concat(Directory.GetFiles(folderPath, "*.ghx"))
                               .ToList();

        if (ghFiles.Count == 0)
        {
            RhinoApp.WriteLine("No Grasshopper files (.gh, .ghx) found in the selected folder.");
            return;
        }

        RhinoApp.WriteLine("Grasshopper files found:");
        for (int i = 0; i < ghFiles.Count; i++)
        {
            RhinoApp.WriteLine($"{i + 1}: {Path.GetFileName(ghFiles[i])}");
        }

        int fileIndex = -1;
        var rc = Rhino.Input.RhinoGet.GetInteger("Enter the number of the file to preview:", false, ref fileIndex);

        if (rc != Rhino.Commands.Result.Success || fileIndex < 1 || fileIndex > ghFiles.Count)
        {
            RhinoApp.WriteLine("--- Debug Info ---");
            RhinoApp.WriteLine($"Input result: {rc}");
            RhinoApp.WriteLine($"Entered number: {fileIndex}");
            RhinoApp.WriteLine($"Number of files found: {ghFiles.Count}");
            RhinoApp.WriteLine($"Valid range is 1 to {ghFiles.Count}");
            RhinoApp.WriteLine("--------------------");
            RhinoApp.WriteLine("Invalid selection. Exiting script.");
            return;
        }

        string selectedFile = ghFiles[fileIndex - 1];
        OpenFile(selectedFile);
    }

    private static void OpenFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            RhinoApp.WriteLine($"File not found: {filePath}");
            return;
        }

        // This command should open the file, which will also open the Grasshopper editor window.
        string cmd = $"-_Grasshopper _Document _Open \"{filePath}\" _Enter";
        RhinoApp.RunScript(cmd, true);

        RhinoApp.WriteLine($"Sent command to open: {Path.GetFileName(filePath)}");
    }
}

// To run this script in Rhino, you would typically call:
// GrasshopperFilePreview.Run();

GrasshopperFilePreview.Run();
