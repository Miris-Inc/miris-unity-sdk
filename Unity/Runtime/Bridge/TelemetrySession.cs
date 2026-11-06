// Copyright © 2026 Miris, Inc. All rights reserved.

using System;
using UnityEngine;

namespace Miris.Runtime
{
    /// <summary>
    /// The telemetry session: one per Play mode run, shared by every collector in it.
    /// </summary>
    internal static class TelemetrySession
    {
        public static string Id { get; private set; }

        /// <summary>
        /// Unix epoch milliseconds.
        /// </summary>
        public static double StartTimeMs { get; private set; }

        private static bool s_claimed;

        // SubsystemRegistration runs at the start of every Play mode run, even with domain reload off.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Id = Guid.NewGuid().ToString();
            StartTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            s_claimed = false;
        }

        /// <summary>
        /// True for the session's first collector only, which emits the session entry.
        /// </summary>
        public static bool ClaimNewSession()
        {
            bool isNewSession = !s_claimed;
            s_claimed = true;
            return isNewSession;
        }
    }
}
