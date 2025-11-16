#r "nuget: Neo4j.Driver"
// async: true

using Neo4j.Driver;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using GraphHop2.Utilities;

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


public class QueryGenerator {

    public QueryGenerator(Neo4jConnector connector)
    {
        Connector = connector;
    }

    Neo4jConnector Connector;

    T GetRecordValue<T>(IRecord record, string key)
    {
        if (record.TryGet(key, out T value))
            return value;
        throw new ArgumentException($"Could not get key {key} from record");
    }

    public async Task<List<string>> QueryDocumentVersionFilePathsFromComponent(IReadOnlyList<IRecord> records)
    {
        List<string> filePaths = new List<string>();
        foreach (var record in records)
        {
            var versionId = GetRecordValue<string>(record, "VersionId");
            var queryString = $"MATCH (n:DocumentVersion) WHERE n.VersionId = '{versionId}' RETURN n.FilePath AS FilePath, n.IsCluster as IsCluster";
            var result = await Connector.RunQuery(queryString, true);
            if (result.Count != 1)
                throw new ArgumentException($"Could not get document with version {versionId}");
            var filePath = GetRecordValue<string>(result.First(), "FilePath");
            if (string.IsNullOrEmpty(filePath))
                continue;
            filePaths.Add(filePath);
        }
        return filePaths;
    }

    public async Task<List<(string, int, int)>> QueryDocumentDataFromComponent(IReadOnlyList<IRecord> records)
    {
        List<(string, int, int)> filePathsAndCoords = new List<(string, int, int)>();
        foreach (var record in records)
        {
            var versionId = GetRecordValue<string>(record, "VersionId");
            var queryString = $"MATCH (n:DocumentVersion) WHERE n.VersionId = '{versionId}' RETURN n.FilePath AS FilePath, n.IsCluster as IsCluster";
            var result = await Connector.RunQuery(queryString, true);
            if (result.Count != 1)
                throw new ArgumentException($"Could not get document with version {versionId}");
            var filePath = GetRecordValue<string>(result.First(), "FilePath");
            if (string.IsNullOrEmpty(filePath))
                continue;
            var pivotX = GetRecordValue<int>(record, "PivotX"); 
            var pivotY = GetRecordValue<int>(record, "PivotY"); 
            filePathsAndCoords.Add((filePath, pivotX, pivotY));
        }
        return filePathsAndCoords;
    }

    public async Task<IReadOnlyList<IRecord>> QueryFromTuples(List<(IGH_DocumentObject, IGH_DocumentObject)> tuples)
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
            var v2 = nodeVarDict[tuple.Item2];
            return $"p{i++}=({v1}:ComponentInstance)-[:Wire]->({v2}:ComponentInstance)";
        });

        // create where clauses
        i = 0;
        var wheres = nodeVarDict.Keys.Select(node => $"{nodeVarDict[node]}.ComponentGuid = '{node.ComponentGuid}'");

        // create complete query
        var queryString = $"MATCH {string.Join(", ", matches)} WHERE {string.Join(" AND ", wheres)} RETURN DISTINCT n1.VersionId AS VersionId, n1.InstanceGuid AS InstanceGuid, n1.PivotX AS PivotX, n1.PivotY AS PivotY";
        Console.WriteLine(queryString);
        
        return await Connector.RunQuery(queryString, true);
    }

}

List<(IGH_DocumentObject, IGH_DocumentObject)> GetConnections(IEnumerable<IGH_DocumentObject> selectedObjects)
{
    var connections = new List<(IGH_DocumentObject, IGH_DocumentObject)>();

    foreach (IGH_DocumentObject obj in selectedObjects)
    {
        if (obj is IGH_Component component)
        {
            // Output connections (this component to recipients)
            foreach (var outputParam in component.Params.Output)
            {
                foreach (IGH_Param recipient in outputParam.Recipients)
                {
                    IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
                    if (selectedObjects.Contains(recipientComponent))
                        connections.Add((obj, recipientComponent));
                }
            }
        }
        else if (obj is IGH_Param par)
        {
            foreach (IGH_Param recipient in par.Recipients)
            {
                IGH_DocumentObject recipientComponent = recipient.Attributes.GetTopLevel.DocObject;
                if (selectedObjects.Contains(recipientComponent))
                    connections.Add((obj, recipientComponent));
            }
        }
    }

    return connections;
}

List<(IGH_DocumentObject, IGH_DocumentObject)> GetSelectedTuples()
{
    var empty = new List<(IGH_DocumentObject, IGH_DocumentObject)>();

    if (Grasshopper.Instances.ActiveCanvas == null)
    {
        Console.WriteLine("Grasshopper is not loaded or no canvas is active!");
        return empty;
    }

    var ghDoc = Grasshopper.Instances.ActiveCanvas.Document;
    if (ghDoc == null)
    {
        Console.WriteLine("No active Grasshopper document found!");
        return empty;
    }

    var selectedObjects = ghDoc.SelectedObjects();
    if (selectedObjects.Count == 0)
    {
        Console.WriteLine("No Grasshopper components selected!");
        return empty;
    }

    var connections = GetConnections(selectedObjects);

    if (connections.Count == 0)
    {
        Console.WriteLine("No connections found for selected components.");
        return empty;
    }

    Console.WriteLine($"Number of nodes/connections found: {selectedObjects.Count}/{connections.Count}");

    return connections;
}


using (var connector = new Neo4jConnector())
{
    var tuples = GetSelectedTuples();
    if (tuples.Count == 0)
        return;

    var generator = new QueryGenerator(connector);
    var records = await generator.QueryFromTuples(tuples);
    var documentData = await generator.QueryDocumentDataFromComponent(records);
    Console.WriteLine($"Found {documentData.Count} document versions");
    foreach (var data in documentData)
    {
        Console.WriteLine($"File {data.Item1}, PivotX {data.Item2}, PivotY {data.Item3}");
    }
}

    