using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;
using GraphHop2.Utilities;

//Query list of "input", components within the selection
//Components (or params) with no input parameters,
//Or all inputs are unconnected,
//Or at least one input is connected to a source outside the selection.

void RunScript()
{
    if (Grasshopper.Instances.ActiveCanvas == null)
    {
        RhinoApp.WriteLine("Grasshopper is not loaded or no canvas is active!");
        return;
    }

    var ghDoc = Grasshopper.Instances.ActiveCanvas.Document;
    if (ghDoc == null)
    {
        RhinoApp.WriteLine("No active Grasshopper document found!");
        return;
    }

    var selectedObjects = ghDoc.SelectedObjects();
    if (selectedObjects.Count == 0)
    {
        RhinoApp.WriteLine("No Grasshopper components selected!");
        return;
    }

    var inputLikeObjects = SelectionToInputUtility.GetInputLikeObjects(selectedObjects);

    if (inputLikeObjects.Count == 0)
    {
        RhinoApp.WriteLine("No input-like components found in selection.");
    }
    else
    {
        RhinoApp.WriteLine("Input-like components in selection (zero or external input):");
        foreach (var obj in inputLikeObjects)
        {
            string name = obj.GetType().Name + ": " + (obj as IGH_Component)?.Name;
            RhinoApp.WriteLine(name);
        }
    }
}

RunScript();