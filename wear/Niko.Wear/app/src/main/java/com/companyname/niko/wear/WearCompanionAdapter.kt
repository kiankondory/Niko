// مسئولیت: آداپتر QuickLog و نگاشت خلاصهٔ aggregate برای صفحهٔ Wear.
// وابستگی‌ها و لایه: Wear UI → Companion contract/transport؛ منطق دامنه در Phone/Core می‌ماند.
// نکات تغییر و قیود: همهٔ شکست‌ها به offline/pending/unavailable تبدیل می‌شوند و raw data پذیرفته نمی‌شود.
package com.companyname.niko.wear

import org.json.JSONException
import org.json.JSONObject
import java.time.Instant
import java.util.UUID

data class WearSummary(
    val smokedToday: Int,
    val resistedToday: Int,
    val currentStreakDays: Int,
    val milestoneProgressPercent: Double,
    val pendingCount: Int,
    val inSync: Boolean
)

sealed class WearDeliveryState {
    data object Delivered : WearDeliveryState()
    data object PendingOffline : WearDeliveryState()
    data object Duplicate : WearDeliveryState()
    data object Unavailable : WearDeliveryState()
}

class WearCompanionAdapter(
    private val transport: WearPhoneTransport,
    private val queue: WearPendingActionQueue,
    private val nowUtc: () -> String = { Instant.now().toString() },
    private val idFactory: () -> String = { UUID.randomUUID().toString().replace("-", "") }
) {
    fun quickLog(eventType: WearEventType): WearDeliveryState {
        val eventId = idFactory()
        val message = WearCompanionMessage(
            messageId = idFactory(),
            eventId = eventId,
            eventType = eventType,
            occurredAtUtc = nowUtc(),
            sentAtUtc = nowUtc()
        ).serialize()

        return when (transport.send(message)) {
            is WearTransportResult.Success -> WearDeliveryState.Delivered
            is WearTransportResult.Failure -> {
                if (queue.enqueue(message)) WearDeliveryState.PendingOffline
                else WearDeliveryState.Duplicate
            }
        }
    }

    fun retryPending(): Int = queue.retry(transport)

    fun loadSummary(): WearTransportResult<WearSummary> {
        return when (val result = transport.requestSummary()) {
            is WearTransportResult.Failure -> result
            is WearTransportResult.Success -> mapSummary(result.value)
        }
    }

    private fun mapSummary(serialized: String): WearTransportResult<WearSummary> {
        return try {
            val json = JSONObject(serialized)
            val summary = WearSummary(
                smokedToday = json.getInt("smokedToday"),
                resistedToday = json.getInt("resistedToday"),
                currentStreakDays = json.getInt("currentStreakDays"),
                milestoneProgressPercent = json.getDouble("milestoneProgressPercent"),
                pendingCount = json.getInt("pendingCount"),
                inSync = json.getBoolean("inSync")
            )
            if (summary.smokedToday < 0 || summary.resistedToday < 0 ||
                summary.currentStreakDays < 0 || summary.pendingCount < 0 ||
                summary.milestoneProgressPercent !in 0.0..100.0
            ) {
                WearTransportResult.Failure(FailureReason.INVALID_RESPONSE)
            } else {
                WearTransportResult.Success(summary)
            }
        } catch (_: JSONException) {
            WearTransportResult.Failure(FailureReason.INVALID_RESPONSE)
        }
    }
}
