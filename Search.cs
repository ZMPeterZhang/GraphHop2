// /Users/romtecmax/Documents/GitHub/GraphHop2/Search.cs
using System;
using System.Linq;
using Grasshopper.Kernel;
using System.Collections.Generic;

namespace GraphHop2 {
    public class Search
    {
        /// <summary>
        /// This function checks the graph db for similar scripts.
        /// </summary>
        /// <param name="term">The search term to be matched</param>
        /// <returns>An integer representing the score of the match</returns>
        public int search(IEnumerable<IGH_DocumentObject> selectedObjects)
        {
            int totalScore = 0;
            totalScore += exactMatchQuery(selectedObjects);
            totalScore += pathMatchQuery(selectedObjects);

            // Add more query scores as needed
            return max(totalScore, 100);
        }

        /// <summary>   
        /// This function checks for an exact match of the configuration in another file
        /// </summary>
        /// <param name="term">The search term to be matched</param>
        /// <returns>An integer representing the score of the match</returns>
        public int exactMatchQuery(IEnumerable<IGH_DocumentObject> selectedObjects)
        {        
            List<(IGH_DocumentObject, IGH_DocumentObject)> graph = SelectionToGraphUtility.GetConnections(selectedObjects);
            
            int score = 0;

            try
            {
                using (var connector = new Neo4jConnector())
                {
                    var generator = new ExactMatchQueryGenerator(connector);
                    // QueryFromTuples is async; block-wait for result here (unless this is run in async context)
                    var result = generator.QueryFromTuples(graph).GetAwaiter().GetResult();
                    // You may use the record count or other logic for scoring
                    if (result != null && result.Count > 0)
                        score = 100;

                    Console.WriteLine($"Score in exactMatchQuery: {score}"
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in exactMatchQuery: " + ex.Message);
            }

            return score;
        }


        public int pathMatchQuery(IEnumerable<IGH_DocumentObject> selectedObjects)
        {
            // Assuming path is a list of IGH_DocumentObject forming a path: [start, ..., end]
            var paths = FindPathFromInputToOutput.GetShortestPathsFromInputsToOutputs(selectedObjects);

            foreach (var path in paths)
            {
                if (path == null || path.Count < 2)
                return 0;

                var start = path.First();
                var end = path.Last();
                var pathLength = path.Count;

                // Get ElementIds or unique Neo4j IDs for start and end if possible
                var startId = start.ComponentGuid.ToString();
                var endId = end.ComponentGuid.ToString();

                int minLength = Math.Max(1, pathLength - 2);
                int maxLength = pathLength + 4;

                string query = FuzzyPathQueryGenerator.QueryFromPath(startId, endId, minLength, maxLength);
                Console.WriteLine(query);
            }
        return 10;
            
        }
    }
}