// ============================================================================
// Niko.Infrastructure — CoachPreferencesStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: ذخیرهٔ محلی ترجیحات مربی با SQLite و پیش‌فرض‌های حریم‌خصوصی امن.
// وابستگی‌ها و لایه: Persistence در Infrastructure؛ قرارداد Core را پیاده می‌کند.
// نکات تغییر و قیود: فقط یک ردیف تنظیمات ذخیره می‌شود؛ حذف آن دادهٔ مربی را پاک
//           می‌کند و هیچ رویداد خام یا دادهٔ provider خارجی ذخیره نمی‌شود.
// ============================================================================

using Microsoft.Data.Sqlite;
using Niko.Core.Abstractions;
using Niko.Core.Domain.Coach;

namespace Niko.Infrastructure.Persistence;

public sealed class CoachPreferencesStore : ICoachPreferencesStore
{
    private readonly string _connectionString;

    public CoachPreferencesStore(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
    }

    public async Task<CoachPreferences?> GetAsync(CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT enabled, allow_external_provider, allow_aggregated_progress, allow_craving_context
            FROM coach_preferences
            WHERE id = 1;
            """;

        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return new CoachPreferences
        {
            Enabled = reader.GetInt32(0) == 1,
            AllowExternalProvider = reader.GetInt32(1) == 1,
            AllowAggregatedProgressContext = reader.GetInt32(2) == 1,
            AllowCravingContext = reader.GetInt32(3) == 1,
        };
    }

    public async Task SaveAsync(CoachPreferences preferences, CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO coach_preferences
                (id, enabled, allow_external_provider, allow_aggregated_progress, allow_craving_context)
            VALUES (1, $enabled, $external, $progress, $craving)
            ON CONFLICT(id) DO UPDATE SET
                enabled = excluded.enabled,
                allow_external_provider = excluded.allow_external_provider,
                allow_aggregated_progress = excluded.allow_aggregated_progress,
                allow_craving_context = excluded.allow_craving_context;
            """;
        command.Parameters.AddWithValue("$enabled", preferences.Enabled ? 1 : 0);
        command.Parameters.AddWithValue("$external", preferences.AllowExternalProvider ? 1 : 0);
        command.Parameters.AddWithValue("$progress", preferences.AllowAggregatedProgressContext ? 1 : 0);
        command.Parameters.AddWithValue("$craving", preferences.AllowCravingContext ? 1 : 0);
        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM coach_preferences WHERE id = 1;";
        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
