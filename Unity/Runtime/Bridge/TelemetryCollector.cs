// Copyright © 2026 Miris, Inc. All rights reserved.

using System;

namespace Miris.Runtime
{
    using TelemetryCollectorHandle = IntPtr;

    /// <summary>
    /// Owns a native telemetry collector. Create, tick and dispose it on the main thread.
    /// </summary>
    internal sealed class TelemetryCollector : IDisposable
    {
        private TelemetryCollectorHandle m_handle;

        /// <param name="sessionStartTimeMs">Unix epoch milliseconds.</param>
        /// <param name="mirisId">The root of every targetId this collector builds.</param>
        public TelemetryCollector(string sessionId, double sessionStartTimeMs, bool isNewSession, uint mirisId)
        {
            m_handle = MirisBindings.CreateTelemetryCollector(sessionId, sessionStartTimeMs, isNewSession, mirisId);
        }

        /// <summary>
        /// Routes pending entries and drives flush timers. Call once per frame.
        /// </summary>
        /// <param name="monotonicTimeMs">Any origin.</param>
        public void Tick(double monotonicTimeMs)
        {
            MirisBindings.TickTelemetryCollector(m_handle, monotonicTimeMs);
        }

        /// <summary>
        /// Session-relative time: milliseconds since the session start, on the collector's
        /// monotonic clock. Pass it as an Enqueue* timestamp.
        /// </summary>
        public double Now()
        {
            return MirisBindings.GetTelemetryCollectorNow(m_handle);
        }

        /// <summary>
        /// Sends this collector's transports now, e.g. when the app is backgrounded.
        /// </summary>
        public void Flush()
        {
            MirisBindings.FlushTelemetryCollector(m_handle);
        }

        public void Dispose()
        {
            if (m_handle != IntPtr.Zero)
            {
                MirisBindings.DestroyTelemetryCollector(m_handle);
                m_handle = IntPtr.Zero;
            }
        }
    }
}
