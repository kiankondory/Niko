# Niko — فهرست اجرایی تا انتشار

> مسئولیت: فهرست شفاف کارهای باقی‌مانده و معیار پذیرش آن‌ها تا نسخهٔ نهایی.
> وابستگی‌ها و لایه: سند محصول؛ تصمیم‌های معماری در `DECISIONS.md` و منطق در Core باقی می‌ماند.
> نکات و قیود: اولویت با آفلاین، حریم خصوصی، متن محلی‌سازی‌شده و عدم ایجاد دادهٔ ساختگی است.
> چک‌لیست دستی انتشار: `docs/MANUAL_RELEASE_CHECKLIST.md`.

## قاعدهٔ انجام و شواهد پذیرش

- [x] هر آیتم کدنویسی‌شده باید با آزمون و build هدف مرتبط همراه باشد.
- [ ] هر آیتم وابسته به دستگاه فقط با گزارش صریح تست دستی Galaxy A50 بسته می‌شود.
- [ ] deployment تنها پس از build معتبر Visual Studio انجام می‌شود؛ Codex هیچ app data یا SQLite دستگاه را حذف نمی‌کند.

## در حال اجرا — مسیر پویای Recovery

- [x] Island چهارمرحله‌ای با دادهٔ واقعی Recovery و asset محلی؛ انیمیشن انتقال مرحله در QA دستی بررسی می‌شود.
- [x] کارت Body Recovery بصری و غیرپزشکی؛ تغییر ظاهر فقط بر اساس Recovery stage.
- [x] تست‌های Core برای همهٔ آستانه‌ها و حالتِ بدون داده.
- [x] گزارش Island برای تعداد مصرف، مقاومت و پس‌انداز تجمعی با timezone محلی در Core؛ نمایش MAUI به جدول فشردهٔ ۱۵ روز اخیر تبدیل شده است.
- [ ] آزمون دستی نگاشت تصویر Island/حالت خالی روی Galaxy A50.

**معیار پذیرش:** کاربر با عبور از مرحله‌های واقعی ترک، Island متفاوت می‌بیند؛ هیچ XP، رویداد خام یا ادعای پزشکی نمایش داده نمی‌شود.

## مسیر نخستین اجرا و داده

- [x] انتخاب زبان و معرفی مرحله‌ای اولین اجرا پیاده‌سازی شده است.
- [x] اگر Android preference بازیابی شود اما profile محلی نباشد، intro دوباره نمایش داده می‌شود.
- [ ] آزمون دستی نصب/بازگردانی/پاک‌سازی داده و نمایش intro روی Galaxy A50.
- [x] export JSON محلی و حذف تراکنشی SQLite با تأیید قفل دستگاه پیاده‌سازی و آزمون شده است.
- [ ] آزمون دستی export، لغو PIN، تأیید PIN، restart و عدم حذف ناخواستهٔ داده روی Galaxy A50.

## تجربه و طراحی

- [x] بازطراحی Quick Log با آیکن‌های یکپارچه، شمارندهٔ امروز و بازخورد ثبت روشن.
- [x] validator خودکار برای جلوگیری از متن قابل‌نمایش hard-code شده در XAML.
- [ ] تکمیل Battle: طراحی مراحل و بازخورد انجام شد؛ آزمون دستیِ صفحهٔ سفید و بازگشت باقی است.
- [ ] یکپارچه‌سازی motion سبک، Reduced Motion، حالت‌های Light/Dark و دسترس‌پذیری.
- [ ] بررسی دستی Dashboard، Island، Profile/Settings، Widget و RTL/LTR روی Galaxy A50.

## زبان و بومی‌سازی

- [x] هر ۱۶ locale پیکربندی‌شده فایل resource دارد و validator کلید/placeholder آن‌ها را می‌سنجد.
- [x] en، fa، ar و zh-Hans با پوشش کامل کلیدها و placeholderهای سازگار هستند.
- [x] کلیدهای گزارش روزانهٔ Island برای de، es، fr، hi و id ترجمه شده‌اند.
- [ ] تکمیل و بازبینی native برای de, es, fr, hi, id, ja, ko, pt-BR, ru, tr, uk, zh-Hant؛ تا آن زمان fallback انگلیسی صریح است.
- [ ] آزمون دستی layout طولانی و RTL/LTR برای هر locale روی Galaxy A50.
- [ ] فقط پس از پوشش کامل، برچسب «پشتیبانی کامل» برای ۱۶ زبان.

## پایداری و انتشار

- [x] مسیر blocking خواندن locale از startup حذف شد.
- [ ] اندازه‌گیری cold-start روی دستگاه باقی است.
- [x] تست Core و SQLite برای export/delete داده و privacy controls واقعی.
- [ ] آزمون دستی Android device credential، export و پاک‌سازی داده روی Galaxy A50.
- [ ] تصمیم محصول برای cloud sync/backup؛ تا آن زمان local-first و NoopSync شفاف بماند.
- [x] آزمون Core/Infrastructure و Android compile-only در CI محلی اجرا شده است.
- [ ] Android packaging/Visual Studio deployment نیازمند اجرای Visual Studio است؛ Windows build عادی و Android compile-only اکنون با ۰ خطا و ۰ هشدار موفق‌اند.
- [x] static secret scan تکرارپذیر با allow-list مثال مستندات Gemini.
- [ ] امضای release، نسخه‌بندی، Store assets، Privacy Policy و چک‌لیست انتشار.

## اختیاری و پس از انتشار

- [ ] External Coach فقط با Proxy امن و تأیید عملی free-tier؛ Local Coach همیشه آفلاین و پیش‌فرض باقی می‌ماند.
- [ ] Wear OS: آزمون واقعی Phone ↔ Wear روی ساعت؛ تا آن زمان بخش Wear فریز است.
