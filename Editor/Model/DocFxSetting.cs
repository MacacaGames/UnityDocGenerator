using System.Collections.Generic;
using UnityEngine;

namespace MacacaGames.DocGenerator
{
    [System.Serializable]
    public class VersionDefine
    {
        [SerializeField]
        public string name;
        public string expression;
        public string define;
    }

    /// <summary>
    /// AsmdefFile json model
    /// </summary>
    [System.Serializable]
    public class AsmdefFile
    {
        public string name;
        public List<string> references;
        public List<string> includePlatforms;
        public List<string> excludePlatforms;
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public List<string> precompiledReferences;
        public bool autoReferenced;
        public List<string> defineConstraints;
        public List<VersionDefine> versionDefines;
        public bool noEngineReferences;
    }

    /// <summary>
    /// Docfx json model
    /// </summary>
    [System.Serializable]
    public class DocFxSetting
    {
        public List<Metadata> metadata;
        public Build build;
    }
    [System.Serializable]
    public class Src
    {
        public List<string> files;
        public string src;
    }

    [System.Serializable]
    public class Metadata
    {
        public List<Src> src;
        public string dest;
        public bool disableGitFeatures;
        public bool disableDefaultFilter;
        public string filter= "";
    }

    [System.Serializable]
    public class Content
    {
        public string src ="";
        public List<string> files;
    }

    [System.Serializable]
    public class Resource
    {
        public string src = "";
        public List<string> files;
    }

    [System.Serializable]
    public class Overwrite
    {
        public List<string> files;
        public List<string> exclude;
    }

    [System.Serializable]
    public class Build
    {
        public List<Content> content;
        public List<Resource> resource;
        public List<Overwrite> overwrite;
        public string dest;
        public List<object> globalMetadataFiles;
        public List<object> fileMetadataFiles;
        public List<string> template;
        public List<object> postProcessors;
        public string markdownEngineName;
        public bool noLangKeyword;
        public bool keepFileLink;
        public bool cleanupCacheHistory;
        public bool disableGitFeatures;
    }
}
