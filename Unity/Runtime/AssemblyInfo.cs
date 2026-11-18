// Copyright © 2026 Miris, Inc. All rights reserved.

using System.Runtime.CompilerServices;

// MirisStreamController exposes some fields (e.g. m_runtimeSettings, m_executionMode) as
// internal rather than public, since they are Miris-internal tuning/debug knobs and should not
// be part of the SDK's public editing surface. These assemblies still need runtime access:
// app_kit's developer UI and preferences persistence, this package's URP pass, its own test
// suites, and com.miris.sdk.internal's native calls (ENV switching, inspectables, settings).
[assembly: InternalsVisibleTo("Miris.AppKit.Runtime")]
[assembly: InternalsVisibleTo("Miris.SDK.Core.Runtime.URP")]
[assembly: InternalsVisibleTo("Miris.SDK.Core.Tests")]
[assembly: InternalsVisibleTo("Miris.SDK.Core.Editor.Tests")]
[assembly: InternalsVisibleTo("Miris.SDK.Internal.Runtime")]
