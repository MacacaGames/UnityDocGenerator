using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class Excuter
{
    static string result = "";
    static string error = "";
    public static (string result, string error) Bash(this string cmd, string workingDirectory = "")
    {

        ProcessStartInfo startInfo;
#if UNITY_EDITOR_OSX
        var escapedArgs = cmd.Replace("\"", "\\\"");

        startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{escapedArgs}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
#elif UNITY_EDITOR_WIN	
       var escapedArgs = cmd;

        startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };
#endif

        var process = new Process()
        {
            StartInfo = startInfo
        };


        // UnityEngine.Debug.Log(escapedArgs);
        process.Start();
        string line = process.StandardOutput.ReadLine();
        while (line != null)
        {
            result += line + "\n";
            // UnityEngine.Debug.Log("line:" + line);
            line = process.StandardOutput.ReadLine();
        }
        string errorLine = process.StandardOutput.ReadLine();
        while (errorLine != null)
        {
            error += errorLine + "\n";
            // UnityEngine.Debug.Log("line:" + line);
            errorLine = process.StandardOutput.ReadLine();
        }
        process.WaitForExit();
        return (result, error);
    }

    //From UnityEditor.PackageManager.DocumentationTools.UI
    internal static bool Unzip(string zipFilePath, string destPath)
    {
        string zipper = Utils.Get7zPath;
        string inputArguments = string.Format("x -y -o\"{0}\" \"{1}\"", destPath, zipFilePath);
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.FileName = zipper;
        startInfo.Arguments = inputArguments;
        startInfo.RedirectStandardError = true;
        var process = Process.Start(startInfo);
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new IOException(string.Format("Failed to unzip:\n{0} {1}\n\n{2}", zipper, inputArguments, process.StandardError.ReadToEnd()));

        return true;
    }

}
