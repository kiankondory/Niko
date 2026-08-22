// مسئولیت: نسخهٔ ابزارهای build پروژهٔ Wear OS.
// وابستگی‌ها و لایه: Gradle root؛ هیچ منطق محصول یا secret در این فایل نیست.
// نکات تغییر و قیود: خروجی فقط native companion است و provider/network واقعی اضافه نمی‌کند.
plugins {
    id("com.android.application") version "8.6.1" apply false
    id("org.jetbrains.kotlin.android") version "2.0.21" apply false
}
