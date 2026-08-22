// ============================================================================
// Niko.Core — LocalizationKeys.cs
// ----------------------------------------------------------------------------
// مسئولیت: کلیدهای پایدار و توصیفی محلی‌سازی. ترجمه‌ها در منابع پلتفرمی جدا
//           نگهداری می‌شوند؛ هسته فقط این کلیدها را می‌شناسد.
// وابستگی‌ها و لایه: بخش Localization در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: کلیدها مستقل از طول متن یا جهت ترجمه‌اند. افزودن کلید جدید
//           باید در تمام منابع locale منعکس شود تا fallback درست کار کند.
// ============================================================================

namespace Niko.Core.Localization;

/// <summary>
/// کلیدهای پایدار محلی‌سازی. مقدار هر کلید در منابع locale تعریف می‌شود.
/// </summary>
public static class LocalizationKeys
{
    /// <summary>عنوان برنامه.</summary>
    public const string AppTitle = "AppTitle";

    /// <summary>برچسب دکمهٔ ثبت «سیگار کشیدم».</summary>
    public const string QuickLogSmoked = "QuickLog.Smoked";

    /// <summary>برچسب دکمهٔ ثبت «مقاومت کردم».</summary>
    public const string QuickLogResisted = "QuickLog.Resisted";

    /// <summary>برچسب دکمهٔ ثبت «هوس دارم».</summary>
    public const string QuickLogCraving = "QuickLog.Craving";

    /// <summary>پیام موفقیت ثبت رویداد.</summary>
    public const string QuickLogSuccess = "QuickLog.Success";

    /// <summary>پیام خطای ثبت رویداد.</summary>
    public const string QuickLogError = "QuickLog.Error";

    /// <summary>وضعیت همگام‌سازی در انتظار.</summary>
    public const string SyncPending = "Sync.Pending";

    /// <summary>وضعیت همگام‌سازی انجام‌شده.</summary>
    public const string SyncInSync = "Sync.InSync";

    /// <summary>وضعیت همگام‌سازی ناموفق.</summary>
    public const string SyncFailed = "Sync.Failed";

    // --- داشبورد ---

    /// <summary>عنوان داشبورد.</summary>
    public const string DashboardTitle = "Dashboard.Title";

    /// <summary>برچسب شمارندهٔ مصرف.</summary>
    public const string DashboardSmoked = "Dashboard.Smoked";

    /// <summary>برچسب شمارندهٔ مقاومت.</summary>
    public const string DashboardResisted = "Dashboard.Resisted";

    /// <summary>برچسب شمارندهٔ هوس.</summary>
    public const string DashboardCravings = "Dashboard.Cravings";

    /// <summary>برچسب استریک فعلی.</summary>
    public const string DashboardCurrentStreak = "Dashboard.CurrentStreak";

    /// <summary>برچسب روز.</summary>
    public const string DashboardDays = "Dashboard.Days";

    /// <summary>برچسب میل‌استون.</summary>
    public const string DashboardMilestone = "Dashboard.Milestone";

    /// <summary>قالب «به سمت روز X».</summary>
    public const string DashboardMilestoneNext = "Dashboard.MilestoneNext";

    /// <summary>برچسب صرفه‌جویی تقریبی.</summary>
    public const string DashboardSavings = "Dashboard.Savings";

    /// <summary>پیام نبود دادهٔ کافی برای صرفه‌جویی.</summary>
    public const string DashboardSavingsUnavailable = "Dashboard.SavingsUnavailable";

    /// <summary>برچسب «رویدادی ثبت نشده است».</summary>
    public const string DashboardEmpty = "Dashboard.Empty";

    /// <summary>سلب مسئولیت صرفه‌جویی تقریبی.</summary>
    public const string DashboardSavingsDisclaimer = "Dashboard.SavingsDisclaimer";

    /// <summary>برچسب دکمهٔ بازخوانی.</summary>
    public const string Refresh = "Common.Refresh";

    /// <summary>عنوان تب ثبت سریع.</summary>
    public const string TabQuickLog = "Tab.QuickLog";

    // --- میل‌استون‌ها ---

    /// <summary>عنوان بخش میل‌استون‌ها.</summary>
    public const string DashboardMilestones = "Dashboard.Milestones";

    /// <summary>برچسب میل‌استون تکمیل‌شده.</summary>
    public const string MilestoneCompleted = "Milestone.Completed";

    /// <summary>برچسب میل‌استون فعلی.</summary>
    public const string MilestoneCurrent = "Milestone.Current";

    /// <summary>برچسب میل‌استون آینده.</summary>
    public const string MilestoneUpcoming = "Milestone.Upcoming";

    /// <summary>قالب روزهای میل‌استون.</summary>
    public const string MilestoneDays = "Milestone.Days";

    // --- نبرد با هوس ---

    /// <summary>عنوان نبرد با هوس.</summary>
    public const string CravingBattleTitle = "CravingBattle.Title";

    /// <summary>دکمهٔ ورود به نبرد با هوس.</summary>
    public const string CravingBattleEntry = "CravingBattle.Entry";

    /// <summary>درخواست انتخاب شدت هوس.</summary>
    public const string CravingBattleSelectIntensity = "CravingBattle.SelectIntensity";

    /// <summary>هوس ملایم.</summary>
    public const string CravingIntensityMild = "CravingBattle.Intensity.Mild";

    /// <summary>هوس متوسط.</summary>
    public const string CravingIntensityModerate = "CravingBattle.Intensity.Moderate";

    /// <summary>هوس شدید.</summary>
    public const string CravingIntensityIntense = "CravingBattle.Intensity.Intense";

    /// <summary>درخواست انتخاب یک مداخله.</summary>
    public const string CravingBattleChooseAction = "CravingBattle.ChooseAction";

    /// <summary>تنفس عمیق.</summary>
    public const string InterventionDeepBreathing = "CravingBattle.Action.DeepBreathing";

    /// <summary>تأخیر/تایمر.</summary>
    public const string InterventionDelay = "CravingBattle.Action.Delay";

    /// <summary>نوشیدن آب.</summary>
    public const string InterventionDrinkWater = "CravingBattle.Action.DrinkWater";

    /// <summary>حرکت سبک.</summary>
    public const string InterventionMovement = "CravingBattle.Action.Movement";

    /// <summary>تماس با پشتیبانی.</summary>
    public const string InterventionSupportContact = "CravingBattle.Action.SupportContact";

    /// <summary>برچسب تایمر بر حسب ثانیه.</summary>
    public const string CravingBattleTimer = "CravingBattle.Timer";

    /// <summary>دکمهٔ تکمیل مداخله.</summary>
    public const string CravingBattleComplete = "CravingBattle.Complete";

    /// <summary>دکمهٔ مقاومت.</summary>
    public const string CravingBattleResistButton = "CravingBattle.ResistButton";

    /// <summary>پیام تکمیل موفق (حمایتی).</summary>
    public const string CravingBattleCompleted = "CravingBattle.Completed";

    /// <summary>پیام مقاومت موفق (حمایتی).</summary>
    public const string CravingBattleResisted = "CravingBattle.Resisted";

    /// <summary>پیام خروج/مصرف بدون شرم.</summary>
    public const string CravingBattleExited = "CravingBattle.Exited";

    /// <summary>دکمهٔ خروج امن.</summary>
    public const string CravingBattleExit = "CravingBattle.Exit";

    /// <summary>دکمهٔ بازگشت به شروع.</summary>
    public const string CravingBattleStartOver = "CravingBattle.StartOver";

    /// <summary>راهنمای هر مداخله.</summary>
    public const string CravingBattleInterventionGuide = "CravingBattle.InterventionGuide";

    // --- بهبود بدن ---

    /// <summary>عنوان بخش بهبود بدن.</summary>
    public const string RecoveryTitle = "Recovery.Title";

    /// <summary>برچسب پیشرفت تخمینی.</summary>
    public const string RecoveryProgress = "Recovery.Progress";

    /// <summary>قالب مرحلهٔ بعدی.</summary>
    public const string RecoveryNextStage = "Recovery.NextStage";

    /// <summary>سلب مسئولیت غیرپزشکی.</summary>
    public const string RecoveryDisclaimer = "Recovery.Disclaimer";

    /// <summary>پیام نبود دادهٔ کافی.</summary>
    public const string RecoveryUnavailable = "Recovery.Unavailable";

    /// <summary>عنوان مرحلهٔ صفر.</summary>
    public const string RecoveryStage0Title = "Recovery.Stage0.Title";

    /// <summary>عنوان مرحلهٔ ۱.</summary>
    public const string RecoveryStage1Title = "Recovery.Stage1.Title";

    /// <summary>عنوان مرحلهٔ ۲.</summary>
    public const string RecoveryStage2Title = "Recovery.Stage2.Title";

    /// <summary>عنوان مرحلهٔ ۳.</summary>
    public const string RecoveryStage3Title = "Recovery.Stage3.Title";

    /// <summary>عنوان مرحلهٔ ۴.</summary>
    public const string RecoveryStage4Title = "Recovery.Stage4.Title";

    /// <summary>عنوان مرحلهٔ ۵.</summary>
    public const string RecoveryStage5Title = "Recovery.Stage5.Title";

    /// <summary>عنوان مرحلهٔ ۶.</summary>
    public const string RecoveryStage6Title = "Recovery.Stage6.Title";

    /// <summary>عنوان مرحلهٔ ۷.</summary>
    public const string RecoveryStage7Title = "Recovery.Stage7.Title";

    /// <summary>توضیح مرحلهٔ صفر.</summary>
    public const string RecoveryStage0Description = "Recovery.Stage0.Description";

    /// <summary>توضیح مرحلهٔ ۱.</summary>
    public const string RecoveryStage1Description = "Recovery.Stage1.Description";

    /// <summary>توضیح مرحلهٔ ۲.</summary>
    public const string RecoveryStage2Description = "Recovery.Stage2.Description";

    /// <summary>توضیح مرحلهٔ ۳.</summary>
    public const string RecoveryStage3Description = "Recovery.Stage3.Description";

    /// <summary>توضیح مرحلهٔ ۴.</summary>
    public const string RecoveryStage4Description = "Recovery.Stage4.Description";

    /// <summary>توضیح مرحلهٔ ۵.</summary>
    public const string RecoveryStage5Description = "Recovery.Stage5.Description";

    /// <summary>توضیح مرحلهٔ ۶.</summary>
    public const string RecoveryStage6Description = "Recovery.Stage6.Description";

    /// <summary>توضیح مرحلهٔ ۷.</summary>
    public const string RecoveryStage7Description = "Recovery.Stage7.Description";

    // --- تنظیمات ---

    /// <summary>عنوان صفحهٔ تنظیمات.</summary>
    public const string SettingsTitle = "Settings.Title";

    /// <summary>دکمهٔ ورود به تنظیمات.</summary>
    public const string SettingsEntry = "Settings.Entry";

    /// <summary>برچسب مصرف روزانه.</summary>
    public const string SettingsCigarettesPerDay = "Settings.CigarettesPerDay";

    /// <summary>برچسب قیمت هر نخ.</summary>
    public const string SettingsPricePerCigarette = "Settings.PricePerCigarette";

    /// <summary>برچسب قیمت هر بسته.</summary>
    public const string SettingsPricePerPack = "Settings.PricePerPack";

    /// <summary>برچسب اندازهٔ بسته.</summary>
    public const string SettingsPackSize = "Settings.PackSize";

    /// <summary>برچسب ارز.</summary>
    public const string SettingsCurrency = "Settings.Currency";

    /// <summary>برچسب تاریخ ترک.</summary>
    public const string SettingsQuitDate = "Settings.QuitDate";

    /// <summary>دکمهٔ ذخیره.</summary>
    public const string SettingsSave = "Settings.Save";

    /// <summary>پیام ذخیرهٔ موفق.</summary>
    public const string SettingsSaved = "Settings.Saved";

    /// <summary>راهنمای صرفه‌جویی تقریبی.</summary>
    public const string SettingsSavingsHint = "Settings.SavingsHint";

    /// <summary>خطای مصرف روزانه نامعتبر.</summary>
    public const string SettingsErrorCigarettesPerDay = "Settings.Error.InvalidCigarettesPerDay";

    /// <summary>خطای قیمت نامعتبر.</summary>
    public const string SettingsErrorPrice = "Settings.Error.InvalidPrice";

    /// <summary>خطای اندازهٔ بستهٔ نامعتبر.</summary>
    public const string SettingsErrorPackSize = "Settings.Error.InvalidPackSize";

    /// <summary>خطای نبود قیمت معتبر.</summary>
    public const string SettingsErrorMissingPrice = "Settings.Error.MissingPrice";

    /// <summary>خطای تاریخ ترک نامعتبر.</summary>
    public const string SettingsErrorQuitDate = "Settings.Error.InvalidQuitDate";

    /// <summary>خطای ارز نامعتبر.</summary>
    public const string SettingsErrorCurrency = "Settings.Error.InvalidCurrency";

    /// <summary>عنوان صفحهٔ پروفایل.</summary>
    public const string ProfileTitle = "Profile.Title";

    /// <summary>ورود به پروفایل.</summary>
    public const string ProfileEntry = "Profile.Entry";

    /// <summary>نام نمایشی.</summary>
    public const string ProfileDisplayName = "Profile.DisplayName";

    /// <summary>آواتار.</summary>
    public const string ProfileAvatar = "Profile.Avatar";

    /// <summary>ذخیرهٔ پروفایل.</summary>
    public const string ProfileSave = "Profile.Save";

    /// <summary>ذخیرهٔ موفق پروفایل.</summary>
    public const string ProfileSaved = "Profile.Saved";

    /// <summary>زبان.</summary>
    public const string ProfileLanguage = "Profile.Language";

    /// <summary>زبان دارای fallback.</summary>
    public const string ProfileLanguageFallback = "Profile.LanguageFallback";

    /// <summary>ورود به تنظیمات اعلان.</summary>
    public const string ProfileNotifications = "Profile.Notifications";

    /// <summary>ورود به حریم خصوصی و داده.</summary>
    public const string ProfilePrivacyData = "Profile.PrivacyData";

    /// <summary>راهنمای آواتار.</summary>
    public const string ProfileAvatarHint = "Profile.AvatarHint";

    /// <summary>خطای نام نمایشی.</summary>
    public const string ProfileErrorDisplayName = "Profile.Error.DisplayName";

    /// <summary>خطای آواتار.</summary>
    public const string ProfileErrorAvatar = "Profile.Error.Avatar";

    /// <summary>خطای locale.</summary>
    public const string ProfileErrorLocale = "Profile.Error.Locale";

    /// <summary>نام زبان انگلیسی.</summary>
    public const string LanguageEnglish = "Language.English";

    /// <summary>نام زبان فارسی.</summary>
    public const string LanguagePersian = "Language.Persian";

    /// <summary>نام زبان عربی.</summary>
    public const string LanguageArabic = "Language.Arabic";

    /// <summary>نام زبان آلمانی.</summary>
    public const string LanguageGerman = "Language.German";

    /// <summary>نام زبان اسپانیایی.</summary>
    public const string LanguageSpanish = "Language.Spanish";

    /// <summary>نام زبان فرانسوی.</summary>
    public const string LanguageFrench = "Language.French";

    /// <summary>نام زبان هندی.</summary>
    public const string LanguageHindi = "Language.Hindi";

    /// <summary>نام زبان اندونزیایی.</summary>
    public const string LanguageIndonesian = "Language.Indonesian";

    /// <summary>نام زبان ژاپنی.</summary>
    public const string LanguageJapanese = "Language.Japanese";

    /// <summary>نام زبان کره‌ای.</summary>
    public const string LanguageKorean = "Language.Korean";

    /// <summary>نام زبان پرتغالی برزیل.</summary>
    public const string LanguagePortugueseBrazil = "Language.PortugueseBrazil";

    /// <summary>نام زبان روسی.</summary>
    public const string LanguageRussian = "Language.Russian";

    /// <summary>نام زبان ترکی.</summary>
    public const string LanguageTurkish = "Language.Turkish";

    /// <summary>نام زبان اوکراینی.</summary>
    public const string LanguageUkrainian = "Language.Ukrainian";

    /// <summary>نام چینی ساده.</summary>
    public const string LanguageChineseSimplified = "Language.ChineseSimplified";

    /// <summary>نام چینی سنتی.</summary>
    public const string LanguageChineseTraditional = "Language.ChineseTraditional";

    /// <summary>نام آواتار پیش‌فرض.</summary>
    public const string AvatarDefault = "Avatar.Default";

    /// <summary>نام آواتار برگ.</summary>
    public const string AvatarLeaf = "Avatar.Leaf";

    /// <summary>نام آواتار خورشید.</summary>
    public const string AvatarSun = "Avatar.Sun";

    /// <summary>نام آواتار موج.</summary>
    public const string AvatarWave = "Avatar.Wave";

    /// <summary>عنوان صفحهٔ حریم خصوصی و داده.</summary>
    public const string PrivacyDataTitle = "PrivacyData.Title";

    /// <summary>توضیح حریم خصوصی.</summary>
    public const string PrivacyDataDescription = "PrivacyData.Description";

    /// <summary>داده‌ها فقط محلی‌اند.</summary>
    public const string PrivacyDataLocalOnly = "PrivacyData.LocalOnly";

    /// <summary>وضعیت آماده بودن کنترل‌ها.</summary>
    public const string PrivacyDataControls = "PrivacyData.Controls";

    // --- اعلان‌ها ---

    /// <summary>عنوان صفحهٔ اعلان‌ها.</summary>
    public const string NotificationsTitle = "Notifications.Title";

    /// <summary>دکمهٔ ورود به اعلان‌ها.</summary>
    public const string NotificationsEntry = "Notifications.Entry";

    /// <summary>برچسب فعال‌سازی اعلان‌ها.</summary>
    public const string NotificationsEnable = "Notifications.Enable";

    /// <summary>برچسب دستهٔ تشویق روزانه.</summary>
    public const string NotificationsDailyEncouragement = "Notifications.DailyEncouragement";

    /// <summary>برچسب دستهٔ میل‌استون.</summary>
    public const string NotificationsMilestoneReached = "Notifications.MilestoneReached";

    /// <summary>برچسب دستهٔ پشتیبانی هوس.</summary>
    public const string NotificationsCravingSupport = "Notifications.CravingSupport";

    /// <summary>برچسب دستهٔ پیشرفت/صرفه‌جویی.</summary>
    public const string NotificationsSavingsProgress = "Notifications.SavingsProgress";

    /// <summary>برچسب زمان روز.</summary>
    public const string NotificationsTimeOfDay = "Notifications.TimeOfDay";

    /// <summary>دکمهٔ ذخیره.</summary>
    public const string NotificationsSave = "Notifications.Save";

    /// <summary>پیام ذخیرهٔ موفق.</summary>
    public const string NotificationsSaved = "Notifications.Saved";

    /// <summary>پیام رد مجوز.</summary>
    public const string NotificationsPermissionDenied = "Notifications.PermissionDenied";

    /// <summary>راهنمای عدم نمایش دادهٔ حساس.</summary>
    public const string NotificationsSensitiveHint = "Notifications.SensitiveHint";

    /// <summary>عنوان اعلان تشویق روزانه.</summary>
    public const string NotificationDailyTitle = "Notification.Daily.Title";

    /// <summary>بدنهٔ اعلان تشویق روزانه.</summary>
    public const string NotificationDailyBody = "Notification.Daily.Body";

    /// <summary>عنوان اعلان میل‌استون.</summary>
    public const string NotificationMilestoneTitle = "Notification.Milestone.Title";

    /// <summary>بدنهٔ اعلان میل‌استون.</summary>
    public const string NotificationMilestoneBody = "Notification.Milestone.Body";

    /// <summary>عنوان اعلان پشتیبانی هوس.</summary>
    public const string NotificationCravingTitle = "Notification.Craving.Title";

    /// <summary>بدنهٔ اعلان پشتیبانی هوس.</summary>
    public const string NotificationCravingBody = "Notification.Craving.Body";

    /// <summary>عنوان اعلان پیشرفت/صرفه‌جویی.</summary>
    public const string NotificationSavingsTitle = "Notification.Savings.Title";

    /// <summary>بدنهٔ اعلان پیشرفت/صرفه‌جویی.</summary>
    public const string NotificationSavingsBody = "Notification.Savings.Body";

    // --- تحلیل محرک ---

    /// <summary>عنوان بخش تحلیل محرک.</summary>
    public const string TriggerAnalysisTitle = "TriggerAnalysis.Title";

    /// <summary>برچسب فعال‌سازی تحلیل.</summary>
    public const string TriggerAnalysisEnable = "TriggerAnalysis.Enable";

    /// <summary>برچسب غیرفعال بودن تحلیل.</summary>
    public const string TriggerAnalysisDisabled = "TriggerAnalysis.Disabled";

    /// <summary>برچسب غیرفعال‌سازی تحلیل.</summary>
    public const string TriggerAnalysisDisable = "TriggerAnalysis.Disable";

    /// <summary>حداقل دادهٔ لازم.</summary>
    public const string TriggerAnalysisMinimumData = "TriggerAnalysis.MinimumData";

    /// <summary>پیام نبود دادهٔ کافی.</summary>
    public const string TriggerAnalysisInsufficientData = "TriggerAnalysis.InsufficientData";

    /// <summary>حالت بدون بینش.</summary>
    public const string TriggerAnalysisEmpty = "TriggerAnalysis.Empty";

    /// <summary>حالت خطای بارگذاری.</summary>
    public const string TriggerAnalysisError = "TriggerAnalysis.Error";

    /// <summary>راهنمای عدم نمایش دادهٔ حساس.</summary>
    public const string TriggerAnalysisPrivacyNote = "TriggerAnalysis.PrivacyNote";

    /// <summary>سلب مسئولیت تقریبی/غیرپزشکی.</summary>
    public const string TriggerAnalysisDisclaimer = "TriggerAnalysis.Disclaimer";

    /// <summary>برچسب «حدوداً» برای بینش‌ها.</summary>
    public const string TriggerAnalysisApproximate = "TriggerAnalysis.Approximate";

    /// <summary>قالب قدرت تقریبی بینش.</summary>
    public const string TriggerAnalysisStrength = "TriggerAnalysis.Strength";

    /// <summary>بینش زمان روز.</summary>
    public const string TriggerInsightTimeOfDay = "Trigger.TimeOfDay";

    /// <summary>بینش روز هفته.</summary>
    public const string TriggerInsightDayOfWeek = "Trigger.DayOfWeek";

    /// <summary>بینش زمینه.</summary>
    public const string TriggerInsightContext = "Trigger.Context";

    /// <summary>متن زمینهٔ تجمیعی بدون نمایش مقدار خام.</summary>
    public const string TriggerInsightContextAggregated = "Trigger.Context.Aggregated";

    /// <summary>بینش فراوانی هوس.</summary>
    public const string TriggerInsightCravingFrequency = "Trigger.CravingFrequency";

    /// <summary>بینش مصرف در برابر مقاومت.</summary>
    public const string TriggerInsightSmokedVsResisted = "Trigger.SmokedVsResisted";

    /// <summary>صبح زود.</summary>
    public const string TimeBucketEarlyMorning = "Trigger.TimeBucket.EarlyMorning";

    /// <summary>صبح.</summary>
    public const string TimeBucketMorning = "Trigger.TimeBucket.Morning";

    /// <summary>بعدازظهر.</summary>
    public const string TimeBucketAfternoon = "Trigger.TimeBucket.Afternoon";

    /// <summary>عصر.</summary>
    public const string TimeBucketEvening = "Trigger.TimeBucket.Evening";

    /// <summary>شب.</summary>
    public const string TimeBucketNight = "Trigger.TimeBucket.Night";

    // --- مربی محلی ---

    public const string CoachTitle = "Coach.Title";
    public const string CoachPrivacyNote = "Coach.PrivacyNote";
    public const string CoachDisabled = "Coach.Disabled";
    public const string CoachEnabled = "Coach.Enabled";
    public const string CoachAllowExternal = "Coach.AllowExternal";
    public const string CoachAllowProgress = "Coach.AllowProgress";
    public const string CoachAllowCraving = "Coach.AllowCraving";
    public const string CoachClear = "Coach.Clear";
    public const string CoachCleared = "Coach.Cleared";
    public const string CoachEmpty = "Coach.Empty";
    public const string CoachLoading = "Coach.Loading";
    public const string CoachError = "Coach.Error";
    public const string CoachSuggestionCravingSupport = "Coach.Suggestion.CravingSupport";
    public const string CoachSuggestionProgress = "Coach.Suggestion.Progress";
    public const string CoachSuggestionMilestone = "Coach.Suggestion.Milestone";
    public const string CoachExternalTitle = "Coach.External.Title";
    public const string CoachExternalNote = "Coach.External.Note";
    public const string CoachExternalConsent = "Coach.External.Consent";
    public const string CoachExternalUnavailable = "Coach.External.Unavailable";
    public const string CoachExternalAvailable = "Coach.External.Available";
    public const string CoachExternalRevoke = "Coach.External.Revoke";
    public const string CoachExternalRevoked = "Coach.External.Revoked";
}
