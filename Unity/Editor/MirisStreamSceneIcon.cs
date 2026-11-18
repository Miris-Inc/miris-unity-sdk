// Copyright © 2026 Miris, Inc. All rights reserved.

using System.IO;

using Miris.Runtime;

using UnityEditor;

namespace Miris.Editor
{
    /// <summary>
    /// Hides the MirisStream icon in the Scene view once per project. The icon still shows in the
    /// Inspector, Add Component and Project windows, and a user who turns it back on in the
    /// Gizmos menu keeps that choice.
    /// </summary>
    [InitializeOnLoad]
    internal static class MirisStreamSceneIcon
    {
        // Relative to the project root. Unity keeps gizmo settings in Library/AnnotationManager,
        // so clearing Library resets the marker and the setting together.
        private const string c_hiddenMarkerPath = "Library/Miris/StreamSceneIconHidden";

        static MirisStreamSceneIcon()
        {
            if (File.Exists(c_hiddenMarkerPath))
            {
                return;
            }

            SceneView.duringSceneGui += TryHideIcon;
        }

        // Unity creates a script's gizmo entry lazily and ignores SetIconEnabled until it exists,
        // so retry on Scene view events until it does.
        private static void TryHideIcon(SceneView sceneView)
        {
            if (!GizmoUtility.TryGetGizmoInfo(typeof(MirisStream), out GizmoInfo _))
            {
                return;
            }

            SceneView.duringSceneGui -= TryHideIcon;
            GizmoUtility.SetIconEnabled(typeof(MirisStream), false);
            Directory.CreateDirectory(Path.GetDirectoryName(c_hiddenMarkerPath));
            File.WriteAllText(c_hiddenMarkerPath, string.Empty);
        }
    }
}
