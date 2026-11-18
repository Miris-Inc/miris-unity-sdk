// Copyright © 2026 Miris, Inc. All rights reserved.

using System;

namespace Miris.Runtime
{
    using ClientHandle = IntPtr;
    using TelemetryCollectorHandle = IntPtr;

    /// <summary>
    /// Owns a native telemetry collector. Create, tick and dispose it on the main thread.
    /// </summary>
    internal sealed class TelemetryCollector : IDisposable
    {
        private TelemetryCollectorHandle m_handle;

        // Reused every frame rather than allocated.
        private readonly ClientHandle[] m_samplerClients = new ClientHandle[1];

        /// <param name="sessionStartTimeMs">Unix epoch milliseconds.</param>
        /// <param name="mirisId">The root of every targetId this collector builds.</param>
        /// <param name="creationTimeMs">Unix epoch milliseconds the Miris was created, which the
        /// collector reports as its lifecycle create; &lt;= 0 means now.</param>
        public TelemetryCollector(string sessionId, double sessionStartTimeMs, bool isNewSession, uint mirisId,
                                  double creationTimeMs)
        {
            m_handle = MirisBindings.CreateTelemetryCollector(sessionId, sessionStartTimeMs, isNewSession, mirisId,
                                                              creationTimeMs);
        }

        /// <summary>
        /// Routes pending entries and drives flush timers. Call once per frame.
        /// </summary>
        /// <param name="monotonicTimeMs">Any origin.</param>
        public void Tick(double monotonicTimeMs)
        {
            MirisBindings.TickTelemetryCollector(m_handle, monotonicTimeMs);
        }

        internal TelemetryCollectorHandle Handle => m_handle;

        /// <summary>
        /// Reports the vitals of a client created with this collector. Call once per frame before
        /// <see cref="Tick"/>, outside the scene lock.
        /// </summary>
        public void TickSampler(Client client)
        {
            ClientHandle? handle = client.GetClientHandleInternal();
            if (handle == null)
            {
                return;
            }

            m_samplerClients[0] = handle.Value;
            MirisBindings.TickTelemetrySampler(m_samplerClients, m_samplerClients.Length);
        }

        public void EnqueueConfigurationMiris(MirisPackage package)
        {
            // Version and hostname stay blank until there is a runtime source for them.
            MirisBindings.EnqueueTelemetryConfigurationMiris(m_handle, Now(), package, "", "", "", "", "");
        }

        public void EnqueueConfigurationScene(uint sceneId, float width, float height, float renderWidth,
                                              float renderHeight, float pixelRatio, string viewerKey)
        {
            MirisBindings.EnqueueTelemetryConfigurationScene(m_handle, Now(), sceneId, width, height,
                                                             renderWidth, renderHeight, pixelRatio, viewerKey ?? "");
        }

        public void EnqueueError(ErrorEntryName name, string message)
        {
            MirisBindings.EnqueueTelemetryError(m_handle, Now(), name, message);
        }

        /// <param name="timestampMs">Session-relative milliseconds, as Now() returns them.</param>
        public void EnqueueTiming(double timestampMs, double durationMs, TargetType targetType, uint sceneId,
                                  TimingEntryName name)
        {
            MirisBindings.EnqueueTelemetryTiming(m_handle, timestampMs, durationMs, targetType, sceneId, 0, name);
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
