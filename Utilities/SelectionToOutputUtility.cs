using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace GraphHop2.Utilities
{
	public static class SelectionToOutputUtility
	{
		/// <summary>
		/// Returns a set of output-like IGH_DocumentObject from the selection.
		/// Output-like means:
		/// - No output parameters at all (e.g., sliders, panels, etc.)
		/// - All output params have no recipients (unconnected)
		/// - At least one output param is connected to a recipient outside the selection
		/// </summary>
		public static HashSet<IGH_DocumentObject> GetOutputLikeObjects(IEnumerable<IGH_DocumentObject> selectedObjects)
		{
			var selectedSet = new HashSet<IGH_DocumentObject>(selectedObjects);
			var outputLikeObjects = new HashSet<IGH_DocumentObject>();

			foreach (IGH_DocumentObject obj in selectedSet)
			{
				var compObj = obj as IGH_Component;
				var paramObj = obj as IGH_Param;
				bool isOutputLike = false;

				if (compObj != null)
				{
					if (compObj.Params.Output.Count == 0)
					{
						isOutputLike = true;
					}
					else
					{
						bool allOutputsUnconnected = compObj.Params.Output.All(p => p.Recipients.Count == 0);
						if (allOutputsUnconnected)
						{
							isOutputLike = true;
						}
						else
						{
							foreach (var outputParam in compObj.Params.Output)
							{
								foreach (IGH_Param recipient in outputParam.Recipients)
								{
									IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
									if (!selectedSet.Contains(recipientComponent))
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
					if (paramObj.Recipients.Count == 0)
					{
						isOutputLike = true;
					}
					else
					{
						foreach (IGH_Param recipient in paramObj.Recipients)
						{
							IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
							if (!selectedSet.Contains(recipientComponent))
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

			return outputLikeObjects;
		}
	}
}
