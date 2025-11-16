#r "nuget: Neo4j.Driver"
// async: true

using Neo4j.Driver;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

public class Neo4jConnector : IDisposable {

    public Neo4jConnector(
        string _uri = null, 
        string _user = null, 
        string _pass = null
    )
    {
        var uri = _uri ?? Environment.GetEnvironmentVariable("NEO4J_URI");
        var user = _user ?? Environment.GetEnvironmentVariable("NEO4J_USER");
        var password = _pass ?? Environment.GetEnvironmentVariable("NEO4J_PASSWORD");

        Driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        Session = Driver.AsyncSession();
    }

    public IAsyncSession Session {get;set;}

    IDriver Driver;

    // print debug info about a node
    public void PrintDebugInfo(string key, INode node, bool indent = false)
    {
        var prefix = indent ? "\t" : ""; 
        Console.WriteLine($"{prefix}Node {key}, Labels: {string.Join(",", node.Labels)}, Id {node.ElementId}");
        foreach (var prop in node.Properties)
        {
            Console.WriteLine($"{prefix}\t{prop.Key} = {prop.Value}");
        }
    }

    // print debug info about a relationship
    public void PrintDebugInfo(string key, IRelationship rel, bool indent = false)
    {
        var prefix = indent ? "\t" : ""; 
        Console.WriteLine($"{prefix}Relationship {key}, Type: {rel.Type}, Id {rel.ElementId}");
        foreach (var prop in rel.Properties)
        {
            Console.WriteLine($"{prefix}\t{prop.Key} = {prop.Value}");
        }
    }

    // print debug info about a path
    public void PrintDebugInfo(string key, IPath path, bool indent = false)
    {
        var prefix = indent ? "\t" : ""; 
        Console.WriteLine($"{prefix}Path {key}, Length: {path.Nodes.Count}");
        for (int i=0; i<path.Nodes.Count-1; i++)
        {
            PrintDebugInfo("", path.Nodes[i], true);
            PrintDebugInfo("", path.Relationships[i], true);
        }
        PrintDebugInfo("", path.End, true);
    }

    // run a query, optionally print debug info
    public async Task<IReadOnlyList<IRecord>> RunQuery(string queryString, bool printDebugInfo = false)
    {
        var result = await Session.RunAsync(queryString);
        var records = new List<IRecord>();

        // Fetch records, optionally print debug info
        await result.ForEachAsync(record =>
        {
            records.Add(record);
            if (!printDebugInfo)
                return;

            // Print debug info about all fields in the record
            for (int i = 0; i < record.Keys.Count; i++)
            {
                var key = record.Keys[i];
                var value = record[i];
                if (value is INode node) {
                    PrintDebugInfo(key, node);
                }
                else if (value is IRelationship rel)
                {
                    PrintDebugInfo(key, rel);
                }
                else if (value is IPath path)
                {
                    PrintDebugInfo(key, path);
                }
                else {
                    Console.Write($"{key} = {value}; ");
                }
            }
            Console.WriteLine();
        });

        return records;
    }

    public void Dispose()
    {
        Driver?.Dispose();
    }

}


public class ExactMatchQueryGenerator {

    public ExactMatchQueryGenerator(Neo4jConnector connector)
    {
        Connector = connector;
    }

    Neo4jConnector Connector;

    public async Task<IReadOnlyList<IRecord>> QueryFromTuples(IEnumerable<Tuple<IGH_DocumentObject, IGH_DocumentObject>> tuples)
    {
        // create a dictionary from componentinstance to variable name
        var nodeVarDict = new Dictionary<IGH_DocumentObject, string>();
        int i = 0;
        foreach (var tuple in tuples)
        {
            var node = tuple.Item1;
            if (!nodeVarDict.ContainsKey(node))
                nodeVarDict.Add(node, $"n{i++}");
            node = tuple.Item2;
            if (!nodeVarDict.ContainsKey(node))
                nodeVarDict.Add(node, $"n{i++}");
        }

        // create match clauses
        i = 0;
        var matches = tuples.Select(tuple => {
            var v1 = nodeVarDict[tuple.Item1];
            var v2 = nodeVarDict[tuple.Item1];
            return $"p{i++}=({v1}:ComponentInstance)-[:Wire]->({v2}:ComponentInstance)";
        });

        // create where clauses
        i = 0;
        var wheres = nodeVarDict.Keys.Select(node => $"{nodeVarDict[node]} = '{node.ComponentGuid}'");

        // create complete query
        var queryString = $"MATCH {string.Join(", ", matches)} WHERE {string.Join(" AND ", wheres)} RETURN n1.VersionId, n1.PivotX, n2.PivotY";
    
        return await Connector.RunQuery(queryString, true);
    }

}


// how to use the connector
using (var connector = new Neo4jConnector())
{
    var query = @"MATCH p=(n:ComponentInstance)-[w:Wire]->(m:ComponentInstance) 
    WHERE n.ComponentGuid = 'c9785b8e-2f30-4f90-8ee3-cca710f82402' 
    AND m.ComponentGuid = '071c3940-a12d-4b77-bb23-42b5d3314a0d' 
    RETURN p";

    //var query = "MATCH (n:ComponentInstance) return n";
    //var query = "show indexes";
    await connector.RunQuery(query, true);
 
}


