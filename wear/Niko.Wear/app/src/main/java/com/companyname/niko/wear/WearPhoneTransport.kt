// مسئولیت: قرارداد انتقال پیام و خلاصه بین Wear OS و برنامهٔ Phone.
// وابستگی‌ها و لایه: Wear adapter؛ implementation واقعی بعداً می‌تواند Data Layer باشد.
// نکات تغییر و قیود: نبودن اتصال خطا است، نه مجوز ساختن network path؛ هیچ secret یا backend در Wear نیست.
package com.companyname.niko.wear

sealed class WearTransportResult<out T> {
    data class Success<T>(val value: T) : WearTransportResult<T>()
    data class Failure(val reason: FailureReason) : WearTransportResult<Nothing>()
}

enum class FailureReason {
    OFFLINE,
    UNAVAILABLE,
    INVALID_RESPONSE
}

interface WearPhoneTransport {
    fun send(serializedMessage: String): WearTransportResult<Unit>
    fun requestSummary(): WearTransportResult<String>
}

class UnavailableWearPhoneTransport : WearPhoneTransport {
    override fun send(serializedMessage: String): WearTransportResult<Unit> =
        WearTransportResult.Failure(FailureReason.UNAVAILABLE)

    override fun requestSummary(): WearTransportResult<String> =
        WearTransportResult.Failure(FailureReason.UNAVAILABLE)
}
