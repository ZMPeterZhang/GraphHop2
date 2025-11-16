using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace GraphHop2.Utilities
{
    public static class SelectionToInputUtility
    {
        /// <summary>
        /// Returns a set of input-like IGH_DocumentObject from the selection.
        /// Input-like means:
        /// - No input parameters at all (e.g., sliders, panels, etc.)
        /// - All input params have no sources (unconnected)
        /// - At least one input param is connected to a source outside the selection
        /// </summary>
        public static HashSet<IGH_DocumentObject> GetInputLikeObjects(IEnumerable<IGH_DocumentObject> selectedObjects)
        {
            var selectedSet = new HashSet<IGH_DocumentObject>(selectedObjects);
            var inputLikeObjects = new HashSet<IGH_DocumentObject>();

            foreach (IGH_DocumentObject obj in selectedSet)
            {
                var compObj = obj as IGH_Component;
                var paramObj = obj as IGH_Param;
                bool isInputLike = false;

                if (compObj != null)
                {
                    if (compObj.Params.Input.Count == 0)
                    {
                        isInputLike = true;
                    }
                    else
                    {
                        bool allInputsUnconnected = compObj.Params.Input.All(p => p.Sources.Count == 0);
                        if (allInputsUnconnected)
                        {
                            isInputLike = true;
                        }
                        else
                        {
                            foreach (var inputParam in compObj.Params.Input)
                            {
                                foreach (IGH_Param source in inputParam.Sources)
                                {
                                    IGH_DocumentObject sourceComponent = source.Attributes.GetTopLevel.DocObject;
                                    if (!selectedSet.Contains(sourceComponent))
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
                    if (paramObj.Sources.Count == 0)
                    {
                        isInputLike = true;
                    }
                    else
                    {
                        foreach (IGH_Param source in paramObj.Sources)
                        {
                            IGH_DocumentObject sourceComponent = source.Attributes.GetTopLevel.DocObject;
                            if (!selectedSet.Contains(sourceComponent))
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

            return inputLikeObjects;
        }
    }
}