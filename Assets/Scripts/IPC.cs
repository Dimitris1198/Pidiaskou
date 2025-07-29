using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IPC
{
    System.Diagnostics.Process proc;

    public IPC(string scriptName)
    {
        proc = new System.Diagnostics.Process();
        proc.StartInfo.WorkingDirectory = Path.Join(Path.Join(Path.Join(Directory.GetCurrentDirectory(), "Assets"), "Scripts"), "Python");
        proc.StartInfo.FileName = "python.exe";
        proc.StartInfo.Arguments = $"{scriptName}.py";
        proc.StartInfo.RedirectStandardInput = true;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;
    }

    public void Start()
    {
        proc.Start();
    }

    public void Wait()
    {
        proc.WaitForExit();
    }

    public void Write(string payload)
    {
        proc.StandardInput.WriteLine(payload);
        proc.StandardInput.FlushAsync();
    }

    public string Read()
    {
        return proc.StandardOutput.ReadToEnd();
    }

    public void End()
    {
        proc.Kill();
    }
}

