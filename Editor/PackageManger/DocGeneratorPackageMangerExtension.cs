using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace MacacaGames.DocGenerator
{
    [InitializeOnLoad]
    public class DocGeneratorPackageMangerExtension : IPackageManagerExtension
    {
        private UnityEditor.PackageManager.PackageInfo packageInfo;

        public VisualElement CreateExtensionUI()
        {
            return new IMGUIContainer(OnGUI);
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Open with Unity Doc Generator"))
            {
                DocGeneratorWindow.OpenWindow();
                DocGeneratorWindow.currentSelectPath = System.IO.Path.GetFullPath(packageInfo.assetPath);
                // Debug.Log(System.IO.Path.GetFullPath(packageInfo.assetPath));
                // Debug.Log(packageInfo.assetPath);
                // Debug.Log(packageInfo.resolvedPath);
            }
        }

        public void OnPackageSelectionChange(UnityEditor.PackageManager.PackageInfo packageInfo)
        {
            if (packageInfo == this.packageInfo)
                return;

            this.packageInfo = packageInfo;
        }

        public void OnPackageAddedOrUpdated(UnityEditor.PackageManager.PackageInfo packageInfo) { }
        public void OnPackageRemoved(UnityEditor.PackageManager.PackageInfo packageInfo) { }

        static DocGeneratorPackageMangerExtension()
        {
            PackageManagerExtensions.RegisterExtension(new DocGeneratorPackageMangerExtension());
        }
    }
}