using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Grasshopper;
using Grasshopper.Kernel;

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

    var inputLikeObjects = new HashSet<IGH_DocumentObject>();

    foreach (IGH_DocumentObject obj in selectedObjects)
    {
        // Only consider objects that are IGH_Param or IGH_Component
        var compObj = obj as IGH_Component;
        var paramObj = obj as IGH_Param;
        bool isInputLike = false;

        if (compObj != null)
        {
            // Case 1: No input parameters at all (e.g., sliders, panels, etc.)
            if (compObj.Params.Input.Count == 0)
            {
                isInputLike = true;
            }
            else
            {
                // Case 2: All input params have no sources (unconnected)
                bool allInputsUnconnected = compObj.Params.Input.All(p => p.Sources.Count == 0);
                if (allInputsUnconnected)
                {
                    isInputLike = true;
                }
                else
                {
                    // Case 3: At least one input param is connected to a source outside the selection
                    foreach (var inputParam in compObj.Params.Input)
                    {
                        foreach (IGH_Param source in inputParam.Sources)
                        {
                            IGH_DocumentObject sourceComponent = source.Attributes.GetTopLevel.DocObject;
                            if (!selectedObjects.Contains(sourceComponent))
                            {
                                isInputLike = true;
                                break;
                            }
                        }
                        if (isInputLike) break;
                    }
                }
            }
        }
        else if (paramObj != null)
        {
            // For IGH_Param (like panels, sliders):
            if (paramObj.Sources.Count == 0)
            {
                isInputLike = true;
            }
            else
            {
                foreach (IGH_Param source in paramObj.Sources)
                {
                    IGH_DocumentObject sourceComponent = source.Attributes.GetTopLevel.DocObject;
                    if (!selectedObjects.Contains(sourceComponent))
                    {
                        isInputLike = true;
                        break;
                    }
                }
            }
        }
        if (isInputLike)
            inputLikeObjects.Add(obj);
    }

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