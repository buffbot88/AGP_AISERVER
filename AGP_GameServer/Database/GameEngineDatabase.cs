using System.Text.Json;
using Abstractions;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace LegendaryGameSystem.Database
{
    /// <summary>
    /// Database persistence layer for Game Engine scenes and entities.
    /// Provides SQLite Storage for game state across server restarts.
    /// </summary>
    public class GameEngineDatabase : IDisposable
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

        public GameEngineDatabase(string? dbPath = null)
        {
            Batteries_V2.Init();
            _dbPath = string.IsNullOrWhiteSpace(dbPath)
                ? Path.Combine(AppContext.BaseDirectory, "Databases", "game_engine.sqlite")
                : dbPath;

            // Ensure Databases directory exists
            var directory = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = $"Data Source={_dbPath}";
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            // Scenes table
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Scenes (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    CreatedAt TEXT NOT NULL,
    CreatedBy TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Metadata TEXT
);

CREATE TABLE IF NOT EXISTS Entities (
    Id TEXT PRIMARY KEY,
    SceneId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Type TEXT NOT NULL,
    PositionX REAL NOT NULL DEFAULT 0,
    PositionY REAL NOT NULL DEFAULT 0,
    PositionZ REAL NOT NULL DEFAULT 0,
    RotationX REAL NOT NULL DEFAULT 0,
    RotationY REAL NOT NULL DEFAULT 0,
    RotationZ REAL NOT NULL DEFAULT 0,
    ScaleX REAL NOT NULL DEFAULT 1,
    ScaleY REAL NOT NULL DEFAULT 1,
    ScaleZ REAL NOT NULL DEFAULT 1,
    Properties TEXT,
    CreatedAt TEXT NOT NULL,
    CreatedBy TEXT NOT NULL,
    FOREIGN KEY (SceneId) REFERENCES Scenes(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_entities_sceneid ON Entities(SceneId);
CREATE INDEX IF NOT EXISTS idx_scenes_isactive ON Scenes(IsActive);
";
            cmd.ExecuteNonQuery();
        }

        #region Scene Operations

        public void SaveScene(GameScene scene)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
INSERT OR REPLACE INTO Scenes (Id, Name, Description, CreatedAt, CreatedBy, IsActive, Metadata)
VALUES ($id, $name, $description, $createdAt, $createdBy, $isActive, $metadata);";

            cmd.Parameters.AddWithValue("$id", scene.Id);
            cmd.Parameters.AddWithValue("$name", scene.Name);
            cmd.Parameters.AddWithValue("$description", scene.Description ?? "");
            cmd.Parameters.AddWithValue("$createdAt", scene.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$createdBy", scene.CreatedBy);
            cmd.Parameters.AddWithValue("$isActive", scene.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("$metadata", JsonSerializer.Serialize(scene.Metadata, _jsonOptions));

            cmd.ExecuteNonQuery();
        }

        public GameScene? LoadScene(string sceneId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
SELECT Id, Name, Description, CreatedAt, CreatedBy, IsActive, Metadata
FROM Scenes
WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", sceneId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var scene = new GameScene
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    CreatedBy = reader.GetString(4),
                    IsActive = reader.GetInt32(5) == 1,
                    Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(6)) ?? new()
                };

                // Load entities for this scene
                scene.Entities = LoadEntities(sceneId);

                return scene;
            }

            return null;
        }

        public List<GameScene> LoadAllScenes()
        {
            var scenes = new List<GameScene>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
SELECT Id, Name, Description, CreatedAt, CreatedBy, IsActive, Metadata
FROM Scenes
ORDER BY CreatedAt DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var scene = new GameScene
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    CreatedBy = reader.GetString(4),
                    IsActive = reader.GetInt32(5) == 1,
                    Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(6)) ?? new()
                };

                // Load entities for this scene
                scene.Entities = LoadEntities(scene.Id);

                scenes.Add(scene);
            }

            return scenes;
        }

        public bool DeleteScene(string sceneId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"DELETE FROM Scenes WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", sceneId);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        #endregion

        #region Entity Operations

        public void SaveEntity(string sceneId, GameEntity entity)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
INSERT OR REPLACE INTO Entities (
    Id, SceneId, Name, Type,
    PositionX, PositionY, PositionZ,
    RotationX, RotationY, RotationZ,
    ScaleX, ScaleY, ScaleZ,
    Properties, CreatedAt, CreatedBy
)
VALUES (
    $id, $sceneId, $name, $type,
    $posX, $posY, $posZ,
    $rotX, $rotY, $rotZ,
    $scaleX, $scaleY, $scaleZ,
    $properties, $createdAt, $createdBy
);";

            cmd.Parameters.AddWithValue("$id", entity.Id);
            cmd.Parameters.AddWithValue("$sceneId", sceneId);
            cmd.Parameters.AddWithValue("$name", entity.Name);
            cmd.Parameters.AddWithValue("$type", entity.Type);
            cmd.Parameters.AddWithValue("$posX", entity.Position.X);
            cmd.Parameters.AddWithValue("$posY", entity.Position.Y);
            cmd.Parameters.AddWithValue("$posZ", entity.Position.Z);
            cmd.Parameters.AddWithValue("$rotX", entity.Rotation.X);
            cmd.Parameters.AddWithValue("$rotY", entity.Rotation.Y);
            cmd.Parameters.AddWithValue("$rotZ", entity.Rotation.Z);
            cmd.Parameters.AddWithValue("$scaleX", entity.Scale.X);
            cmd.Parameters.AddWithValue("$scaleY", entity.Scale.Y);
            cmd.Parameters.AddWithValue("$scaleZ", entity.Scale.Z);
            cmd.Parameters.AddWithValue("$properties", JsonSerializer.Serialize(entity.Properties, _jsonOptions));
            cmd.Parameters.AddWithValue("$createdAt", entity.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$createdBy", entity.CreatedBy);

            cmd.ExecuteNonQuery();
        }

        public List<GameEntity> LoadEntities(string sceneId)
        {
            var entities = new List<GameEntity>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
SELECT Id, Name, Type,
       PositionX, PositionY, PositionZ,
       RotationX, RotationY, RotationZ,
       ScaleX, ScaleY, ScaleZ,
       Properties, CreatedAt, CreatedBy
FROM Entities
WHERE SceneId = $sceneId
ORDER BY CreatedAt ASC;";
            cmd.Parameters.AddWithValue("$sceneId", sceneId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var entity = new GameEntity
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Position = new Vector3
                    {
                        X = (float)reader.GetDouble(3),
                        Y = (float)reader.GetDouble(4),
                        Z = (float)reader.GetDouble(5)
                    },
                    Rotation = new Vector3
                    {
                        X = (float)reader.GetDouble(6),
                        Y = (float)reader.GetDouble(7),
                        Z = (float)reader.GetDouble(8)
                    },
                    Scale = new Vector3
                    {
                        X = (float)reader.GetDouble(9),
                        Y = (float)reader.GetDouble(10),
                        Z = (float)reader.GetDouble(11)
                    },
                    Properties = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(12)) ?? new(),
                    CreatedAt = DateTime.Parse(reader.GetString(13)),
                    CreatedBy = reader.GetString(14)
                };

                entities.Add(entity);
            }

            return entities;
        }

        public bool DeleteEntity(string entityId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"DELETE FROM Entities WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", entityId);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        #endregion

        #region Statistics

        public (int totalScenes, int activeScenes, int totalEntities) GetStatistics()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
SELECT 
    (SELECT COUNT(*) FROM Scenes) as total_scenes,
    (SELECT COUNT(*) FROM Scenes WHERE IsActive = 1) as active_scenes,
    (SELECT COUNT(*) FROM Entities) as total_entities;";

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetInt32(2)
                );
            }

            return (0, 0, 0);
        }

        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
