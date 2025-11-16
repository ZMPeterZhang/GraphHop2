using System.Collections.Generic;
using System.IO;
using Eto.Forms;
using Eto.Drawing;

namespace GraphHop2.Utilities
{
    /// <summary>
    /// Utility for displaying a dialog to select one or more files from a provided list.
    /// </summary>
    public static class FileSelector
    {
        /// <summary>
        /// Shows a modeless window with the given file paths and returns immediately.
        /// Optionally replaces the prefix of each file path before "DemoModels" with the provided library path.
        /// </summary>
        /// <param name="filePaths">List of full file paths to display.</param>
        /// <param name="replaceDemoModelPrefix">If true, replace the prefix before "DemoModels" with the provided library path.</param>
        /// <param name="libraryPath">The library path to use for replacement. Required if replaceDemoModelPrefix is true.</param>
        /// <returns>List of selected file paths (full paths). Empty if none selected.</returns>
        public static List<string> SelectFiles(List<string> filePaths, bool replaceDemoModelPrefix = false, string libraryPath = null)
        {
            // Filter out null or empty file paths
            var validFiles = filePaths?.FindAll(f => !string.IsNullOrWhiteSpace(f)) ?? new List<string>();

            if (replaceDemoModelPrefix)
            {
                if (string.IsNullOrWhiteSpace(libraryPath))
                    throw new System.ArgumentException("libraryPath must be provided when replaceDemoModelPrefix is true.");

                for (int i = 0; i < validFiles.Count; i++)
                {
                    var idx = validFiles[i].IndexOf("DemoModels", System.StringComparison.OrdinalIgnoreCase);
                    if (idx > 0)
                    {
                        // Remove everything before "DemoModels"
                        string subPath = validFiles[i].Substring(idx);
                        // Combine with library path
                        validFiles[i] = Path.Combine(libraryPath, subPath.Substring("DemoModels".Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                        // Remove trailing semicolon if present
                        validFiles[i] = validFiles[i].TrimEnd(';');
                    }
                    else
                    {
                        // Remove trailing semicolon if present
                        validFiles[i] = validFiles[i].TrimEnd(';');
                    }
                }
            }
            else
            {
                // Remove trailing semicolons if present
                for (int i = 0; i < validFiles.Count; i++)
                    validFiles[i] = validFiles[i].TrimEnd(';');
            }

            var form = new EtoGhFileSelector(validFiles);
            // Bring the window to the front after showing
            form.Show();
            form.BringToFront();
            // Since modeless, you can't return the result immediately.
            // You may want to provide a callback or event to handle the result asynchronously.
            return new List<string>(); // Or handle result via event/callback
        }

        private class EtoGhFileSelector : Form
        {
            private readonly List<string> _files;
            private readonly ListBox _listBox;
            private readonly Label _detailsLabel;
            private readonly Label _statusLabel;

            public List<string> Result { get; private set; } = new List<string>();

            public EtoGhFileSelector(List<string> files)
            {
                _files = files;

                Title = "GRAPHOP2 - Select a Grasshopper File";
                Padding = new Padding(10);
                Width = 400;
                Height = 600;
                Resizable = true;

                // Title and subtitle UI elements
                var titleLabel = new Label
                {
                    Text = "GRAPHOP2",
                    Font = new Font(SystemFont.Bold, 18),
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextColor = Colors.DarkBlue
                };

                var subtitleLabel = new Label
                {
                    Text = "Here are what we found to be similar to your reference",
                    Font = new Font(SystemFont.Default, 11),
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextColor = Colors.Gray
                };

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
                    Result = new List<string>();
                    Close();
                };

                Content = new StackLayout
                {
                    Spacing = 5,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        // Title and subtitle at the top
                        titleLabel,
                        subtitleLabel,
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
                        },
                        // Buttons at the bottom, full width
                        new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalContentAlignment = HorizontalAlignment.Right,
                            Spacing = 10,
                            Items = { openButton, cancelButton }
                        }
                    }
                };
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

                Result = new List<string> { selectedFilePath };
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
    }
}