using System;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.UI;

// Method 1: Using RhinoScript to show an Eto form
void ShowEtoFormFromScript()
{
    // Create and show the form
    var form = new MyEtoForm();
    form.Show();
}

// Method 2: Using Rhino.UI to show in a panel
void ShowEtoFormInPanel()
{
    var form = new MyEtoForm();
    form.Show();
}

// Example Eto Form class
public class MyEtoForm : Form
{
    public MyEtoForm()
    {
        Title = "My Eto Form";
        ClientSize = new Size(400, 300);
        
        var button = new Button { Text = "Click Me" };
        button.Click += (sender, e) => 
        {
            RhinoApp.WriteLine("Button clicked!");
        };
        
        Content = new StackLayout
        {
            Items = 
            {
                new Label { Text = "Hello from Eto Form!" },
                button
            },
            Padding = 10,
            Spacing = 5
        };
    }

}

var form = new MyEtoForm();
form.Owner = RhinoEtoApp.MainWindow;
form.Show();