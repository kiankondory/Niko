// ============================================================================
// Niko.Infrastructure — TriggerAnalysisPreferenceStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی ITriggerAnalysisPreferenceStore با SQLite. ترجیح تحلیل محرک
//           به‌صورت یک ردیف (id=1) ذخیره می‌شود؛ پیش‌فرض امن «غیرفعال» است.
// وابستگی‌ها و لایه: Infrastructure/Persistence → Core.Abstractions + Core.Domain.
// نکات تغییر و قیود: ذخیره اتمیک و محلی است. هیچ دادهٔ رویداد حساسی در اینجا نیست.
// ============================================================================

using Microsoft.Data.Sqlite;
using Niko.Core.Abstractions;
using Niko.Core.Domain.TriggerAnalysis;

namespace Niko.Infrastructure.Persistence;

/// <summary>
/// ذخیره‌ساز ترجیح تحلیل محرک بر پایهٔ SQLite.
/// </summary>
public sealed class TriggerAnalysisPreferenceStore : ITriggerAnalysisPreferenceStore
{
    private readonly string _connectionString;

    public TriggerAnalysisPreferenceStore(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
    }

    public async Task<TriggerAnalysisPreference?> GetAsync(CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            SELECT enabled
            FROM trigger_analysis_preferences
            WHERE id = 1;
            """;

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return new TriggerAnalysisPreference { Enabled = reader.GetInt32(0) == 1 };
    }

    public async Task SaveAsync(TriggerAnalysisPreference preference, CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO trigger_analysis_preferences (id, enabled)
            VALUES (1, $enabled)
            ON CONFLICT(id) DO UPDATE SET enabled = excluded.enabled;
            """;
        cmd.Parameters.AddWithValue("$enabled", preference.Enabled ? 1 : 0);

        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
