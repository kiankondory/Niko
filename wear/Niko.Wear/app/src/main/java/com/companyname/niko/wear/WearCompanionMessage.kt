// مسئولیت: ساخت و اعتبارسنجی پیام QuickLog سازگار با قرارداد Companion نسخهٔ ۱.
// وابستگی‌ها و لایه: Wear adapter → JSON contract؛ هیچ منطق دامنه، event history یا metadata خصوصی ندارد.
// نکات تغییر و قیود: MessageId و EventId برای retry پایدارند و فقط سه EventType مجاز است.
package com.companyname.niko.wear

import org.json.JSONException
import org.json.JSONObject

enum class WearEventType(val code: Int) {
    SMOKED(1),
    RESISTED(2),
    CRAVING(3);

    companion object {
        fun fromCode(code: Int): WearEventType? = entries.firstOrNull { it.code == code }
    }
}

data class WearCompanionMessage(
    val messageId: String,
    val eventId: String,
    val eventType: WearEventType,
    val occurredAtUtc: String,
    val sentAtUtc: String,
    val contractVersion: Int = CURRENT_CONTRACT_VERSION
) {
    fun serialize(): String {
        val payload = JSONObject()
            .put("eventType", eventType.code)
            .put("eventId", eventId)
            .put("occurredAtUtc", occurredAtUtc)

        return JSONObject()
            .put("contractVersion", contractVersion)
            .put("messageId", messageId)
            .put("source", WEARABLE_SOURCE)
            .put("messageType", QUICK_LOG_MESSAGE_TYPE)
            .put("payload", payload.toString())
            .put("sentAtUtc", sentAtUtc)
            .toString()
    }

    companion object {
        const val CURRENT_CONTRACT_VERSION = 1
        const val WEARABLE_SOURCE = 2
        const val QUICK_LOG_MESSAGE_TYPE = 0

        fun parse(serialized: String): WearCompanionMessage? {
            return try {
                val envelope = JSONObject(serialized)
                if (envelope.optInt("contractVersion", -1) != CURRENT_CONTRACT_VERSION ||
                    envelope.optInt("source", -1) != WEARABLE_SOURCE ||
                    envelope.optInt("messageType", -1) != QUICK_LOG_MESSAGE_TYPE
                ) {
                    return null
                }

                val messageId = envelope.optString("messageId")
                val sentAtUtc = envelope.optString("sentAtUtc")
                val payload = JSONObject(envelope.optString("payload"))
                val eventId = payload.optString("eventId")
                val occurredAtUtc = payload.optString("occurredAtUtc")
                val eventType = WearEventType.fromCode(payload.optInt("eventType", -1))
                if (messageId.isBlank() || eventId.isBlank() || occurredAtUtc.isBlank() ||
                    sentAtUtc.isBlank() || eventType == null
                ) {
                    return null
                }

                WearCompanionMessage(messageId, eventId, eventType, occurredAtUtc, sentAtUtc)
            } catch (_: JSONException) {
                null
            }
        }
    }
}
