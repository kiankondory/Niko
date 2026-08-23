# چک‌لیست دستی انتشار Niko

> مسئولیت: ثبت آزمون‌های نهایی قابل‌اجرا در Visual Studio روی Samsung Galaxy A50.
> وابستگی‌ها و لایه: مستندات QA؛ فقط به build Debug دستگاه و دادهٔ محلی موجود وابسته است.
> نکات تغییر و قیود: آزمون‌ها نباید uninstall، clear data یا حذف SQLite انجام دهند؛ هر شکست باید با screenshot و متن خطا ثبت شود.

## آماده‌سازی

- [ ] از Visual Studio، Android Debug را برای Samsung Galaxy A50 انتخاب کنید.
- [ ] نصب/به‌روزرسانی را فقط با مسیر Fast Deployment موجود انجام دهید.
- [ ] تأیید کنید profile، locale، settings و eventهای قبلی قبل از آزمون باقی هستند.

## اولین اجرا و زبان

- [ ] مسیر: نصب تازه یا دادهٔ پاک‌شدهٔ آزمایشی → صفحهٔ Welcome.
- [ ] زبان را انتخاب کنید و مراحل Intro را تا Start Niko ادامه دهید.
- [ ] مسیر: Settings → Language؛ زبان‌های `en`، `fa`، `ar` و `zh-Hans` را تغییر دهید.
- [ ] انتظار: متن‌ها، جهت RTL/LTR و tabها پس از refresh/app restart درست تغییر کنند.
- [ ] برای زبان‌های fallback، متن انگلیسیِ مستندشده نمایش داده شود و raw key دیده نشود.

## Profile و Settings

- [ ] مسیر: Settings → Profile؛ نام، avatar، تاریخ ترک، مصرف روزانه، قیمت و currency را ذخیره کنید.
- [ ] برنامه را force-stop و دوباره باز کنید؛ همهٔ مقادیر باید باقی بمانند.
- [ ] مسیر: Settings → Appearance؛ Light، Dark و Reduce motion را تغییر دهید.
- [ ] انتظار: تغییر ظاهر نباید domain data یا eventها را تغییر دهد.

## Privacy و داده

- [ ] مسیر: Settings → Privacy and data → Export data.
- [ ] انتظار: export محلی ساخته شود و هیچ network call یا حذف داده رخ ندهد.
- [ ] Clear Data را باز کنید، لغو PIN و سپس تأیید PIN دستگاه را جداگانه امتحان کنید.
- [ ] انتظار: لغو PIN هیچ داده‌ای را حذف نکند؛ تأیید PIN فقط profile/settings/events محلی را پاک کند و onboarding تازه باز شود.
- [ ] screenshot نتیجهٔ export، پیام لغو و صفحهٔ onboarding پس از پاک‌سازی ثبت شود.

## Dashboard، Island و Battle

- [ ] مسیر: Home/Dashboard؛ countها، progress، savings و body recovery را بررسی کنید.
- [ ] مسیر: Island؛ برای روزهای مختلف Smoked و Resisted ثبت کنید.
- [ ] انتظار: تعداد مصرف روزانه، پس‌انداز روزانه و مجموع تجمعی از تاریخ ترک با SQLite/Core سازگار باشد.
- [ ] مسیر: Battle؛ یک craving را شروع، کامل و خارج کنید؛ صفحهٔ سفید یا route error نباید رخ دهد.
- [ ] انتظار: Dashboard و Island بعد از بازگشت از Battle به‌روز شوند.

## Widget

- [ ] از launcher: Widgets → Niko را اضافه کنید.
- [ ] مسیرهای Smoked، Resisted و Craving را هرکدام یک‌بار اجرا کنید.
- [ ] انتظار: feedback کلیک/حالت pending دیده شود، رویداد فقط یک‌بار ذخیره شود و count روزانه دقیقاً یک واحد تغییر کند.
- [ ] QuickLog را داخل برنامه اجرا کنید و widget را refresh کنید؛ countها باید یکسان باشند.
- [ ] network را خاموش کنید؛ دادهٔ محلی و وضعیت offline باید بدون raw event نمایش داده شود.
- [ ] زبان را تغییر دهید و widget را refresh یا برنامه را restart کنید؛ label و جهت باید به‌روز شوند.

## لاگ و گزارش شکست

- [ ] لاگ فقط برای startup و خطای قابل‌مشاهده بررسی شود؛ secret، note، location یا raw event نباید در گزارش باشد.
- [ ] در صورت شکست، screenshot، مسیر دقیق، زبان فعال، online/offline، action آخر و متن کامل خطا ثبت شود.
- [ ] هیچ موردی با uninstall، clear data یا حذف SQLite برای دور زدن شکست تکرار نشود.

