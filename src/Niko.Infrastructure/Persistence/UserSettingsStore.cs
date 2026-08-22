// ============================================================================
// Niko.Infrastructure — UserSettingsStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی IUserSettingsStore با SQLite برای ذخیرهٔ پروفایل کاربر.
//           پروفایل به‌صورت یک ردیف (id=1) ذخیره می‌شود؛ فیلدهای اختیاری می‌توانند null باشند.
// وابستگی‌ها و لایه: Infrastructure/Persistence → Core.Abstractions + Core.Domain.
// نکات تغییر و قیود: ذخیره اتمیک و سازگار با عقب‌ماندگی است. زمان/مقادیر مالی
//           به‌صورت متن با CultureInfo ثابت ذخیره می‌شوند تا محلی نشوند.
// ============================================================================

using System.Globalization;
using Microsoft.Data.Sqlite;
using Niko.Core.Abstractions;
using Niko.Core.Domain;

namespace Niko.Infrastructure.Persistence;

/// <summary>
/// ذخیره‌ساز پروفایل کاربر بر پایهٔ SQLite.
/// </summary>
public sealed class UserSettingsStore : IUserSettingsStore
{
    private readonly string _connectionString;

    public UserSettingsStore(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();
    }

    public async Task<UserProfile?> GetAsync(CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            SELECT quit_date_utc, cigarettes_per_day, price_per_cigarette, currency_code, preferred_locale,
                   price_per_pack, pack_size, display_name, avatar_id
            FROM user_profile
            WHERE id = 1;
            """;

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        DateTimeOffset? quitDate = reader.IsDBNull(0)
            ? null
            : DateTimeOffset.Parse(
                reader.GetString(0),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

        return new UserProfile
        {
            QuitDateUtc = quitDate,
            CigarettesPerDay = reader.IsDBNull(1) ? null : reader.GetInt32(1),
            PricePerCigarette = reader.IsDBNull(2)
                ? null
                : decimal.Parse(reader.GetString(2), CultureInfo.InvariantCulture),
            CurrencyCode = reader.IsDBNull(3) ? "USD" : reader.GetString(3),
            PreferredLocale = reader.IsDBNull(4) ? null : reader.GetString(4),
            PricePerPack = reader.IsDBNull(5)
                ? null
                : decimal.Parse(reader.GetString(5), CultureInfo.InvariantCulture),
            PackSize = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            DisplayName = reader.IsDBNull(7) ? null : reader.GetString(7),
            AvatarId = reader.IsDBNull(8) ? "niko-default" : reader.GetString(8),
        };
    }

    public async Task SaveAsync(UserProfile profile, CancellationToken ct = default)
    {
        await using var connection = OpenConnection();
        StoreSchema.EnsureSchema(connection);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO user_profile (id, quit_date_utc, cigarettes_per_day, price_per_cigarette, currency_code, preferred_locale, price_per_pack, pack_size, display_name, avatar_id)
            VALUES (1, $quit, $cpd, $price, $currency, $locale, $packPrice, $packSize, $displayName, $avatarId)
            ON CONFLICT(id) DO UPDATE SET
                quit_date_utc = excluded.quit_date_utc,
                cigarettes_per_day = excluded.cigarettes_per_day,
                price_per_cigarette = excluded.price_per_cigarette,
                currency_code = excluded.currency_code,
                preferred_locale = excluded.preferred_locale,
                price_per_pack = excluded.price_per_pack,
                pack_size = excluded.pack_size,
                display_name = excluded.display_name,
                avatar_id = excluded.avatar_id;
            """;
        cmd.Parameters.AddWithValue("$quit",
            profile.QuitDateUtc is { } q
                ? q.ToString("O", CultureInfo.InvariantCulture)
                : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$cpd",
            profile.CigarettesPerDay is { } cpd ? cpd : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$price",
            profile.PricePerCigarette is { } p
                ? p.ToString(CultureInfo.InvariantCulture)
                : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$currency", profile.CurrencyCode);
        cmd.Parameters.AddWithValue("$locale",
            profile.PreferredLocale is { } l ? l : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$packPrice",
            profile.PricePerPack is { } pp
                ? pp.ToString(CultureInfo.InvariantCulture)
                : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$packSize",
            profile.PackSize is { } ps ? ps : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$displayName",
            profile.DisplayName is { } name ? name : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$avatarId", profile.AvatarId);

        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
