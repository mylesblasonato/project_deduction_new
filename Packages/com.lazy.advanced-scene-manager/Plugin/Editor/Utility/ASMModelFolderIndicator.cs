﻿using System.IO;
using System.Linq;
using AdvancedSceneManager.Models;
using AdvancedSceneManager.Models.Internal;
using AdvancedSceneManager.Utility;
using UnityEditor;
using UnityEngine;

namespace AdvancedSceneManager.Editor.Utility
{

    [InitializeInEditor]
    static class ASMModelFolderIndicator
    {

        static ASMModelFolderIndicator() =>
            SceneManager.OnInitialized(() =>
                EditorApplication.projectWindowItemOnGUI += (string guid, Rect rect) =>
                {

                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(path) && path.Contains(Assets.assetPath))
                    {

                        var name = Path.GetFileName(path);
                        var scene = Assets.allAssets.OfType<ASMModelBase>().FirstOrDefault(s => s.id == name);
                        if (scene)
                        {

                            var content = new GUIContent(((Object)scene).name);
                            var size = EditorStyles.label.CalcSize(content);
                            var w = Mathf.Min(size.x + 4, rect.width);
                            rect = new Rect(rect.xMax - w, rect.y, w, rect.height);

                            var c = GUI.color;
                            GUI.color = Color.gray;
                            GUI.Label(rect, content);
                            GUI.color = c;

                        }

                    }

                });

    }

}
