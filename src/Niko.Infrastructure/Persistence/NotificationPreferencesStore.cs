// ============================================================================
// Niko.Infrastructure — NotificationPreferencesStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی INotificationPreferencesStore با SQLite. ترجیحات اعلان به‌صورت
//           یک ردیف (id=1) ذخیره می‌شوند؛ همهٔ دسته‌ها به‌صورت پیش‌فرض غیرفعال‌اند.
// وابستگی‌ها و لایه: Infrastructure/Persistence → Core.Abstractions + Core.Domain.
// نکات تغییر و قیود: ذخیره اتمیک و محلی است. زمان روز به‌صورت "HH:mm" ذخیره می‌شود.
// ============================================================================

using Microsoft.Data.Sqlite;
using Niko.Core.Abstractions;
using Niko.Core.Domain.Notifications;

namespace Niko.Infrastructure.Persistence;

/// <summary>
/// ذخیره‌ساز ترجیحات اعلان بر پایهٔ SQLite.
/// </summary>
public sealed class NotificationPreferencesStore : INotificationPreferencesStore
{
    private readonly string _connectionString;

    public NotificationPreferencesStore(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
    }

    public async Task<NotificationPreferences?> GetAsync(CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            SELECT daily_encouragement, milestone_reached, craving_support, savings_progress, time_of_day
            FROM notification_preferences
            WHERE id = 1;
            """;

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return new NotificationPreferences
        {
            DailyEncouragementEnabled = reader.GetInt32(0) == 1,
            MilestoneReachedEnabled = reader.GetInt32(1) == 1,
            CravingSupportEnabled = reader.GetInt32(2) == 1,
            SavingsProgressEnabled = reader.GetInt32(3) == 1,
            TimeOfDay = ParseTime(reader.IsDBNull(4) ? null : reader.GetString(4)),
        };
    }

    public async Task SaveAsync(NotificationPreferences preferences, CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO notification_preferences
                (id, daily_encouragement, milestone_reached, craving_support, savings_progress, time_of_day)
            VALUES (1, $d, $m, $c, $s, $time)
            ON CONFLICT(id) DO UPDATE SET
                daily_encouragement = excluded.daily_encouragement,
                milestone_reached = excluded.milestone_reached,
                craving_support = excluded.craving_support,
                savings_progress = excluded.savings_progress,
                time_of_day = excluded.time_of_day;
            """;
        cmd.Parameters.AddWithValue("$d", preferences.DailyEncouragementEnabled ? 1 : 0);
        cmd.Parameters.AddWithValue("$m", preferences.MilestoneReachedEnabled ? 1 : 0);
        cmd.Parameters.AddWithValue("$c", preferences.CravingSupportEnabled ? 1 : 0);
        cmd.Parameters.AddWithValue("$s", preferences.SavingsProgressEnabled ? 1 : 0);
        cmd.Parameters.AddWithValue("$time",
            preferences.TimeOfDay is { } t ? t.ToString("HH:mm") : (object)DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static TimeOnly? ParseTime(string? value)
        => value is null || !TimeOnly.TryParse(value, out var time) ? null : time;

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
