using System.Collections.Generic;
using Grasshopper.Kernel;

namespace GraphHop2.Utilities
{
    public static class SelectionToGraphUtility
    {
        /// <summary>
        /// Returns a list of tuples representing all connections (source, target) for the selected objects.
        /// Each tuple contains the source and target IGH_DocumentObject.
        /// </summary>
        public static List<(IGH_DocumentObject, IGH_DocumentObject)> GetConnections(IEnumerable<IGH_DocumentObject> selectedObjects)
        {
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

            return connections;
        }
    }
}