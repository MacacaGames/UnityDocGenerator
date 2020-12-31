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
        string packagePath;
        public VisualElement CreateExtensionUI()
        {
            return new IMGUIContainer(OnGUI);
        }

        private void OnGUI()
        {
            using (var horizon = new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Open with Unity Doc Generator", "Open Unity Doc Generator for more detail settings")))
                {
                    DocGeneratorWindow.OpenWindow();
                    DocGeneratorWindow.currentSelectPath = packagePath;
                }
                string p1 = System.IO.Path.Combine(packagePath, DocGeneratorWindow.settingFile);
                string p2 = System.IO.Path.Combine(packagePath, DocGeneratorWindow.DocFxProject);
                bool isSettingExsit = System.IO.File.Exists(p1);
                bool isDocFxDocumentExsit = System.IO.Directory.Exists(p2);
                bool buildDirectAvailable = !(isSettingExsit && isDocFxDocumentExsit);
                using (var disable = new EditorGUI.DisabledGroupScope(buildDirectAvailable))
                {
                    if (GUILayout.Button(new GUIContent("Generate Doc with last setting", "Require the package has generate document using UnityDocGenerator before")))
                    {
                        DocGeneratorWindow.OpenWindow();
                        DocGeneratorWindow.currentSelectPath = System.IO.Path.GetFullPath(packageInfo.assetPath);
                        DocGeneratorWindow.Instance.Docfx();
                    }
                }
            }
        }

        public void OnPackageSelectionChange(UnityEditor.PackageManager.PackageInfo packageInfo)
        {
            if (packageInfo == this.packageInfo)
                return;
            this.packageInfo = packageInfo;
            this.packagePath = System.IO.Path.GetFullPath(packageInfo.assetPath);
        }

        public void OnPackageAddedOrUpdated(UnityEditor.PackageManager.PackageInfo packageInfo) { }
        public void OnPackageRemoved(UnityEditor.PackageManager.PackageInfo packageInfo) { }

        static DocGeneratorPackageMangerExtension()
        {
            PackageManagerExtensions.RegisterExtension(new DocGeneratorPackageMangerExtension());
        }
    }
}