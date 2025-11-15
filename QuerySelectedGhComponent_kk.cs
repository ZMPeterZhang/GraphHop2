using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

// More detailed tuple: (SourceComponent, SourceParam, SourceGUID, TargetComponent, TargetParam, TargetGUID)
List<(string sourceComp, string sourceParam, Guid sourceGuid, string targetComp, string targetParam, Guid targetGuid)> connections = 
    new List<(string, string, Guid, string, string, Guid)>();

void RunScript()
{
    connections.Clear();
    
    if (Grasshopper.Instances.ActiveCanvas == null)
    {
        RhinoApp.WriteLine("Grasshopper is not loaded!");
        return;
    }

    GH_Document ghDoc = Grasshopper.Instances.ActiveCanvas.Document;
    if (ghDoc == null) return;

    var selectedObjects = ghDoc.SelectedObjects();
    if (selectedObjects.Count == 0) return;

    foreach (IGH_DocumentObject obj in selectedObjects)
    {
        if (obj is IGH_Component component)
        {
            // Input connections
            foreach (var inputParam in component.Params.Input)
            {
                foreach (IGH_Param source in inputParam.Sources)
                {
                    IGH_DocumentObject sourceComponent = source.Attributes.GetTopLevel.DocObject;
                    connections.Add((
                        sourceComponent.Name,
                        source.Name,
                        sourceComponent.InstanceGuid,
                        component.Name,
                        inputParam.Name,
                        component.InstanceGuid
                    ));
                }
            }
            
            // Output connections
            foreach (var outputParam in component.Params.Output)
            {
                foreach (IGH_Param recipient in outputParam.Recipients)
                {
                    IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
                    connections.Add((
                        component.Name,
                        outputParam.Name,
                        component.InstanceGuid,
                        recipientComponent.Name,
                        recipient.Name,
                        recipientComponent.InstanceGuid
                    ));
                }
            }
        }
    }
    
    // Print for verification
    foreach (var conn in connections)
    {
        RhinoApp.WriteLine($"{conn.sourceComp}.{conn.sourceParam} -> {conn.targetComp}.{conn.targetParam}");
    }
    RhinoApp.WriteLine("--");
}

RunScript();