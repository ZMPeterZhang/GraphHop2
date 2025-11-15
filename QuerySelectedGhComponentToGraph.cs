using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;

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

    // Build a set of selected component objects for quick lookup
    var selectedComponents = new HashSet<IGH_DocumentObject>(
        selectedObjects.OfType<IGH_Component>().Cast<IGH_DocumentObject>()
    );

    var connections = new List<(IGH_DocumentObject, IGH_DocumentObject)>();

    foreach (var obj in selectedObjects.OfType<IGH_Component>())
    {
        foreach (var output in obj.Params.Output)
        {
            foreach (var recipient in output.Recipients)
            {
                var targetComponent = recipient.Attributes.GetTopLevel.DocObject;
                if (targetComponent != null && selectedComponents.Contains(targetComponent))
                {
                    connections.Add((obj, targetComponent));
                }
            }
        }
    }

    if (connections.Count == 0)
    {
        RhinoApp.WriteLine("No connections found between selected components.");
    }
    else
    {
        RhinoApp.WriteLine("Connections (source -> target):");
        foreach (var pair in connections)
        {
            string srcName = pair.Item1?.GetType().Name + ": " + (pair.Item1 as IGH_Component)?.Name;
            string tgtName = pair.Item2?.GetType().Name + ": " + (pair.Item2 as IGH_Component)?.Name;
            RhinoApp.WriteLine($"({srcName}) -> ({tgtName})");
        }
    }
}

RunScript();