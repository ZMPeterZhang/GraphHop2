using System;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;
using GraphHop2.Utilities;

//Query list of output like components within the selection
//Components (or params) with no output parameters,
//Or all outputs are unconnected,
//Or at least one output is connected to a recipient outside the selection.

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

    var outputLikeObjects = SelectionToOutputUtility.GetOutputLikeObjects(selectedObjects);

    if (outputLikeObjects.Count == 0)
    {
        RhinoApp.WriteLine("No output-like components found in selection.");
    }
    else
    {
        RhinoApp.WriteLine("Output-like components in selection (zero or external output):");
        foreach (var obj in outputLikeObjects)
        {
            string name = obj.GetType().Name + ": " + (obj as IGH_Component)?.Name;
            RhinoApp.WriteLine(name);
        }
    }
}

RunScript();