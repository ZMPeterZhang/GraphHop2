using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino;

namespace GraphHop2.Utilities
{
	public static class FindPathFromInputToOutput
	{
        
        private static List<IGH_DocumentObject> GetParents(IGH_DocumentObject node, List<(IGH_DocumentObject, IGH_DocumentObject)> graph)
        {
            var parents = graph
                .Where(edge => edge.Item2 == node)
                .Select(edge => edge.Item1)
                .ToList();
            RhinoApp.WriteLine($"The parents for node {node.NickName} are: {string.Join(", ", parents.Select(p => p.NickName))}");
            return parents;
        }

        private static List<List<IGH_DocumentObject>> FindAllPaths(
            IGH_DocumentObject input,
            IGH_DocumentObject output,
            List<(IGH_DocumentObject, IGH_DocumentObject)> graph
        )
        {
            var allPaths = new List<List<IGH_DocumentObject>>();
            var currentPath = new List<IGH_DocumentObject>();
            var visited = new HashSet<IGH_DocumentObject>();

            void DFS(IGH_DocumentObject node)
            {
                currentPath.Add(node);
                visited.Add(node);

                if (node.Equals(output))
                {
                    allPaths.Add(new List<IGH_DocumentObject>(currentPath));
                }
                else
                {
                    // Get children (nodes this node points to)
                    var children = graph.Where(edge => edge.Item1 == node).Select(edge => edge.Item2);
                    foreach (var child in children)
                    {
                        if (!visited.Contains(child))
                        {
                            DFS(child);
                        }
                    }
                }

                // Backtrack
                currentPath.RemoveAt(currentPath.Count - 1);
                visited.Remove(node); 
            }

            DFS(input);

            return allPaths;
        }
        /// <summary>
        /// BFS for shortest path from Input to Output in a DAG (returns node chain or empty if not found)
        /// </summary>
        private static List<IGH_DocumentObject> FindShortestPath(
            IGH_DocumentObject input,
            IGH_DocumentObject output,
            List<(IGH_DocumentObject, IGH_DocumentObject)> graph
        )
        {
            var queue = new Queue<List<IGH_DocumentObject>>();
            var visited = new HashSet<IGH_DocumentObject>();
            queue.Enqueue(new List<IGH_DocumentObject> { input });

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var last = path.Last();
                if (last.Equals(output))
                    return path;

                if (!visited.Add(last))
                    continue;

                // Find neighbors (nodes THIS node points to)
                var children = graph
                    .Where(edge => edge.Item1 == last)
                    .Select(edge => edge.Item2);

                foreach (var child in children)
                {
                    if (!visited.Contains(child))
                    {
                        var newPath = new List<IGH_DocumentObject>(path) { child };
                        queue.Enqueue(newPath);
                    }
                }
            }
            return new List<IGH_DocumentObject>(); // No path found
        }
		/// <summary>
		/// Returns the paths from input nodes to output nodes in a graph
		/// </summary>
        public static List<List<IGH_DocumentObject>> GetShortestPathsFromInputsToOutputs(IEnumerable<IGH_DocumentObject> selectedObjects)
        {
            HashSet<IGH_DocumentObject> inputs = SelectionToInputUtility.GetInputLikeObjects(selectedObjects);
            HashSet<IGH_DocumentObject> outputs = SelectionToOutputUtility.GetOutputLikeObjects(selectedObjects);
            RhinoApp.WriteLine($"INPUTS: {string.Join(", ", inputs.Select(i => i.NickName))}");
            RhinoApp.WriteLine($"OUTPUTS: {string.Join(", ", outputs.Select(i => i.NickName))}");
            List<(IGH_DocumentObject, IGH_DocumentObject)> graph = SelectionToGraphUtility.GetConnections(selectedObjects);

            var allPaths = new List<List<IGH_DocumentObject>>();
            foreach (IGH_DocumentObject input in inputs)
            {
                foreach (IGH_DocumentObject output in outputs)
                {
                    var spath = FindShortestPath(input, output, graph);
                    RhinoApp.WriteLine($"The shortest path is: {string.Join(" -> ", spath.Select(n => n.NickName))}");
                    if (spath.Any())
                        allPaths.Add(spath);
                }
            }
            return allPaths;
        }

        public static List<List<IGH_DocumentObject>> GetAllPathsFromInputsToOutputs(IEnumerable<IGH_DocumentObject> selectedObjects)
        {
            HashSet<IGH_DocumentObject> inputs = SelectionToInputUtility.GetInputLikeObjects(selectedObjects);
            HashSet<IGH_DocumentObject> outputs = SelectionToOutputUtility.GetOutputLikeObjects(selectedObjects);
            List<(IGH_DocumentObject, IGH_DocumentObject)> graph = SelectionToGraphUtility.GetConnections(selectedObjects);
            var allPaths = new List<List<IGH_DocumentObject>>();
            RhinoApp.WriteLine($"INPUTS: {string.Join(", ", inputs.Select(i => i.NickName))}");
            RhinoApp.WriteLine($"OUTPUTS: {string.Join(", ", outputs.Select(i => i.NickName))}");

            foreach (IGH_DocumentObject input in inputs)
            {
                foreach (IGH_DocumentObject output in outputs)
                {
                    var paths = FindAllPaths(input, output, graph);
                    foreach (var path in paths)
                    {
                        RhinoApp.WriteLine($"Path X: {string.Join(" -> ", path.Select(n => n.NickName))}");
                        allPaths.Add(path);
                    }
                }
            }
            return allPaths;
        }
    }
}
