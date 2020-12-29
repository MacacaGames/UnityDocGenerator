using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class Excuter
{
    public static string Bash(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (string.IsNullOrEmpty(result)) UnityEngine.Debug.Log(result);
        if (string.IsNullOrEmpty(error)) UnityEngine.Debug.LogError(process.StandardError.ReadToEnd());
        return result;
    }

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
