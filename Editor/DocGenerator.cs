using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Linq;

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
        static string SampleDocumentProjectPath => PackageRoot + "/DocFxTemplate";
        static string DocFxZip => PackageRoot + "/Tools/docfx.7z";
        static string DocWebPath => currentSelectPath + "/docs";
        static string DocFxPath => UnityProjectPath + "/Temp/docfx";
        static string DocFxExcuablePath => DocFxPath + "/docfx/docfx.exe";
        static string DocFxProjectPath => Path.Combine(currentSelectPath, DocFxProject);
        static string DocFxSettingFilePath => Path.Combine(DocFxProjectPath, "docfx.json");
        static string UnityProjectPath => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        static string ReadmeFilePath => Path.Combine(currentSelectPath, "Readme.md");
        static string MonoPath = "/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono";
        #endregion
        #region Command
        const int httpPort = 18080;
        #endregion

        public static DocGenerator Instance;
        static SimpleHTTPServer httpServer;

        static string _currentSelectPath = "";
        public static string currentSelectPath
        {
            set
            {
                _currentSelectPath = value;
                GetCsproj();
            }
            get
            {
                return _currentSelectPath;
            }
        }

        [MenuItem("MacacaGames/DocGenerator")]
        private static void OpenWindow()
        {
            Instance = GetWindow<DocGenerator>();
            Instance.titleContent = new GUIContent("DocGenerator");
            Instance.minSize = new Vector2(600, 400);

        }
        void OnEnable()
        {
            PrepareList();
        }

        string testCmd = "";
        Vector2 scrollPosition;
        static bool hosting = false;
        bool isGenerateAvailable
        {
            get
            {
                return !string.IsNullOrEmpty(currentSelectPath) && Directory.Exists(currentSelectPath) && Directory.Exists(DocFxProjectPath);
            }
        }
        ReorderableList list;
        public void PrepareList()
        {
            list = new ReorderableList(copyFolderToDocument, typeof(List<string>), true, false, true, true);
            list.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                copyFolderToDocument[index] = EditorGUI.TextField(rect, copyFolderToDocument[index]);
            };
            list.onAddCallback += (ReorderableList list) =>
            {
                copyFolderToDocument.Add("");
            };

        }



        void OnGUI()
        {
            using (var vertical = new GUILayout.VerticalScope("box"))
            {
                DrawLabel("Enviromnent");

                using (var disable = new EditorGUI.DisabledGroupScope(Application.platform != RuntimePlatform.OSXEditor))
                {
                    MonoPath = EditorGUILayout.TextField("Mono Path", MonoPath);
                    // EditorGUILayout.SelectableLabel("Run 'which mono' to get the full mono path which install in your computer");
                    EditorGUILayout.HelpBox("Run 'which mono' to get the full mono path which installed in your computer \n (Only required on macOS or Linux)", MessageType.Info);
                }

                using (var horizon = new GUILayout.HorizontalScope())
                {
                    currentSelectPath = EditorGUILayout.TextField("Working Folder", currentSelectPath);
                    if (GUILayout.Button("Select Folder", GUILayout.Width(150)))
                    {
                        currentSelectPath = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                    }
                }

                if (string.IsNullOrEmpty(currentSelectPath))
                {
                    GUILayout.Label("No thing select :)");
                }

                if (Directory.Exists(currentSelectPath) && !Directory.Exists(DocFxProjectPath))
                {
                    EditorGUILayout.HelpBox("No DocFx Project Found", MessageType.Error);
                    if (GUILayout.Button("Generate Document Project From sample"))
                    {
                        GenerateDocumentProject(); ;
                    }
                }
            }
            using (var disable = new EditorGUI.DisabledGroupScope(!isGenerateAvailable))
            {
                using (var horizon = new GUILayout.HorizontalScope())
                {
                    using (var vertical = new GUILayout.VerticalScope("box"))
                    {
                        DrawLabel("Metadata");
                        copyReadmeToDocfxIndex = GUILayout.Toggle(copyReadmeToDocfxIndex, "Copy Readme to Docfx index");
                        disableDefaultFilter = GUILayout.Toggle(disableDefaultFilter, "Disable Default Filter");
                        disableGitFeatures = GUILayout.Toggle(disableGitFeatures, "Disable Git Features");
                    }
                    using (var vertical = new GUILayout.VerticalScope("box"))
                    {
                        DrawLabel("Copy Folder To Document");
                        GUILayout.Label("Path is relative to Working Folder, will copy to generated document folder");
                        list.DoLayoutList();
                    }
                }

                using (var vertical = new GUILayout.VerticalScope("box"))
                {
                    if (csprojFiles == null || csprojFiles.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No Asmdef find in target Folder", MessageType.Error);
                    }
                    else
                    {
                        GUILayout.Label($"Found {csprojFiles.Count} asmdef file{(csprojFiles.Count > 1 ? "s" : "")}");

                        using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.Height(150)))
                        {
                            scrollPosition = scroll.scrollPosition;
                            foreach (var item in csprojFiles)
                            {
                                GUILayout.Label($"- {item}");
                            }
                        }
                    }
                }
                if (GUILayout.Button("Generate Document"))
                {
                    Docfx();
                }
            }

            GUILayout.FlexibleSpace();
            DrawHosting();
        }

        bool copyReadmeToDocfxIndex = true;
        bool disableDefaultFilter = false;
        bool disableGitFeatures = false;
        List<string> copyFolderToDocument = new List<string>();
        void Docfx()
        {
            //ToDo 
            CheckAndPrepareDocfx();

            // modify docfx.json
            var settingFileContent = File.ReadAllText(DocFxSettingFilePath);
            DocFxSetting setting = LitJson.JsonMapper.ToObject<DocFxSetting>(settingFileContent);

            setting.build.dest = "../docs";
            Metadata metadata;
            setting.metadata.Clear();
            //Create default metadata
            if (setting.metadata == null || setting.metadata.Count <= 0)
            {
                metadata = new Metadata();
            }
            else
            {
                metadata = setting.metadata[0];
            }
            if (metadata.src == null)
            {
                metadata.src = new List<Src>();
            }
            metadata.src.Clear();
            metadata.dest = "api";
            metadata.disableDefaultFilter = disableDefaultFilter;
            metadata.disableGitFeatures = disableGitFeatures;

            var src = new Src();
            src.files = csprojFiles;
            src.src = Utils.GetRelativePath(PackageRoot, UnityProjectPath);
            metadata.src.Add(src);
            setting.metadata.Add(metadata);

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
            string cmd = "";
#if UNITY_EDITOR_OSX
            cmd = $"{MonoPath} \"{DocFxExcuablePath}\" \"{DocFxSettingFilePath}\"";
#elif UNITY_EDITOR_WIN
       cmd = $"\"{DocFxExcuablePath}\" \"{DocFxSettingFilePath}\"";
#endif
            var r = cmd.Bash(DocFxProjectPath);

            // Do something while exit success

            if (Directory.Exists(DocWebPath) && copyFolderToDocument != null && copyFolderToDocument.Count > 0)
            {
                foreach (var item in copyFolderToDocument)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    try
                    {
                        if (Directory.Exists(DocWebPath + item))
                        {
                            Directory.Delete(DocWebPath + item);
                        }

                        Directory.CreateDirectory(DocWebPath + item);
                        Utils.DirectoryCopy(currentSelectPath + item, DocWebPath + item);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"error occur while copy folder: {item}, msg: {ex.Message}");
                    }
                }
            }

            Debug.Log(r.result);
            Debug.LogError(r.error);
        }

        static List<string> csprojFiles = new List<string>();
        static void GetCsproj()
        {
            if (string.IsNullOrEmpty(currentSelectPath))
            {
                return;
            }
            //Get csproj from asmdef 
            var asmdefFiles = Directory.GetFiles(currentSelectPath, "*.asmdef", SearchOption.AllDirectories);
            csprojFiles = GetCSProjName(asmdefFiles);
        }

        static List<string> GetCSProjName(string[] path)
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


        void DrawHosting()
        {
            using (var vertical = new GUILayout.VerticalScope("box"))
            {
                DrawLabel("Hosting");
                using (var disable = new EditorGUI.DisabledGroupScope(!Directory.Exists(DocWebPath)))
                {
                    using (var horizon = new GUILayout.HorizontalScope())
                    {
                        string url = $"http://127.0.0.1:{httpPort}/index.html";
                        EditorGUILayout.HelpBox(
                                           hosting ? $"Go {url} to preview your document \nCurrent hosting root: {httpServer.GetRootDirectory()}" : "Hosting is not running", MessageType.Warning);
                        using (var vertical2 = new GUILayout.VerticalScope("box", GUILayout.Width(100)))
                        {
                            if (GUILayout.Button("Open preview"))
                            {
                                Application.OpenURL(url);
                            }

                            using (var check = new EditorGUI.ChangeCheckScope())
                            {
                                hosting = GUILayout.Toggle(hosting, hosting ? "Hosting..." : "Start Hosting", EditorStyles.toolbarButton);
                                if (check.changed)
                                {
                                    SwitchHosting();
                                }
                            }
                        }
                    }
                }
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

        static void StopServer()
        {
            if (httpServer != null)
                httpServer.Stop();
        }
        void OnDestroy()
        {
            StopServer();
        }
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptReload()
        {
            csprojFiles.Clear();
            StopServer();
        }

        void DrawLabel(string label)
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            // GUILayout.Label("", new GUIStyle("TV Insertion"), GUILayout.Width(position.width));
        }

        [MenuItem("Assets/Open with Unity Doc Generator")]
        public static void OpenByDocGenerator()
        {
            string folderPath = Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.objects.FirstOrDefault()));
            // if (!string.IsNullOrEmpty(Path.GetExtension(folderPath)))
            // {
            //     Debug.LogError("select item is not a folder");
            //     return;
            // }
            OpenWindow();
            DocGenerator.currentSelectPath = folderPath;
        }

    }


}