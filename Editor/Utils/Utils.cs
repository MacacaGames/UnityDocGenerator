using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class Utils
{
    //From UnityEditor.PackageManager.DocumentationTools.UI
    internal static string Get7zPath
    {
        get
        {
#if (UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX)
            string execFilename = "7za";
#else
            string execFilename = "7z.exe";
#endif
            string zipper = UnityEditor.EditorApplication.applicationContentsPath + "/Tools/" + execFilename;
            if (!File.Exists(zipper))
                throw new FileNotFoundException("Could not find " + zipper);
            return zipper;
        }
    }

    /// <summary>
    /// A very simple method to get relativepath, only work if two path is base on same root
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <returns></returns>
    internal static string GetRelativePath(string path1, string path2)
    {
        string result = "../";
        var p1 = path1.Split(Path.DirectorySeparatorChar);
        var p2 = path2.Split(Path.DirectorySeparatorChar);
        for (int i = 0; i < Mathf.Abs(p1.Length - p2.Length); i++)
        {
            result += "../";
        }
        return result;
    }

    // Need to re-create this method since Unity's FileUtil equivalent (with overwrite) is internal only
    // From: https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
    public static void DirectoryCopy(string SourcePath, string DestinationPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
    }

    public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }

    // public static string GetGeneratorPath()
    // {
    //      "Packages/com.macacagames.docgenerator/";
    // }
}
