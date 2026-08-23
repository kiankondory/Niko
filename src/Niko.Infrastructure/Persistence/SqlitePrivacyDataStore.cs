// ============================================================================
// Niko.Infrastructure — SqlitePrivacyDataStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: export JSON محلی و پاک‌سازی تراکنشی تمام داده‌های Niko در SQLite.
// وابستگی‌ها و لایه: Infrastructure → Core IPrivacyDataStore + Microsoft.Data.Sqlite.
// نکات تغییر و قیود: schema حذف نمی‌شود؛ erase فقط پس از رضایت UI اجرا می‌شود و شبکه ندارد.
// ============================================================================

using System.Text.Json;
using Microsoft.Data.Sqlite;
using Niko.Core.Abstractions;

namespace Niko.Infrastructure.Persistence;

public sealed class SqlitePrivacyDataStore(string databasePath) : IPrivacyDataStore
{
    private readonly string _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath, Mode = SqliteOpenMode.ReadWriteCreate }.ToString();

    public async Task<string> ExportJsonAsync(CancellationToken ct = default)
    {
        await using var connection = Open();
        StoreSchema.EnsureSchema(connection);
        var payload = new Dictionary<string, object?>();
        foreach (var table in Tables)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {table};";
            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
            var rows = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var row = new Dictionary<string, object?>();
                for (var index = 0; index < reader.FieldCount; index++) row[reader.GetName(index)] = reader.IsDBNull(index) ? null : reader.GetValue(index);
                rows.Add(row);
            }
            payload[table] = rows;
        }
        return JsonSerializer.Serialize(new { format = "niko-local-export-v1", exportedAtUtc = DateTimeOffset.UtcNow, data = payload });
    }

    public async Task EraseAllAsync(CancellationToken ct = default)
    {
        await using var connection = Open();
        StoreSchema.EnsureSchema(connection);
        await using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var table in Tables)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $"DELETE FROM {table};";
                await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
            await transaction.CommitAsync(ct).ConfigureAwait(false);
        }
        catch { await transaction.RollbackAsync(ct).ConfigureAwait(false); throw; }
    }

    private SqliteConnection Open() { var connection = new SqliteConnection(_connectionString); connection.Open(); return connection; }
    private static readonly string[] Tables = ["events", "user_profile", "notification_preferences", "trigger_analysis_preferences", "coach_preferences", "processed_companion_messages"];
}
