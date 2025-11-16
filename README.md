# GraphHop2

A Grasshopper plugin that enables intelligent search for similar component clusters from a local Neo4j graph database. Select components in Grasshopper, and GraphHop2 will find and display similar configurations from your database, allowing you to quickly discover and open related Grasshopper files.

**AecTech2025 Hackathon Project**

## Overview

GraphHop2 connects Grasshopper (Rhino's visual programming environment) with a Neo4j graph database containing parsed Grasshopper files. When you select components in Grasshopper, the tool queries the database to find similar component clusters and displays matching files in a popup window. You can then open these files directly with the solver disabled for safe exploration.

## Features

- **Component Selection Query**: Select components in Grasshopper and query for similar configurations
- **Exact Match Search**: Find files containing identical component connections and configurations
- **Fuzzy Path Matching**: Discover similar component paths with flexible length matching
- **File Browser Interface**: Popup window displaying matching Grasshopper files with details
- **Safe File Opening**: Automatically disables solver before opening files to prevent unintended execution
- **Neo4j Integration**: Leverages graph database for efficient pattern matching and relationship queries

## Requirements

- **Rhino 7+** (or compatible version)
- **Grasshopper** (included with Rhino)
- **Neo4j Database** (local or remote instance)
- **Neo4j.Driver** NuGet package (automatically referenced via `#r "nuget: Neo4j.Driver"`)

## Setup

### 1. Neo4j Database Setup

1. Install and run Neo4j (Community Edition or Desktop)
2. Load your Grasshopper database dump file into Neo4j
3. Ensure the database contains parsed Grasshopper files with the following structure:
   - **Nodes**: `ComponentInstance`, `DocumentVersion`
   - **Relationships**: `Wire` (between ComponentInstance nodes)
   - **Properties**: `ComponentGuid`, `VersionId`, `FilePath`, `PivotX`, `PivotY`

<img width="1500" height="auto" alt="Screenshot 2025-11-16 at 10 00 52 AM" src="https://github.com/user-attachments/assets/b0e2b9f4-a6b0-41c5-91b7-983f0ed3ab35" />
<img width="1500" height="auto" alt="Screenshot 2025-11-16 at 10 04 35 AM" src="https://github.com/user-attachments/assets/a5ce1672-ee14-41d1-84b5-b0879bb8203a" />


### 2. Environment Variables

Set the following environment variables for Neo4j connection:

```bash
NEO4J_URI=neo4j://[your_connection_uri]
NEO4J_USER=your_username
NEO4J_PASSWORD=your_password
```

Alternatively, you can pass these values directly to the `Neo4jConnector` constructor.

### 3. Install GraphHop2

1. Clone or download this repository
2. Copy the project files to your Grasshopper components directory or reference them in your Grasshopper scripts
3. Ensure all utility classes are accessible:
   - `SelectionToGraphUtility.cs`
   - `FindPathFromInputToOutput.cs`
   - Other utility classes in the `Utilities/` folder

## Usage

### Basic Workflow

1. **Select Components**: In Grasshopper, select the components you want to search for
2. **Run Query**: Execute the search script (via C# script component or custom component)
3. **View Results**: A popup window will display matching Grasshopper files
4. **Open File**: Select a file from the list and click "Open" to load it (solver will be disabled)

### Example: Query Selected Components

```csharp
using (var connector = new Neo4jConnector())
{
    var tuples = GetSelectedTuples();
    if (tuples.Count == 0)
        return;

    var generator = new ExactMatchQueryGenerator(connector);
    var records = await generator.QueryFromTuples(tuples);
    var filePaths = await generator.QueryDocumentVersionFilePathsFromComponent(records);
    
    Console.WriteLine($"Found {filePaths.Count} document versions");
}
```

### Example: Search with Scoring

```csharp
var search = new Search();
var selectedObjects = Grasshopper.Instances.ActiveCanvas.Document.SelectedObjects();
int score = search.search(selectedObjects);
```

## Architecture

### Core Components

#### `Neo4jConnector`
- Manages connection to Neo4j database
- Provides query execution with optional debug output
- Handles async operations and resource disposal

#### `ExactMatchQueryGenerator`
- Generates Cypher queries for exact component configuration matches
- Converts Grasshopper component selections to graph queries
- Retrieves file paths for matching document versions

#### `FuzzyPathQueryGenerator`
- Creates queries for similar component paths
- Supports flexible path length matching (min/max length)
- Finds paths between start and end components

#### `Search`
- Main search interface combining multiple query strategies
- Implements scoring system for match quality
- Integrates exact match and path match queries

#### `GrasshopperFileLister`
- Provides UI for file selection and opening
- Displays file details (name, modification date, location)
- Handles file opening with solver disabled

### Database Schema

```
ComponentInstance
  - ComponentGuid (string)
  - VersionId (string)
  - PivotX (float)
  - PivotY (float)

DocumentVersion
  - VersionId (string)
  - FilePath (string)

Wire (Relationship)
  - Connects ComponentInstance nodes
```

### Query Types

1. **Exact Match Query**: Finds files with identical component connections
   ```cypher
   MATCH (n1:ComponentInstance)-[:Wire]->(n2:ComponentInstance)
   WHERE n1.ComponentGuid = '...' AND n2.ComponentGuid = '...'
   RETURN n1.VersionId, n1.PivotX, n1.PivotY
   ```

2. **Fuzzy Path Query**: Finds similar paths between components
   ```cypher
   MATCH (start:ComponentInstance),
         (end:ComponentInstance),
         p=((start)-[:Wire*{minLength}..{maxLength}]->(end))
   WHERE start.ComponentGuid = '...' AND end.ComponentGuid = '...'
   RETURN p
   ```

## Utilities

### `SelectionToGraphUtility`
Converts selected Grasshopper components into connection tuples for graph queries.

### `FindPathFromInputToOutput`
Finds shortest paths from input components to output components in the selection.

## Development

### Project Structure

```
GraphHop2/
├── Neo4jConnector.cs          # Database connection and query execution
├── Search.cs                   # Main search functionality
├── EtoForm.cs                  # UI components
├── QuerySelectedGhComponent*.cs # Various query implementations
├── Utilities/
│   ├── SelectionToGraphUtility.cs
│   ├── FindPathFromInputToOutput.cs
│   └── ...
└── README.md
```

### Adding New Query Types

1. Create a new query generator class (similar to `ExactMatchQueryGenerator`)
2. Implement query generation logic
3. Add scoring method to `Search` class
4. Integrate into main search workflow

## Troubleshooting

### Connection Issues
- Verify Neo4j is running and accessible
- Check environment variables are set correctly
- Ensure firewall allows connection to Neo4j port (default: 7687)

### No Results Found
- Verify database contains parsed Grasshopper files
- Check that ComponentGuid values match between Grasshopper and database
- Ensure Wire relationships are properly created in the database

### File Opening Issues
- Verify file paths in database are valid
- Check file permissions
- Ensure Grasshopper is loaded in Rhino

## License

This project was created for the AecTech2025 Hackathon.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Acknowledgments

- Built for AecTech2025 Hackathon
- Uses Neo4j graph database for pattern matching
- Integrates with Grasshopper and Rhino
