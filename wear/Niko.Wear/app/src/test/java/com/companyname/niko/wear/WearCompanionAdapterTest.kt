// مسئولیت: آزمون قطعی قرارداد، صف تحویل و نگاشت aggregate در companion Wear.
// وابستگی‌ها و لایه: تست native adapter؛ بدون Android device، network، backend یا دادهٔ شخصی.
// نکات تغییر و قیود: retry باید MessageId/EventId را ثابت نگه دارد و duplicate را رد کند.
package com.companyname.niko.wear

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Test
import org.json.JSONObject

class WearCompanionAdapterTest {
    @Test
    fun messageRoundTripContainsContractAndIds() {
        val message = message("message-1", "event-1", WearEventType.SMOKED)
        val restored = WearCompanionMessage.parse(message.serialize())

        assertNotNull(restored)
        assertEquals("message-1", restored!!.messageId)
        assertEquals("event-1", restored.eventId)
        assertEquals(WearEventType.SMOKED, restored.eventType)
        assertEquals(WearCompanionMessage.CURRENT_CONTRACT_VERSION, restored.contractVersion)
    }

    @Test
    fun invalidPayloadAndUnsupportedEventAreRejected() {
        assertFalse(WearPendingActionQueue(FakeStore()).enqueue("not-json"))
        val valid = JSONObject(message("message-1", "event-1", WearEventType.CRAVING).serialize())
        val invalidPayload = JSONObject(valid.getString("payload")).put("eventType", 99)
        val invalid = valid.put("payload", invalidPayload.toString()).toString()
        assertFalse(WearPendingActionQueue(FakeStore()).enqueue(invalid))
    }

    @Test
    fun duplicateMessageIdOrEventIdIsRejected() {
        val store = FakeStore()
        val queue = WearPendingActionQueue(store)
        assertTrue(queue.enqueue(message("message-1", "event-1", WearEventType.SMOKED).serialize()))
        assertFalse(queue.enqueue(message("message-1", "event-2", WearEventType.SMOKED).serialize()))
        assertFalse(queue.enqueue(message("message-2", "event-1", WearEventType.SMOKED).serialize()))
        assertEquals(1, queue.pending().size)
    }

    @Test
    fun failedDeliveryIsRetriedAfterReconnect() {
        val store = FakeStore()
        val transport = FakeTransport()
        val adapter = WearCompanionAdapter(
            transport,
            WearPendingActionQueue(store),
            nowUtc = { "2026-08-21T10:00:00Z" },
            idFactory = object : Iterator<String> {
                private val values = listOf("event-1", "message-1")
                private var index = 0
                override fun hasNext() = index < values.size
                override fun next() = values[index++]
            }.let { iterator -> { iterator.next() } }
        )

        transport.fail = true
        assertEquals(WearDeliveryState.PendingOffline, adapter.quickLog(WearEventType.RESISTED))
        assertEquals(1, store.messages.size)

        transport.fail = false
        assertEquals(1, adapter.retryPending())
        assertTrue(store.messages.isEmpty())
        assertEquals(1, transport.sent.size)
    }

    @Test
    fun summaryMappingAcceptsAggregatesOnly() {
        val transport = FakeTransport().apply {
            summary = """
                {"smokedToday":2,"resistedToday":1,"currentStreakDays":4,
                 "milestoneProgressPercent":50.0,"pendingCount":0,"inSync":true}
            """.trimIndent()
        }
        val result = WearCompanionAdapter(transport, WearPendingActionQueue(FakeStore())).loadSummary()

        assertTrue(result is WearTransportResult.Success)
        assertEquals(2, (result as WearTransportResult.Success).value.smokedToday)
    }

    private fun message(messageId: String, eventId: String, type: WearEventType) =
        WearCompanionMessage(
            messageId,
            eventId,
            type,
            "2026-08-21T10:00:00Z",
            "2026-08-21T10:00:00Z"
        )

    private class FakeStore : PendingActionStore {
        val messages = mutableListOf<String>()
        override fun load() = messages.toList()
        override fun save(messages: List<String>) {
            this.messages.clear()
            this.messages.addAll(messages)
        }
    }

    private class FakeTransport : WearPhoneTransport {
        var fail = false
        var summary: String? = null
        val sent = mutableListOf<String>()

        override fun send(serializedMessage: String): WearTransportResult<Unit> =
            if (fail) WearTransportResult.Failure(FailureReason.OFFLINE)
            else {
                sent += serializedMessage
                WearTransportResult.Success(Unit)
            }

        override fun requestSummary(): WearTransportResult<String> =
            summary?.let { WearTransportResult.Success(it) }
                ?: WearTransportResult.Failure(FailureReason.UNAVAILABLE)
    }
}
