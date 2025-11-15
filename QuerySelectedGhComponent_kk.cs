using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

void RunScript()
{
    // Check if Grasshopper is loaded by checking ActiveCanvas
    if (Grasshopper.Instances.ActiveCanvas == null)
    {
        RhinoApp.WriteLine("Grasshopper is not loaded or no canvas is active!");
        return;
    }

    // Get the active Grasshopper document
    GH_Document ghDoc = Grasshopper.Instances.ActiveCanvas.Document;
    
    if (ghDoc == null)
    {
        RhinoApp.WriteLine("No active Grasshopper document found!");
        return;
    }

    // Get selected objects from Grasshopper
    var selectedObjects = ghDoc.SelectedObjects();
    
    if (selectedObjects.Count == 0)
    {
        RhinoApp.WriteLine("No Grasshopper components selected!");
        return;
    }

    RhinoApp.WriteLine($"Found {selectedObjects.Count} selected object(s):");
    RhinoApp.WriteLine("---");

    // Process each selected object
    foreach (IGH_DocumentObject obj in selectedObjects)
    {
        // Check if it's a component
        if (obj is IGH_Component component)
        {
            RhinoApp.WriteLine($"Component: {component.Name}");
            RhinoApp.WriteLine($"  Category: {component.Category}");
            RhinoApp.WriteLine($"  Subcategory: {component.SubCategory}");
            RhinoApp.WriteLine($"  Instance GUID: {component.InstanceGuid}");
            
            // Get input parameters and their connections
            RhinoApp.WriteLine($"  Input Parameters: {component.Params.Input.Count}");
            foreach (var param in component.Params.Input)
            {
                RhinoApp.WriteLine($"    - {param.Name} ({param.TypeName})");
                
                // Get connections TO this input (sources)
                if (param.Sources.Count > 0)
                {
                    RhinoApp.WriteLine($"      Connected from:");
                    foreach (IGH_Param source in param.Sources)
                    {
                        // Get the parent component of the source parameter
                        IGH_DocumentObject sourceComponent = source.Attributes.GetTopLevel.DocObject;
                        RhinoApp.WriteLine($"        • {sourceComponent.Name} -> {source.Name}");
                    }
                }
                else
                {
                    RhinoApp.WriteLine($"      (No connections)");
                }
            }
            
            // Get output parameters and their connections
            RhinoApp.WriteLine($"  Output Parameters: {component.Params.Output.Count}");
            foreach (var param in component.Params.Output)
            {
                RhinoApp.WriteLine($"    - {param.Name} ({param.TypeName})");
                
                // Get connections FROM this output (recipients)
                if (param.Recipients.Count > 0)
                {
                    RhinoApp.WriteLine($"      Connected to:");
                    foreach (IGH_Param recipient in param.Recipients)
                    {
                        // Get the parent component of the recipient parameter
                        IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
                        RhinoApp.WriteLine($"        • {param.Name} -> {recipientComponent.Name}.{recipient.Name}");
                    }
                }
                else
                {
                    RhinoApp.WriteLine($"      (No connections)");
                }
            }
            
            // Access component data (if needed)
            if (component.Params.Output.Count > 0)
            {
                var firstOutput = component.Params.Output[0];
                if (firstOutput.VolatileDataCount > 0)
                {
                    RhinoApp.WriteLine($"  First output has {firstOutput.VolatileDataCount} data items");
                }
            }
            
            RhinoApp.WriteLine("---");
        }
        // Check if it's a parameter
        else if (obj is IGH_Param param)
        {
            RhinoApp.WriteLine($"Parameter: {param.Name}");
            RhinoApp.WriteLine($"  Type: {param.TypeName}");
            RhinoApp.WriteLine($"  Data Count: {param.VolatileDataCount}");
            
            // Get connections for this parameter
            if (param.Sources.Count > 0)
            {
                RhinoApp.WriteLine($"  Sources:");
                foreach (IGH_Param source in param.Sources)
                {
                    IGH_DocumentObject sourceComponent = source.Attributes.GetTopLevel.DocObject;
                    RhinoApp.WriteLine($"    • {sourceComponent.Name} -> {source.Name}");
                }
            }
            
            if (param.Recipients.Count > 0)
            {
                RhinoApp.WriteLine($"  Recipients:");
                foreach (IGH_Param recipient in param.Recipients)
                {
                    IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
                    RhinoApp.WriteLine($"    • {param.Name} -> {recipientComponent.Name}.{recipient.Name}");
                }
            }
        }
        else
        {
            RhinoApp.WriteLine($"Object: {obj.GetType().Name}");
        }
    }
}

RunScript();