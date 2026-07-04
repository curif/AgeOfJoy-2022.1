using UnityEngine;
using System.IO;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MarketplaceDatabase : MonoBehaviour
{
    public string CatalogYamlFilePath;
    public string CabinetsTSVCatalogName;
    public string CabinetsTSVFilePath;

    private SQLiteConnection db;
    private const int LATEST_DB_VERSION = 1; // Set the latest version
    private const string DB_VERSION_KEY = "db_version"; // Key to store the DB version

    void Start()
    {
        Init();
    }

    void Init()
    {
        string dbPath = Path.Combine(ConfigManager.CabinetsDB, "marketplace.db"); // Use the configured database path
        db = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        ConfigManager.WriteConsole("Database Path: " + dbPath);

        int currentDbVersion = PlayerPrefs.GetInt(DB_VERSION_KEY, 0); // Get current version from PlayerPrefs
        if (currentDbVersion < LATEST_DB_VERSION)
        {
            PerformMigrations(currentDbVersion);
            PlayerPrefs.SetInt(DB_VERSION_KEY, LATEST_DB_VERSION); // Update to latest version
            PlayerPrefs.Save();
        }

        InitializeDatabase();
    }

    [System.Serializable]
    public class Catalog
    {
        [PrimaryKey, Unique]
        [NotNull] public string CatalogName { get; set; }

        public string Description { get; set; }
        [NotNull] public string Url { get; set; }
    }
    [System.Serializable]
    public class Cabinet
    {
        // --- Handling Composite Primary Key for ORM ---
        // IMPORTANT: SQLite-net's standard ORM operations like db.Update(obj) or db.Get<T>(pk)
        // work best with a single primary key defined by [PrimaryKey].
        // Since your actual PK is composite (CatalogName, Name), you have a few considerations:
        // 1. You CANNOT use [PrimaryKey] on CatalogName or Name here if the actual DB PK is composite.
        // 2. Operations like db.Update(cabinetObject) might NOT work as expected because the ORM
        //    won't automatically know how to build the WHERE clause for the composite key.
        //    You might need to use db.Update(cabinetObject, typeof(Cabinet)) or write specific update queries.
        // 3. db.Delete(cabinetObject) will likely face the same issue.
        // 4. Queries (db.Table<Cabinet>().Where(...)) will work fine.
        // 5. db.Insert(cabinetObject) will work fine.

        // Remove individual [PrimaryKey] attributes
        [NotNull] public string CatalogName { get; set; } // Part of composite key, references Catalog.Name

        [NotNull] public string Name { get; set; } // cabinet name
        [NotNull] public string game { get; set; } // game name

        public string CreationDate { get; set; } // YYYY-MM-DD
        public string Version { get; set; }
        public string RomName { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Core { get; set; }

        [Indexed] // Query by creator is common
        public string Creator { get; set; }

        public string Notes { get; set; }
    }

    //only for yamls
    [System.Serializable]
    public class MarketplaceWrapper
    {
        public List<Catalog> marketplace { get; set; }
    }

    private void InitializeDatabase()
    {
        // Add foreign key constraint with cascade delete
        db.Execute("PRAGMA foreign_keys = ON;");

        db.Execute(@"CREATE TABLE IF NOT EXISTS Catalog (
                    CatalogName TEXT primary key not null ,
                    Description TEXT,
                    url TEXT)"
                    ); 
        // Remove default CreateTable for Cabinet and define it with a composite key
        db.Execute(@"CREATE TABLE IF NOT EXISTS Cabinet (
                        CatalogName TEXT NOT NULL,
                        Name TEXT NOT NULL,
                        Game TEXT NOT NULL,
                        CreationDate TEXT,
                        Version TEXT,
                        RomName TEXT,
                        Url TEXT NOT NULL,
                        Description TEXT,
                        Core TEXT,
                        Creator TEXT,
                        Notes TEXT,
                        PRIMARY KEY (CatalogName, Name),
                        FOREIGN KEY (CatalogName) REFERENCES Catalog(CatalogName) ON DELETE CASCADE
                    );"
                );

        // Add indexes for performance
        db.Execute("CREATE INDEX IF NOT EXISTS idx_cabinet_name ON Cabinet (Name);");
        db.Execute("CREATE INDEX IF NOT EXISTS idx_cabinet_creator ON Cabinet (Creator);");

        ConfigManager.WriteConsole("Database initialized with indexes.");
    }
    // Method to apply migrations
    private void PerformMigrations(int currentVersion)
    {
        if (currentVersion < 1)
        {
            InitializeDatabase();
            ConfigManager.WriteConsole("Applied Migration to Version 1.");
        }
    }

    public bool CabinetExists(string catalogName, string name)
    {
        string checkSql = "SELECT EXISTS (SELECT 1 FROM Cabinet WHERE CatalogName = ? AND Name = ? LIMIT 1)";
        int existsResult = 0;
        try
        {
            existsResult = db.ExecuteScalar<int>(checkSql, catalogName, name);

        }
        catch (SQLiteException e)
        {
            ConfigManager.WriteConsoleException($"Error checking for existing cabinet '{name}' in catalog '{catalogName}'", e);
            return false; // Stop execution if the check fails
        }
        return existsResult == 1;
    }

    // Insert or update a catalog
    public void AddCatalog(string CatalogName, string description, string url)
    {
        var existingCatalog = db.Find<Catalog>(CatalogName);
        if (existingCatalog == null)
        {
            try
            {
                db.Insert(new Catalog { CatalogName = CatalogName, Description = description, Url = url });
                ConfigManager.WriteConsole($"Added new catalog: '{CatalogName}'");
            }
            catch (SQLiteException e)
            {
                ConfigManager.WriteConsoleException($"Failed to add catalog '{CatalogName}'", e);
            }
        }
        else
        {
            try
            {
                existingCatalog.Description = description;
                existingCatalog.Url = url;
                db.Update(existingCatalog);
                ConfigManager.WriteConsole($"Updated existing catalog: '{CatalogName}'");
            }
            catch (SQLiteException e)
            {
                ConfigManager.WriteConsoleException($"Failed to update catalog '{CatalogName}'",e);
            }
        }
    }

    // Get all catalogs
    public List<Catalog> GetAllCatalogs()
    {
        return db.Table<Catalog>().ToList();
    }

    //it is complex to use the ORM when using composite PK.
    public void AddCabinet(string catalogName, string name, string game, string creationDate, string version, string romName, string url, string description, string core, string creator, string notes)
    {
        if (!CabinetExists(catalogName, name))
        {
            string insertSql = @"
                INSERT INTO Cabinet (
                    CatalogName, Name, Game, CreationDate, Version, RomName, Url, Description, Core, Creator, Notes
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            try
            {
                // Parameters must be in the exact order of the columns listed above and the '?' placeholders
                db.Execute(insertSql,
                    catalogName,
                    name,
                    game, // Assuming SQL column name is 'Game' matching the parameter
                    creationDate,
                    version,
                    romName,
                    url,
                    description,
                    core,
                    creator,
                    notes
                );
                ConfigManager.WriteConsole($"Added new cabinet: '{name}' in catalog '{catalogName}'");
            }
            catch (SQLiteException e)
            {
                // Catch potential constraint violations (e.g., FK violation if CatalogName doesn't exist in Catalog)
                ConfigManager.WriteConsoleException($"Failed to add cabinet '{name}' in catalog '{catalogName}'", e);
            }
        }
        else
        {
            string updateSql = @"
                UPDATE Cabinet
                SET Game = ?,
                    CreationDate = ?,
                    Version = ?,
                    RomName = ?,
                    Url = ?,
                    Description = ?,
                    Core = ?,
                    Creator = ?,
                    Notes = ?
                WHERE CatalogName = ? AND Name = ?"; // Use composite key in WHERE clause

            try
            {
                // Parameters must be in the exact order: first the SET parameters, then the WHERE parameters
                db.Execute(updateSql,
                    // SET parameters:
                    game,
                    creationDate, // Include creationDate in update as per original logic
                    version,
                    romName,
                    url,
                    description,
                    core,
                    creator,
                    notes,
                    // WHERE parameters:
                    catalogName,
                    name
                );
                ConfigManager.WriteConsole($"Updated existing cabinet: '{name}' in catalog '{catalogName}'");
            }
            catch (SQLiteException e)
            {
                ConfigManager.WriteConsoleException($"Failed to update cabinet '{name}' in catalog '{catalogName}'", e);
            }
        }
    }

    // Get all cabinets in a catalog (paginated)
    public List<Cabinet> GetCatalogPage(string catalogName, int page, int pageSize)
    {
        int offset = page * pageSize;
        return db.Query<Cabinet>(
            "SELECT * FROM Cabinet WHERE CatalogName = ? ORDER BY Name ASC LIMIT ? OFFSET ?;",
            catalogName, pageSize, offset
        );
    }

    // Get cabinets by name (fast lookup)
    public List<Cabinet> GetCabinetsByName(string name)
    {
        return db.Table<Cabinet>().Where(c => c.Name == name).ToList();
    }

    // Search cabinets by partial name
    public List<Cabinet> SearchCabinetsByName(string partialName)
    {
        return db.Query<Cabinet>(
            "SELECT * FROM Cabinet WHERE LOWER(Name) LIKE ? ORDER BY Name ASC;",
            "%" + partialName.ToLower() + "%"
        );
    }

    // Get cabinets by creator
    public List<Cabinet> GetCabinetsByCreator(string creator)
    {
        return db.Query<Cabinet>(
            "SELECT * FROM Cabinet WHERE Creator = ? ORDER BY Name ASC;",
            creator
        );
    }

    // Get cabinets by ROM name
    public List<Cabinet> GetCabinetsByRom(string romName)
    {
        return db.Query<Cabinet>(
            "SELECT * FROM Cabinet WHERE RomName = ?;",
            romName
        );
    }

    // Get cabinets by core (architecture)
    public List<Cabinet> GetCabinetsByCore(string core)
    {
        return db.Query<Cabinet>(
            "SELECT * FROM Cabinet WHERE Core = ? ORDER BY Name ASC;",
            core
        );
    }

    // Update a catalog
    public void UpdateCatalog(string name, string description, string url)
    {
        var catalog = db.Find<Catalog>(name);
        if (catalog != null)
        {
            catalog.Description = description;
            catalog.Url = url;
            db.Update(catalog);
        }
    }

    // Update a cabinet using CatalogName and Name
    public void UpdateCabinet(string catalogName, string name, string version, string url, string notes)
    {
        var cabinet = db.Find<Cabinet>(c => c.CatalogName == catalogName && c.Name == name);
        if (cabinet != null)
        {
            cabinet.Version = version;
            cabinet.Url = url;
            cabinet.Notes = notes;
            db.Update(cabinet);
        }
    }

    // Delete a catalog (and all its cabinets)
    public void DeleteCatalog(string name)
    {
        db.Execute("DELETE FROM Cabinet WHERE CatalogName = ?", name);
        db.Delete<Catalog>(name);
    }

    // Delete a cabinet using CatalogName and Name
    public void DeleteCabinet(string catalogName, string name)
    {
        db.Execute("DELETE FROM Cabinet WHERE CatalogName = ? AND Name = ?", catalogName, name);
    }

    void OnApplicationQuit()
    {
        db.Close();
    }

    public void RefreshCatalogsFromYaml(string yamlFilePath)
    {
        try
        {
            if (db == null)
                Init();

            // Read and deserialize YAML content
            string yamlContent = File.ReadAllText(yamlFilePath);
            var deserializer = new DeserializerBuilder().Build();
            var yamlMarketplace = deserializer.Deserialize<MarketplaceWrapper>(yamlContent);

            List<Catalog> yamlCatalogs = yamlMarketplace?.marketplace ?? new List<Catalog>();

            // Get all existing catalogs from the database
            var dbCatalogs = db.Table<Catalog>().ToDictionary(c => c.CatalogName, c => c);

            // Track catalogs in YAML for removal check
            var yamlCatalogNames = new HashSet<string>(yamlCatalogs.Select(c => c.CatalogName));

            // Step 1: Add or update catalogs from YAML
            foreach (var yamlCatalog in yamlCatalogs)
            {
                if (string.IsNullOrEmpty(yamlCatalog.CatalogName))
                {
                    ConfigManager.WriteConsoleWarning("Skipping YAML catalog with empty or null Name.");
                    continue;
                }

                var existingCatalog = dbCatalogs.ContainsKey(yamlCatalog.CatalogName) ? dbCatalogs[yamlCatalog.CatalogName] : null;

                if (existingCatalog == null)
                {
                    // New catalog: insert it
                    try
                    {
                        db.Insert(yamlCatalog);
                        ConfigManager.WriteConsole($"Added new catalog: {yamlCatalog.CatalogName}");
                    }
                    catch (SQLiteException e)
                    {
                        ConfigManager.WriteConsoleError($"Failed to insert catalog '{yamlCatalog.CatalogName}': {e.Message}");
                    }
                }
                else
                {
                    // Existing catalog: update if modified
                    if (existingCatalog.Description != yamlCatalog.Description || existingCatalog.Url != yamlCatalog.Url)
                    {
                        try
                        {
                            existingCatalog.Description = yamlCatalog.Description;
                            existingCatalog.Url = yamlCatalog.Url;
                            db.Update(existingCatalog);
                            ConfigManager.WriteConsole($"Updated existing catalog: {yamlCatalog.CatalogName}");
                        }
                        catch (SQLiteException e)
                        {
                            ConfigManager.WriteConsoleError($"Failed to update catalog '{yamlCatalog.CatalogName}': {e.Message}");
                        }
                    }
                    // Remove from dbCatalogs to mark it as "still exists"
                    dbCatalogs.Remove(yamlCatalog.CatalogName);
                }
            }

            // Step 2: Remove catalogs no longer in YAML (dependent cabinets will cascade delete)
            foreach (var obsoleteCatalog in dbCatalogs.Values)
            {
                try
                {
                    db.Delete(obsoleteCatalog);
                    ConfigManager.WriteConsole($"Removed obsolete catalog '{obsoleteCatalog.CatalogName}' (cabinets auto-deleted via cascade).");
                }
                catch (SQLiteException e)
                {
                    ConfigManager.WriteConsoleError($"Failed to remove catalog '{obsoleteCatalog.CatalogName}': {e.Message}");
                }
            }

            ConfigManager.WriteConsole("Catalogs synchronized with YAML successfully.");
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException("Failed to load catalogs from YAML ", e);
        }
    }

    // --- MODIFIED METHOD ---
    public void RefreshCabinetsFromTsv(string catalogName, string tsvFilePath)
    {
        if (db == null)
            Init();

        try
        {
            if (!File.Exists(tsvFilePath))
            {
                ConfigManager.WriteConsoleWarning($"TSV file not found at {tsvFilePath}.");
                return;
            }

            string tsvContent = File.ReadAllText(tsvFilePath);
            List<string[]> rows = ParseTsv(tsvContent);

            // --- CHANGE 1: Check for empty file (no rows at all) ---
            if (rows.Count == 0) // Changed from rows.Count <= 1
            {
                // --- CHANGE 2: Update warning message ---
                ConfigManager.WriteConsoleWarning($"TSV file at {tsvFilePath} is empty.");
                return;
            }

            // --- REMOVED: Header processing ---
            // string[] headers = rows[0];

            // Define expected column positions (0-based index)
            const int NameIndex = 0;
            const int UrlIndex = 1;
            const int GameIndex = 2;
            const int CreationDateIndex = 3;
            const int VersionIndex = 4;
            const int RomNameIndex = 5;
            const int DescriptionIndex = 6;
            const int CoreIndex = 7;
            const int CreatorIndex = 8;
            const int NotesIndex = 9;

            // Define minimum required columns (e.g., Name and Game)
            const int MinRequiredColumns = 2; // Adjust if only Name is strictly required (set to 1)

            // --- CHANGE 3: Loop starts from 0 and iterates through all rows ---
            for (int i = 0; i < rows.Count; i++) // Changed from i = 1
            {
                string[] values = rows[i];
                int currentRowNumber = i + 1; // For user-friendly messages (1-based)

                // --- CHANGE 4: Column count check based on minimum required ---
                if (values.Length < MinRequiredColumns)
                {
                    ConfigManager.WriteConsoleWarning($"Skipping row {currentRowNumber}: has only {values.Length} columns, expected at least {MinRequiredColumns}.");
                    continue;
                }

                // --- REMOVED: rowData dictionary creation ---
                // var rowData = new Dictionary<string, string>(); ...

                // --- CHANGE 5: Access data by index with safety checks ---

                // Get required fields by index
                string name = values[NameIndex]; // Column 1
                if (string.IsNullOrWhiteSpace(name)) // Use IsNullOrWhiteSpace for better check
                {
                    ConfigManager.WriteConsoleWarning($"Skipping row {currentRowNumber}: Name (column {NameIndex + 1}) is required and cannot be empty.");
                    continue;
                }

                // It's good practice to assume Game (Column 2) is also required or handle nulls appropriately
                string game = values[GameIndex];
                if (string.IsNullOrWhiteSpace(game)) // Example: Treat empty game name as warning or skip if needed
                {
                    ConfigManager.WriteConsoleWarning($"Row {currentRowNumber}: Game (column {GameIndex + 1}) is empty or whitespace.");
                    // Decide if you want to 'continue;' here to skip rows with empty games
                }


                // Access optional fields safely, providing default empty string if column doesn't exist
                // Check if the array is long enough before accessing the index
                string creationDate = (values.Length > CreationDateIndex) ? values[CreationDateIndex] : "";
                string version = (values.Length > VersionIndex) ? values[VersionIndex] : "";
                string romName = (values.Length > RomNameIndex) ? values[RomNameIndex] : "";
                string url = (values.Length > UrlIndex) ? values[UrlIndex] : "";
                string description = (values.Length > DescriptionIndex) ? values[DescriptionIndex] : "";
                string core = (values.Length > CoreIndex) ? values[CoreIndex] : "";
                string creator = (values.Length > CreatorIndex) ? values[CreatorIndex] : "";
                string notes = (values.Length > NotesIndex) ? values[NotesIndex] : "";

                // Call the existing AddCabinet method with data retrieved by position
                AddCabinet(catalogName, name, game, creationDate, version, romName, url, description, core, creator, notes);
            }

            ConfigManager.WriteConsole($"Successfully processed {rows.Count} data rows from TSV into catalog '{catalogName}'."); // Updated success message
        }
        catch (IOException ioEx) // More specific exception for file issues
        {
            ConfigManager.WriteConsoleException($"Failed to read TSV file at '{tsvFilePath}'", ioEx);
        }
        catch (Exception e) // General exception handler
        {
            ConfigManager.WriteConsoleException($"Failed to load cabinets from TSV", e);
        }
    }


    // Add this new TSV parser
    private List<string[]> ParseTsv(string tsvContent)
    {
        List<string[]> rows = new List<string[]>();
        List<string> currentRow = new List<string>();
        string currentField = "";
        bool inQuotes = false;

        for (int i = 0; i < tsvContent.Length; i++)
        {
            char c = tsvContent[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < tsvContent.Length && tsvContent[i + 1] == '"')
                {
                    // Escaped quote within quoted field
                    currentField += '"';
                    i++; // Skip the next quote
                }
                else
                {
                    inQuotes = !inQuotes; // Toggle quote state
                }
            }
            else if (c == '\t' && !inQuotes) // Tab as delimiter
            {
                currentRow.Add(currentField);
                currentField = "";
            }
            else if (c == '\n' && !inQuotes)
            {
                currentRow.Add(currentField);
                rows.Add(currentRow.ToArray());
                currentRow.Clear();
                currentField = "";
            }
            else if (c != '\r') // Ignore carriage returns
            {
                currentField += c;
            }
        }

        // Add the last field and row if not empty
        if (!string.IsNullOrEmpty(currentField) || currentRow.Count > 0)
        {
            currentRow.Add(currentField);
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }
    #if UNITY_EDITOR
        [CustomEditor(typeof(MarketplaceDatabase))]
        public class YourGameObjectScriptEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                MarketplaceDatabase script = (MarketplaceDatabase)target;

                if (GUILayout.Button("Refresh Catalogs from YAML"))
                {
                    script.RefreshCatalogsFromYaml(script.CatalogYamlFilePath); 
                }
                if (GUILayout.Button("Refresh Cabinets from TSV"))
                {
                    script.RefreshCabinetsFromTsv(script.CabinetsTSVCatalogName, script.CabinetsTSVFilePath);
                }
            //CabinetsTSVFilePath
        }
        }
    #endif
}