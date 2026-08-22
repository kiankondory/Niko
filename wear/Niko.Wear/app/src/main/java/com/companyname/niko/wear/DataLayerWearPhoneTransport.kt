// مسئولیت: ارسال پیام‌های Companion از Wear OS به Phone با Google Data Layer.
// وابستگی‌ها و لایه: Wear adapter → Google Play Services Wearable؛ Core و
// storage رویداد روی Phone باقی می‌مانند و هیچ backend یا دادهٔ خصوصی اضافه نمی‌شود.
// نکات تغییر و قیود: پیام همان JSON نسخه‌دار با MessageId/EventId است؛ نبودن
// Node یا خطای transport به Offline/Unavailable تبدیل می‌شود و queue موجود آن را retry می‌کند.
package com.companyname.niko.wear

import android.content.Context
import com.google.android.gms.tasks.Tasks
import com.google.android.gms.wearable.Wearable
import java.nio.charset.StandardCharsets
import java.util.concurrent.TimeUnit

class DataLayerWearPhoneTransport(private val context: Context) : WearPhoneTransport {
    override fun send(serializedMessage: String): WearTransportResult<Unit> {
        return try {
            val node = Tasks.await(
                Wearable.getNodeClient(context).connectedNodes,
                TIMEOUT_SECONDS,
                TimeUnit.SECONDS
            ).firstOrNull() ?: return WearTransportResult.Failure(FailureReason.OFFLINE)

            Tasks.await(
                Wearable.getMessageClient(context).sendMessage(
                    node.id,
                    PATH_QUICK_LOG,
                    serializedMessage.toByteArray(StandardCharsets.UTF_8)
                ),
                TIMEOUT_SECONDS,
                TimeUnit.SECONDS
            )
            WearTransportResult.Success(Unit)
        } catch (_: Exception) {
            WearTransportResult.Failure(FailureReason.UNAVAILABLE)
        }
    }

    override fun requestSummary(): WearTransportResult<String> =
        WearTransportResult.Failure(FailureReason.UNAVAILABLE)

    private companion object {
        const val PATH_QUICK_LOG = "/niko/companion/quicklog"
        const val TIMEOUT_SECONDS = 2L
    }
}
