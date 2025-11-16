using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;
using GraphHop2.Utilities;
using GraphHop2;


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

    var connections = SelectionToGraphUtility.GetConnections(selectedObjects);

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

        RhinoApp.WriteLine("START:");

        // FindPathFromInputToOutput.GetShortestPathsFromInputsToOutputs(selectedObjects);
        var searcher = new GraphHop2.Search();
        searcher.pathMatchQuery(selectedObjects);
        RhinoApp.WriteLine("END:");
    }
}

RunScript();