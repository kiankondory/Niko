// مسئولیت: صف حداقلی و durable برای پیام‌هایی که هنگام قطع اتصال به Phone تحویل نشده‌اند.
// وابستگی‌ها و لایه: Wear adapter → PendingActionStore؛ SQLite یا event storage را دور نمی‌زند.
// نکات تغییر و قیود: فقط پیام قرارداد ذخیره می‌شود، duplicate بر پایهٔ MessageId/EventId رد می‌شود.
package com.companyname.niko.wear

interface PendingActionStore {
    fun load(): List<String>
    fun save(messages: List<String>)
}

class WearPendingActionQueue(private val store: PendingActionStore) {
    fun enqueue(serializedMessage: String): Boolean {
        val message = WearCompanionMessage.parse(serializedMessage) ?: return false
        val existing = store.load().mapNotNull(WearCompanionMessage::parse)
        if (existing.any { it.messageId == message.messageId || it.eventId == message.eventId }) {
            return false
        }

        store.save(store.load() + serializedMessage)
        return true
    }

    fun pending(): List<String> = store.load()

    fun retry(transport: WearPhoneTransport): Int {
        val remaining = mutableListOf<String>()
        var delivered = 0
        for (serialized in store.load()) {
            when (transport.send(serialized)) {
                is WearTransportResult.Success -> delivered++
                is WearTransportResult.Failure -> remaining += serialized
            }
        }
        store.save(remaining)
        return delivered
    }
}
