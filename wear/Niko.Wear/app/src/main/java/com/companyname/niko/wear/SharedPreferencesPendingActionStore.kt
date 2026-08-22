// مسئولیت: ذخیرهٔ durable پیام‌های pending در فضای خصوصی companion.
// وابستگی‌ها و لایه: Android adapter → SharedPreferences؛ دادهٔ event یا note ذخیره نمی‌شود.
// نکات تغییر و قیود: این صف فقط برای تحویل transport است و منبع حقیقت Phone/Core نیست.
package com.companyname.niko.wear

import android.content.Context

class SharedPreferencesPendingActionStore(context: Context) : PendingActionStore {
    private val preferences = context.getSharedPreferences("companion_delivery", Context.MODE_PRIVATE)

    override fun load(): List<String> = preferences.getStringSet(KEY_MESSAGES, emptySet())
        ?.toList()
        ?.sorted()
        ?: emptyList()

    override fun save(messages: List<String>) {
        preferences.edit().putStringSet(KEY_MESSAGES, messages.toSet()).apply()
    }

    private companion object {
        const val KEY_MESSAGES = "pending_messages"
    }
}
