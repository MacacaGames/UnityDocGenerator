using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace MacacaGames.DocGenerator
{

    public class DocGenerator : EditorWindow
    {
        private static string _persistentDataPath;
        internal static string PersistentDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_persistentDataPath))
                    _persistentDataPath = Application.persistentDataPath;

                return _persistentDataPath;
            }
        }
        private static string _dataPath;
        internal static string DataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dataPath))
                    _dataPath = Application.dataPath;

                return _dataPath;
            }
        }

        #region Path
        const string DocFxProject = "Document";
        static string PackageRoot { get { return Path.GetFullPath("packages/com.macacagames.docgenerator"); } }
        static string SampleDocumentProjectPath => PackageRoot + "/DocFxSample";
        static string DocFxZip => PackageRoot + "/Tools/docfx.7z";
        static string DocWebPath => currentSelectPath + "/docs";
        static string DocFxPath => PersistentDataPath + "/Temp";
        static string DocFxExcuablePath => PersistentDataPath + "/Temp/docfx/docfx.exe";
        static string DocFxProjectPath => Path.Combine(currentSelectPath, DocFxProject);
        static string DocFxSettingFilePath => Path.Combine(DocFxProjectPath, "docfx.json");
        static string UnityProjectPath => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        static string ReadmeFilePath => Path.Combine(PackageRoot, "Readme.md");
        static string MonoPath = "/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono";
        #endregion
        #region Command
        const int httpPort = 18080;
        #endregion

        static DocGenerator Instance;
        static SimpleHTTPServer httpServer;
        static string currentSelectPath;

        [MenuItem("MacacaGames/DocGenerator")]
        private static void OpenWindow()
        {
            Instance = GetWindow<DocGenerator>();
            Instance.titleContent = new GUIContent("DocGenerator");
            Instance.minSize = new Vector2(600, 400);

        }

        string testCmd = "";
        static bool hosting = false;
        void OnGUI()
        {
            using (var vertical = new EditorGUILayout.VerticalScope("box"))
            {
                testCmd = EditorGUILayout.TextField("cmd", testCmd);
                if (GUILayout.Button("Run test cmd", GUILayout.Width(150)))
                {
                    Debug.Log(testCmd.Bash());
                }

            }

            using (var disable = new EditorGUI.DisabledGroupScope(Application.platform != RuntimePlatform.OSXEditor))
            {
                MonoPath = EditorGUILayout.TextField("Mono Path", MonoPath);
                // EditorGUILayout.SelectableLabel("Run 'which mono' to get the full mono path which install in your computer");
                EditorGUILayout.HelpBox("Run 'which mono' to get the full mono path which installed in your computer", MessageType.Info);
            }

            using (var horizon = new GUILayout.HorizontalScope())
            {
                currentSelectPath = EditorGUILayout.TextField("Project Folder", currentSelectPath);
                if (GUILayout.Button("OpenFolder", GUILayout.Width(150)))
                {
                    currentSelectPath = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                    GetCsproj();
                }
            }

            if (string.IsNullOrEmpty(currentSelectPath))
            {
                GUILayout.Label("No thing select :)");
                return;
            }

            if (!Directory.Exists(currentSelectPath))
            {
                GUILayout.Label("Target path is not a Folder");
                return;
            }

            if (!Directory.Exists(DocFxProjectPath))
            {
                EditorGUILayout.HelpBox("No DocFx Project Found", MessageType.Error);
                if (GUILayout.Button("Generate Document Project From sample"))
                {
                    GenerateDocumentProject(); ;
                }
                return;
            }
            if (GUILayout.Button("Refresh Asmdef files"))
            {
                GetCsproj();
            }
            if (csprojFiles == null || csprojFiles.Count == 0)
            {
                EditorGUILayout.HelpBox("No Asmdef find in target Folder", MessageType.Error);
                return;
            }

            GUILayout.Label($"Found {csprojFiles.Count} asmdef file{(csprojFiles.Count > 1 ? "s" : "")}");
            foreach (var item in csprojFiles)
            {
                GUILayout.Label($"- {item}");
            }
            copyReadmeToDocfxIndex = GUILayout.Toggle(copyReadmeToDocfxIndex, "Copy Readme to Docfx index");
            if (GUILayout.Button("Generate Document"))
            {
                Docfx();
            }


            GUILayout.FlexibleSpace();
            using (var disable = new EditorGUI.DisabledGroupScope(!Directory.Exists(DocWebPath)))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    using (var horizon = new GUILayout.HorizontalScope())
                    {
                        string url = $"http://127.0.0.1:{httpPort}/index.html";
                        EditorGUILayout.HelpBox(
                                           hosting ? $"Go {url} to preview your document" : "Hosting is not running", MessageType.Warning);
                        if (GUILayout.Button("Open preview", GUILayout.Width(100)))
                        {
                            Application.OpenURL(url);
                        }
                    }

                    hosting = GUILayout.Toggle(hosting, hosting ? "Hosting..." : "Start Hosting", EditorStyles.toolbarButton);
                    if (check.changed)
                    {
                        SwitchHosting();
                    }
                }
            }

        }

        bool copyReadmeToDocfxIndex = false;
        void Docfx()
        {
            //ToDo 
            CheckAndPrepareDocfx();

            // modify docfx.json
            var settingFileContent = File.ReadAllText(DocFxSettingFilePath);
            DocFxSetting setting = LitJson.JsonMapper.ToObject<DocFxSetting>(settingFileContent);

            setting.build.dest = "../docs";
            //Create default metadata
            if (setting.metadata == null || setting.metadata.Count <= 0)
            {
                var metadata = new Metadata();
                metadata.dest = "api";
                metadata.disableDefaultFilter = false;
                metadata.disableGitFeatures = false;
                setting.metadata.Add(metadata);
            }
            setting.metadata[0].src.Clear();
            var src = new Src();
            src.files = csprojFiles;
            src.src = Utils.GetRelativePath(PackageRoot, UnityProjectPath);
            setting.metadata[0].src.Add(src);

            var modifiedJson = LitJson.JsonMapper.ToJson(setting);
            File.WriteAllText(DocFxSettingFilePath, modifiedJson);

            // modify index.md
            if (copyReadmeToDocfxIndex)
            {
                if (File.Exists(ReadmeFilePath))
                {
                    var readme = File.ReadAllText(ReadmeFilePath);
                    File.WriteAllText(Path.Combine(DocFxProjectPath, "index.md"), readme);
                }
                else
                {
                    Debug.LogError("copyReadmeToDocfxIndex is checked, but no Readme.md found in your folder");
                }
            }

            var cmd = $"{MonoPath} \"{DocFxExcuablePath}\" \"{DocFxSettingFilePath}\"";
            cmd.Bash();
        }

        List<string> csprojFiles = new List<string>();
        void GetCsproj()
        {
            //Get csproj from asmdef 
            var asmdefFiles = Directory.GetFiles(currentSelectPath, "*.asmdef", SearchOption.AllDirectories);
            csprojFiles = GetCSProjName(asmdefFiles);
        }

        List<string> GetCSProjName(string[] path)
        {
            List<string> result = new List<string>();
            foreach (var item in path)
            {
                var json = File.ReadAllText(item);
                AsmdefFile setting = LitJson.JsonMapper.ToObject<AsmdefFile>(json);
                result.Add(setting.name + ".csproj");
            }
            return result;
        }

        void GenerateDocumentProject()
        {

            Utils.DirectoryCopy(SampleDocumentProjectPath, DocFxProjectPath);

        }

        void CheckAndPrepareDocfx()
        {
            if (!Directory.Exists(DocFxPath))
            {
                Excuter.Unzip(DocFxZip, DocFxPath);
            }
        }
        void SwitchHosting()
        {
            if (hosting)
            {
                StopServer();
                httpServer = new SimpleHTTPServer(DocWebPath, httpPort, 32);
            }
            else
            {
                StopServer();
            }
        }
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void StopServer()
        {
            hosting = false;
            if (httpServer != null)
                httpServer.Stop();
        }
    }


}