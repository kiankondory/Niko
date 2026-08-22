// ============================================================================
// Niko.Infrastructure — SqliteStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی ILocalStore با SQLite (Microsoft.Data.Sqlite). رویدادها
//           ابتدا به‌صورت محلی و پایدار ذخیره می‌شوند تا رفتار آفلاین حفظ شود.
// وابستگی‌ها و لایه: Infrastructure/Persistence → Core.Abstractions + Events.
// نکات تغییر و قیود: ذخیرهٔ رویداد idempotent است (کلید اصلی event_id). زمان
//           به‌صورت ISO-8601 UTC ذخیره می‌شود. همهٔ عملیات اتمیک‌اند.
// ============================================================================

using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Niko.Core.Abstractions;
using Niko.Core.Events;

namespace Niko.Infrastructure.Persistence;

/// <summary>
/// ذخیره‌ساز رویدادها بر پایهٔ SQLite.
/// </summary>
public sealed class SqliteStore : ILocalStore
{
    private readonly string _connectionString;

    public SqliteStore(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
    }

    public async Task SaveEventAsync(LogEvent logEvent, CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO events (event_id, occurred_at_utc, source, type, sync_status, metadata)
            VALUES ($id, $occurred, $source, $type, $sync, $metadata)
            ON CONFLICT(event_id) DO NOTHING;
            """;
        cmd.Parameters.AddWithValue("$id", logEvent.EventId);
        cmd.Parameters.AddWithValue("$occurred", logEvent.OccurredAtUtc.ToString("O", CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$source", (int)logEvent.Source);
        cmd.Parameters.AddWithValue("$type", (int)logEvent.Type);
        cmd.Parameters.AddWithValue("$sync", (int)logEvent.SyncStatus);
        cmd.Parameters.AddWithValue("$metadata", JsonSerializer.Serialize(logEvent.Metadata));

        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<LogEvent>> GetEventsAsync(
        int offset = 0,
        int limit = 100,
        CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            SELECT event_id, occurred_at_utc, source, type, sync_status, metadata
            FROM events
            ORDER BY occurred_at_utc
            LIMIT $limit OFFSET $offset;
            """;
        cmd.Parameters.AddWithValue("$limit", limit);
        cmd.Parameters.AddWithValue("$offset", offset);

        return await ReadRowsAsync(cmd, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<LogEvent>> GetPendingEventsAsync(
        int limit = 100,
        CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            SELECT event_id, occurred_at_utc, source, type, sync_status, metadata
            FROM events
            WHERE sync_status IN (0, 2)
            ORDER BY occurred_at_utc
            LIMIT $limit;
            """;
        cmd.Parameters.AddWithValue("$limit", limit);

        return await ReadRowsAsync(cmd, ct).ConfigureAwait(false);
    }

    public async Task UpdateSyncStatusAsync(
        string eventId,
        SyncStatus status,
        CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE events SET sync_status = $sync WHERE event_id = $id;";
        cmd.Parameters.AddWithValue("$sync", (int)status);
        cmd.Parameters.AddWithValue("$id", eventId);

        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static async Task<IReadOnlyList<LogEvent>> ReadRowsAsync(
        SqliteCommand cmd,
        CancellationToken ct)
    {
        var result = new List<LogEvent>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            var occurredAtUtc = DateTimeOffset.Parse(
                reader.GetString(1),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(
                reader.GetString(5)) ?? new Dictionary<string, string>();

            result.Add(new LogEvent(
                reader.GetString(0),
                occurredAtUtc,
                (EventSource)reader.GetInt32(2),
                (EventType)reader.GetInt32(3),
                (SyncStatus)reader.GetInt32(4),
                metadata));
        }

        return result;
    }
}
