using System.Collections.Generic;

namespace MacacaGames.DocGenerator
{
    [System.Serializable]
    public class VersionDefine    {
        public string name { get; set; } 
        public string expression { get; set; } 
        public string define { get; set; } 
    }

    /// <summary>
    /// AsmdefFile json model
    /// </summary>
    [System.Serializable]
    public class AsmdefFile    {
        public string name { get; set; } 
        public List<string> references { get; set; } 
        public List<string> includePlatforms { get; set; } 
        public List<string> excludePlatforms { get; set; } 
        public bool allowUnsafeCode { get; set; } 
        public bool overrideReferences { get; set; } 
        public List<string> precompiledReferences { get; set; } 
        public bool autoReferenced { get; set; } 
        public List<string> defineConstraints { get; set; } 
        public List<VersionDefine> versionDefines { get; set; } 
        public bool noEngineReferences { get; set; } 
    }

    /// <summary>
    /// Docfx json model
    /// </summary>
    [System.Serializable]
    public class DocFxSetting
    {
        public List<Metadata> metadata { get; set; }
        public Build build { get; set; }
    }
    [System.Serializable]
    public class Src
    {
        public List<string> files { get; set; }
        public string src { get; set; }
    }

    [System.Serializable]
    public class Metadata
    {
        public List<Src> src { get; set; }
        public string dest { get; set; }
        public bool disableGitFeatures { get; set; }
        public bool disableDefaultFilter { get; set; }
    }

    [System.Serializable]
    public class Content
    {
        public List<string> files { get; set; }
    }

    [System.Serializable]
    public class Resource
    {
        public List<string> files { get; set; }
    }

    [System.Serializable]
    public class Overwrite
    {
        public List<string> files { get; set; }
        public List<string> exclude { get; set; }
    }

    [System.Serializable]
    public class Build
    {
        public List<Content> content { get; set; }
        public List<Resource> resource { get; set; }
        public List<Overwrite> overwrite { get; set; }
        public string dest { get; set; }
        public List<object> globalMetadataFiles { get; set; }
        public List<object> fileMetadataFiles { get; set; }
        public List<string> template { get; set; }
        public List<object> postProcessors { get; set; }
        public string markdownEngineName { get; set; }
        public bool noLangKeyword { get; set; }
        public bool keepFileLink { get; set; }
        public bool cleanupCacheHistory { get; set; }
        public bool disableGitFeatures { get; set; }
    }
}
