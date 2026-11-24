// Copyright © 2026 Miris, Inc. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace Miris.Editor
{
    /// <summary>
    /// Adds Miris Stream to the GameObject menu and the Hierarchy's right-click Create menu.
    /// </summary>
    internal static class MirisStreamCreateMenu
    {
        private const int c_menuPriority = 10;

        [MenuItem("GameObject/Miris/Stream", false, c_menuPriority)]
        private static void CreateStream(MenuCommand command)
        {
            var contextObject = command.context as GameObject;
            Transform parent = contextObject != null ? contextObject.transform : null;
            var payload = new MirisStreamDragPayload { AssetId = "" };
            MirisStreamAssetBrowserDropHandler.SpawnMirisStreamInstance(payload, parent, worldPosition: null);
        }
    }
}
