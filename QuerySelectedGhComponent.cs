// #! csharp
#r "nuget: Neo4j.Driver, 5.28.3"
using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.GUI.Canvas;

public class QuerySelectedGhComponent : Command
{
    public override string EnglishName => "QuerySelectedGhComponent";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        // Get the Grasshopper plugin instance
        var gh = Grasshopper.Instances.ActiveCanvas;
        if (gh == null)
        {
            RhinoApp.WriteLine("Grasshopper is not running.");
            return Result.Failure;
        }

        var ghDoc = gh.Document;
        if (ghDoc == null)
        {
            RhinoApp.WriteLine("No Grasshopper document is open.");
            return Result.Failure;
        }

        // Get all selected objects in the Grasshopper document
        var selectedObjects = ghDoc.SelectedObjects();
        if (selectedObjects.Count == 0)
        {
            RhinoApp.WriteLine("No components are selected in Grasshopper.");
            return Result.Success;
        }

        RhinoApp.WriteLine("Selected Grasshopper components:");
        foreach (var obj in selectedObjects.OfType<IGH_Component>())
        {
            RhinoApp.WriteLine($"- {obj.Name} (ID: {obj.InstanceGuid})");
        }

        return Result.Success;
    }
}
