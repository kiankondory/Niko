// ============================================================================
// Niko.Infrastructure — StoreSchema.cs
// ----------------------------------------------------------------------------
// مسئولیت: تعریف و مدیریت نسخهٔ طرح پایگاه‌داده و اجرای مهاجرت‌های سازگار با
//           عقب‌ماندگی. نگهداری از جدول فرادادهٔ نسخه و اعمال مهاجرت به‌ترتیب.
// وابستگی‌ها و لایه: لایهٔ Infrastructure/Persistence؛ با SQLite کار می‌کند.
// نکات تغییر و قیود: مهاجرت‌ها باید سازگار با عقب‌ماندگی باشند و هیچ‌گاه دادهٔ
//           کاربر را به‌صورت مخرب حذف نکنند. افزودن نسخهٔ جدید باید در
//           DECISIONS.md ثبت شود.
// ============================================================================

using Microsoft.Data.Sqlite;

namespace Niko.Infrastructure.Persistence;

/// <summary>
/// نسخهٔ فعلی طرح و منطق مهاجرت.
/// </summary>
internal static class StoreSchema
{
    /// <summary>نسخهٔ فعلی طرح پایگاه‌داده.</summary>
    public const int CurrentVersion = 8;

    /// <summary>
    /// اطمینان از وجود جدول فرادادهٔ نسخه و ارتقای طرح به نسخهٔ جاری.
    /// </summary>
    public static void EnsureSchema(SqliteConnection connection)
    {
        using var init = connection.CreateCommand();
        init.CommandText =
            """
            CREATE TABLE IF NOT EXISTS schema_meta (
                version INTEGER NOT NULL
            );
            """;
        init.ExecuteNonQuery();

        var current = ReadVersion(connection);

        if (current < 1)
        {
            ApplyMigrationV1(connection);
        }

        if (current < 2)
        {
            ApplyMigrationV2(connection);
        }

        if (current < 3)
        {
            ApplyMigrationV3(connection);
        }

        if (current < 4)
        {
            ApplyMigrationV4(connection);
        }

        if (current < 5)
        {
            ApplyMigrationV5(connection);
        }

        if (current < 6)
        {
            ApplyMigrationV6(connection);
        }

        if (current < 7)
        {
            ApplyMigrationV7(connection);
        }

        if (current < 8)
        {
            ApplyMigrationV8(connection);
        }

        if (current < CurrentVersion)
        {
            WriteVersion(connection, CurrentVersion);
        }
    }

    private static int ReadVersion(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT MAX(version) FROM schema_meta;";
        var result = cmd.ExecuteScalar();
        return result is null || result is DBNull ? 0 : Convert.ToInt32(result);
    }

    private static void WriteVersion(SqliteConnection connection, int version)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO schema_meta (version) VALUES ($v);";
        cmd.Parameters.AddWithValue("$v", version);
        cmd.ExecuteNonQuery();
    }

    private static void ApplyMigrationV1(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS events (
                event_id       TEXT NOT NULL PRIMARY KEY,
                occurred_at_utc TEXT NOT NULL,
                source         INTEGER NOT NULL,
                type           INTEGER NOT NULL,
                sync_status    INTEGER NOT NULL,
                metadata       TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_events_occurred ON events (occurred_at_utc);
            CREATE INDEX IF NOT EXISTS idx_events_sync ON events (sync_status);
            """;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyMigrationV2(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS user_profile (
                id                 INTEGER PRIMARY KEY CHECK (id = 1),
                quit_date_utc      TEXT,
                cigarettes_per_day INTEGER,
                price_per_cigarette TEXT,
                currency_code      TEXT,
                preferred_locale   TEXT
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyMigrationV3(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            ALTER TABLE user_profile ADD COLUMN price_per_pack TEXT;
            ALTER TABLE user_profile ADD COLUMN pack_size INTEGER;
            """;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyMigrationV4(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS notification_preferences (
                id                   INTEGER PRIMARY KEY CHECK (id = 1),
                daily_encouragement   INTEGER NOT NULL DEFAULT 0,
                milestone_reached     INTEGER NOT NULL DEFAULT 0,
                craving_support       INTEGER NOT NULL DEFAULT 0,
                savings_progress      INTEGER NOT NULL DEFAULT 0,
                time_of_day           TEXT
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyMigrationV5(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS trigger_analysis_preferences (
                id      INTEGER PRIMARY KEY CHECK (id = 1),
                enabled INTEGER NOT NULL DEFAULT 0
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyMigrationV6(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            ALTER TABLE user_profile ADD COLUMN display_name TEXT;
            ALTER TABLE user_profile ADD COLUMN avatar_id TEXT NOT NULL DEFAULT 'niko-default';
            """;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyMigrationV7(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS coach_preferences (
                id                         INTEGER PRIMARY KEY CHECK (id = 1),
                enabled                    INTEGER NOT NULL DEFAULT 0,
                allow_external_provider   INTEGER NOT NULL DEFAULT 0,
                allow_aggregated_progress INTEGER NOT NULL DEFAULT 0,
                allow_craving_context     INTEGER NOT NULL DEFAULT 0
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyMigrationV8(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS processed_companion_messages (
                message_id       TEXT NOT NULL PRIMARY KEY,
                processed_at_utc TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }
}
