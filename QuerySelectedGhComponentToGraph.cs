using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;

//Query list of connection to list of tuples (source component document object, target component document object)

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

    var connections = new List<(IGH_DocumentObject, IGH_DocumentObject)>();

    foreach (IGH_DocumentObject obj in selectedObjects)
    {
        if (obj is IGH_Component component)
        {
            // Input connections (sources to this component)
            foreach (var inputParam in component.Params.Input)
            {
                foreach (IGH_Param source in inputParam.Sources)
                {
                    IGH_DocumentObject sourceComponent = source.Attributes.GetTopLevel.DocObject;
                    if (sourceComponent != null)
                        connections.Add((sourceComponent, obj));
                }
            }
            // Output connections (this component to recipients)
            foreach (var outputParam in component.Params.Output)
            {
                foreach (IGH_Param recipient in outputParam.Recipients)
                {
                    IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
                    if (recipientComponent != null)
                        connections.Add((obj, recipientComponent));
                }
            }
        }
    }

    if (connections.Count == 0)
    {
        RhinoApp.WriteLine("No connections found for selected components.");
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