using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;

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

    var outputLikeObjects = new HashSet<IGH_DocumentObject>();

    foreach (IGH_DocumentObject obj in selectedObjects)
    {
        var compObj = obj as IGH_Component;
        var paramObj = obj as IGH_Param;
        bool isOutputLike = false;

        if (compObj != null)
        {
            // Case 1: No output parameters at all (e.g., panels, sliders, etc.)
            if (compObj.Params.Output.Count == 0)
            {
                isOutputLike = true;
            }
            else
            {
                // Case 2: All output params have no recipients (unconnected)
                bool allOutputsUnconnected = compObj.Params.Output.All(p => p.Recipients.Count == 0);
                if (allOutputsUnconnected)
                {
                    isOutputLike = true;
                }
                else
                {
                    // Case 3: At least one output param is connected to a recipient outside the selection
                    foreach (var outputParam in compObj.Params.Output)
                    {
                        foreach (IGH_Param recipient in outputParam.Recipients)
                        {
                            IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
                            if (!selectedObjects.Contains(recipientComponent))
                            {
                                isOutputLike = true;
                                break;
                            }
                        }
                        if (isOutputLike) break;
                    }
                }
            }
        }
        else if (paramObj != null)
        {
            // For IGH_Param (like panels, sliders):
            if (paramObj.Recipients.Count == 0)
            {
                isOutputLike = true;
            }
            else
            {
                foreach (IGH_Param recipient in paramObj.Recipients)
                {
                    IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
                    if (!selectedObjects.Contains(recipientComponent))
                    {
                        isOutputLike = true;
                        break;
                    }
                }
            }
        }
        if (isOutputLike)
            outputLikeObjects.Add(obj);
    }

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