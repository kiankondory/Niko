// ============================================================================
// Niko.Infrastructure — SqliteProcessedMessageStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: نگهداری پایدار و حداقلی شناسهٔ پیام‌های Companion برای idempotency.
// وابستگی‌ها و لایه: Infrastructure/Persistence → Core.Abstractions و SQLite.
// نکات تغییر و قیود: فقط شناسه و زمان ثبت ذخیره می‌شود؛ دادهٔ رویداد حذف یا تغییر
// نمی‌کند و درج تکراری را به‌صورت اتمیک رد می‌کند.
// ============================================================================

using System.Globalization;
using Microsoft.Data.Sqlite;
using Niko.Core.Abstractions;

namespace Niko.Infrastructure.Persistence;

/// <summary>ذخیره‌ساز پایدار شناسهٔ پیام‌های Companion.</summary>
public sealed class SqliteProcessedMessageStore : IProcessedMessageStore
{
    private readonly string _connectionString;

    public SqliteProcessedMessageStore(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
    }

    public async Task<bool> TryMarkProcessedAsync(
        string messageId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            return false;
        }

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);
        StoreSchema.EnsureSchema(connection);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO processed_companion_messages (message_id, processed_at_utc)
            VALUES ($id, $processed)
            ON CONFLICT(message_id) DO NOTHING;
            """;
        command.Parameters.AddWithValue("$id", messageId);
        command.Parameters.AddWithValue(
            "$processed",
            DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));

        return await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false) == 1;
    }
}
