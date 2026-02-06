using Microsoft.Data.Sqlite;
using SillowApp.Models;

namespace SillowApp.Data
{
    public static class Database
    {
        private static string _dbPath;
        private static bool _initialized;

        public static string DbPath =>
            _dbPath ??= Path.Combine(FileSystem.AppDataDirectory, "sillow.db3");

        public static async Task InitAsync(string overridePath = null)
        {
            if (_initialized) return;

            if (!string.IsNullOrWhiteSpace(overridePath))
                _dbPath = overridePath;

            Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);

            SQLitePCL.Batteries_V2.Init();

            using var conn = new SqliteConnection($"Data Source={DbPath}");
            await conn.OpenAsync();

            using (var pragma = conn.CreateCommand())
            {
                pragma.CommandText = "PRAGMA journal_mode = WAL; PRAGMA foreign_keys = ON;";
                await pragma.ExecuteNonQueryAsync();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS Jobs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    Status TEXT,
                    CreatedAt TEXT NOT NULL,
                    Latitude REAL,
                    Longitude REAL,
                    JobAddress TEXT,
                    CustomerName TEXT,
                    CustomerEmail TEXT,
                    CustomerPhone TEXT,
                    JobNotes TEXT,
                    JobDateTime TEXT,
                    JobCost REAL
                );

                CREATE INDEX IF NOT EXISTS IX_Jobs_Status ON Jobs(Status);
                CREATE INDEX IF NOT EXISTS IX_Jobs_CreatedAt ON Jobs(CreatedAt);
                ";

                await cmd.ExecuteNonQueryAsync();
            }

            _initialized = true;
        }

        private static SqliteConnection Open()
        {
            var conn = new SqliteConnection($"Data Source={DbPath}");
            conn.Open();
            return conn;
        }

        public static async Task<int> AddJobAsync(Job job)
        {
            if (!_initialized) await InitAsync();
            job.CreatedAt = job.CreatedAt == default ? DateTime.UtcNow : job.CreatedAt;

            using var conn = Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText =
            @"
            INSERT INTO Jobs (Title, Description, Status, CreatedAt, Latitude, Longitude, 
            JobAddress, CustomerName, CustomerEmail, CustomerPhone, JobNotes, JobDateTime, JobCost)
            VALUES ($title, $desc, $status, $created, $lat, $lng, $address, $name, $email, $phone, $notes, $jobdt, $cost);
            SELECT last_insert_rowid();
            ";

            cmd.Parameters.AddWithValue("$title", job.Title ?? "");
            cmd.Parameters.AddWithValue("$desc", (object?)job.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$status", (object?)job.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$created", job.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$lat", (object?)job.Latitude ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$lng", (object?)job.Longitude ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$address", (object?)job.JobAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$name", (object?)job.CustomerName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$email", (object?)job.CustomerEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$phone", (object?)job.CustomerPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$notes", (object?)job.JobNotes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$jobdt", (object?)job.JobDateTime?.ToString("o") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$cost", (object?)job.JobCost ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public static async Task<List<Job>> GetJobsAsync()
        {
            if (!_initialized) await InitAsync();

            var list = new List<Job>();
            using var conn = Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
            SELECT Id, Title, Description, Status, CreatedAt, Latitude, Longitude,
                   JobAddress, CustomerName, CustomerEmail, CustomerPhone, JobNotes,
                   JobDateTime, JobCost  -- New columns added to the SELECT list
            FROM Jobs
            ORDER BY CreatedAt DESC;
            ";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Job
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Status = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind),
                    Latitude = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                    Longitude = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                    JobAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CustomerName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    CustomerEmail = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CustomerPhone = reader.IsDBNull(10) ? null : reader.GetString(10),
                    JobNotes = reader.IsDBNull(11) ? null : reader.GetString(11),
                    JobDateTime = reader.IsDBNull(12) ? (DateTime?)null : DateTime.Parse(reader.GetString(12), null, System.Globalization.DateTimeStyles.RoundtripKind),
                    JobCost = reader.IsDBNull(13) ? (decimal?)null : reader.GetDecimal(13),
                });
            }
            return list;
        }

        public static async Task<int> UpdateJobAsync(Job job)
        {
            if (!_initialized) await InitAsync();

            using var conn = Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText =
            @"
            UPDATE Jobs
            SET Title = $title,
                Description = $desc,
                Status = $status,
                Latitude = $lat,
                Longitude = $lng,
                JobAddress = $address,
                CustomerName = $name,
                CustomerEmail = $email,
                CustomerPhone = $phone,
                JobNotes = $notes,
                JobDateTime = $jobdt,
                JobCost = $cost
            WHERE Id = $id;
            ";

            cmd.Parameters.AddWithValue("$title", job.Title ?? "");
            cmd.Parameters.AddWithValue("$desc", (object?)job.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$status", (object?)job.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$lat", (object?)job.Latitude ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$lng", (object?)job.Longitude ?? DBNull.Value);

            cmd.Parameters.AddWithValue("$address", (object?)job.JobAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$name", (object?)job.CustomerName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$email", (object?)job.CustomerEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$phone", (object?)job.CustomerPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$notes", (object?)job.JobNotes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$jobdt", (object?)job.JobDateTime?.ToString("o") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$cost", (object?)job.JobCost ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$id", job.Id);

            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> DeleteJobAsync(int id)
        {
            if (!_initialized) await InitAsync();

            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Jobs WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> GetJobCountAsync()
        {
            if (!_initialized) await InitAsync();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Jobs;";
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public static async Task<int> GetBookedJobCountAsync()
        {
            if (!_initialized) await InitAsync();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Jobs WHERE Status = 'Job Booked';";
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public static async Task<int> GetCompletedJobCountAsync()
        {
            if (!_initialized) await InitAsync();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Jobs WHERE Status = 'Completed';";
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public static async Task<int> GetReturnVisitsCountAsync()
        {
            if (!_initialized) await InitAsync();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) 
                FROM Jobs 
                WHERE LOWER(Status) IN ('come back later', 'do not return');
            "; 
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public static async Task<Job?> GetMostRecentJobAsync()
        {
            if (!_initialized) await InitAsync();
            using var conn = Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
            SELECT Id, Title, Description, Status, CreatedAt, Latitude, Longitude,
                   JobAddress, CustomerName, CustomerEmail, CustomerPhone, JobNotes,
                   JobDateTime, JobCost
            FROM Jobs
            ORDER BY CreatedAt DESC
            LIMIT 1;
            ";

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Job
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Status = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind),
                    Latitude = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                    Longitude = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                    JobAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CustomerName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    CustomerEmail = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CustomerPhone = reader.IsDBNull(10) ? null : reader.GetString(10),
                    JobNotes = reader.IsDBNull(11) ? null : reader.GetString(11),
                    JobDateTime = reader.IsDBNull(12) ? (DateTime?)null : DateTime.Parse(reader.GetString(12), null, System.Globalization.DateTimeStyles.RoundtripKind),
                    JobCost = reader.IsDBNull(13) ? (decimal?)null : reader.GetDecimal(13)
                };
            }
            return null;
        }

        public static async Task<Job?> GetUpcomingJobAsync()
        {
            if (!_initialized) await InitAsync();
            using var conn = Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
            SELECT Id, Title, Description, Status, CreatedAt, Latitude, Longitude,
                   JobAddress, CustomerName, CustomerEmail, CustomerPhone, JobNotes,
                   JobDateTime, JobCost
            FROM Jobs
            WHERE JobDateTime IS NOT NULL
            ORDER BY JobDateTime ASC
            LIMIT 1;
            ";

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Job
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Status = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind),
                    Latitude = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                    Longitude = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                    JobAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CustomerName = reader.IsDBNull(8) ? null : reader.GetString(8),
                    CustomerEmail = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CustomerPhone = reader.IsDBNull(10) ? null : reader.GetString(10),
                    JobNotes = reader.IsDBNull(11) ? null : reader.GetString(11),
                    JobDateTime = reader.IsDBNull(12) ? (DateTime?)null : DateTime.Parse(reader.GetString(12), null, System.Globalization.DateTimeStyles.RoundtripKind),
                    JobCost = reader.IsDBNull(13) ? (decimal?)null : reader.GetDecimal(13)
                };
            }

            return null;
        }
    }
}