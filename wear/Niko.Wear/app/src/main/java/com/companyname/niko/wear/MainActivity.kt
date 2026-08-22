// مسئولیت: صفحهٔ حداقلی Wear OS برای سه QuickLog و نمایش aggregate امن.
// وابستگی‌ها و لایه: Wear UI → WearCompanionAdapter؛ هیچ محاسبه، persistence دامنه یا raw event ندارد.
// نکات تغییر و قیود: transport پیش‌فرض unavailable است و عملیات شکست‌خورده فقط در صف durable می‌مانند.
package com.companyname.niko.wear

import android.app.Activity
import android.os.Bundle
import android.view.Gravity
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView

class MainActivity : Activity() {
    private lateinit var adapter: WearCompanionAdapter
    private lateinit var smokedText: TextView
    private lateinit var resistedText: TextView
    private lateinit var summaryText: TextView
    private lateinit var statusText: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        adapter = WearCompanionAdapter(
            DataLayerWearPhoneTransport(this),
            WearPendingActionQueue(SharedPreferencesPendingActionStore(this))
        )
        setContentView(createContent())
        refreshSummary()
    }

    override fun onResume() {
        super.onResume()
        adapter.retryPending()
        refreshSummary()
    }

    private fun createContent(): LinearLayout {
        val content = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            gravity = Gravity.CENTER_HORIZONTAL
            setPadding(16, 12, 16, 12)
        }

        smokedText = label()
        resistedText = label()
        summaryText = label()
        statusText = label()
        content.addView(smokedText)
        content.addView(resistedText)
        content.addView(summaryText)
        content.addView(statusText)
        content.addView(actionButton(R.string.action_smoked, WearEventType.SMOKED))
        content.addView(actionButton(R.string.action_resisted, WearEventType.RESISTED))
        content.addView(actionButton(R.string.action_craving, WearEventType.CRAVING))
        return content
    }

    private fun label(): TextView = TextView(this).apply {
        textSize = 14f
        setPadding(0, 4, 0, 4)
    }

    private fun actionButton(labelId: Int, eventType: WearEventType): Button = Button(this).apply {
        setText(labelId)
        setOnClickListener {
            val state = adapter.quickLog(eventType)
            statusText.setText(
                when (state) {
                    WearDeliveryState.Delivered -> R.string.status_offline
                    WearDeliveryState.PendingOffline -> R.string.status_pending
                    WearDeliveryState.Duplicate -> R.string.status_pending
                    WearDeliveryState.Unavailable -> R.string.status_unavailable
                }
            )
        }
    }

    private fun refreshSummary() {
        when (val result = adapter.loadSummary()) {
            is WearTransportResult.Success -> {
                val summary = result.value
                smokedText.text = getString(R.string.smoked_today, summary.smokedToday)
                resistedText.text = getString(R.string.resisted_today, summary.resistedToday)
                summaryText.text = getString(
                    R.string.progress_summary,
                    summary.currentStreakDays,
                    summary.milestoneProgressPercent
                )
                statusText.text = if (summary.inSync) {
                    getString(R.string.status_offline)
                } else {
                    getString(R.string.status_pending, summary.pendingCount)
                }
            }
            is WearTransportResult.Failure -> {
                smokedText.setText(R.string.status_unavailable)
                resistedText.setText(R.string.status_unavailable)
                summaryText.setText(R.string.status_unavailable)
                statusText.setText(
                    if (result.reason == FailureReason.OFFLINE) {
                        R.string.status_offline
                    } else {
                        R.string.status_unavailable
                    }
                )
            }
        }
    }
}
