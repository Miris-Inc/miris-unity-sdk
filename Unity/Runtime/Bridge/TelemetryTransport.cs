// Copyright © 2026 Miris, Inc. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Miris.Runtime
{
    /// <summary>
    /// Sends what native telemetry flushes, from the main thread.
    /// </summary>
    internal static class TelemetryTransport
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void TransportCallback(IntPtr url, IntPtr payload, UIntPtr payloadSize);

        // Native code keeps the function pointer for the life of the process, so the delegate must
        // never be collected.
        private static readonly TransportCallback s_callback = OnFlush;

        private static SynchronizationContext s_mainThread;

        /// <summary>
        /// Call on the main thread before creating a collector.
        /// </summary>
        public static void Install()
        {
            s_mainThread = SynchronizationContext.Current;
            MirisBindings.SetTelemetryTransportCallback(Marshal.GetFunctionPointerForDelegate(s_callback));
        }

        [AOT.MonoPInvokeCallback(typeof(TransportCallback))]
        private static void OnFlush(IntPtr url, IntPtr payload, UIntPtr payloadSize)
        {
            // url is only valid during this call. The CDN transport sends no payload.
            string pixelUrl = Marshal.PtrToStringAnsi(url);

            if (SynchronizationContext.Current == s_mainThread)
            {
                SendTemporary(pixelUrl);
            }
            else
            {
                s_mainThread.Post(_ => SendTemporary(pixelUrl), null);
            }
        }

        // GETs a pixel URL that carries the entries. Fire and forget: a failed request is dropped.
        // TODO: OnFlush goes back to Send once the server takes POSTed batches.
        private static async void SendTemporary(string url)
        {
            try
            {
                using UnityWebRequest request = UnityWebRequest.Get(url);
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MirisDebug.Log($"Telemetry pixel to {url.Split('?')[0]} failed: {request.error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        // Fire and forget: a failed batch is dropped, not retried.
        private static async void Send(string url, byte[] body)
        {
            try
            {
                using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                request.uploadHandler = new UploadHandlerRaw(body) { contentType = "application/json" };
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MirisDebug.Log($"Telemetry post to {url} failed: {request.error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
