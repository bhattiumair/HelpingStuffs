using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ConsoleLogManager : Singleton<ConsoleLogManager>
{
    public string CurrentFileName => currentFileName;
    private string currentFileName = "";
    public string PreviousFileName => previousFileName;
    private string previousFileName = "";
    StreamWriter fileStream;
    string temp;
    private StringBuilder logStringBuilder = new StringBuilder();
    public const string specialLog = "SPLog:";
    public List<string> specialLogsData = new List<string>();
    private void OnEnable()
    {
        currentFileName = Path.Combine(Application.persistentDataPath, "ConsoleLogs.txt");
        previousFileName = Path.Combine(Application.persistentDataPath, "PreviouseConsoleLogs.txt");
        Application.logMessageReceived += Handle_logMessageReceived;
        Init();
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Handle_logMessageReceived;
    }
    private void Init()
    {
        GameObject.DontDestroyOnLoad(gameObject);

        if (File.Exists(previousFileName))
        {
            File.Delete(previousFileName);
        }
        if (File.Exists(currentFileName))
        {
            File.Move(currentFileName, previousFileName);
        }
        // fileStream = File.CreateText(currentFileName);
    }

    public void Handle_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        string appendValue = "";
        if (type != LogType.Log)
            appendValue = "\n" + stackTrace;
        temp = $"{type.ToString()}#&{condition}{appendValue}|?";
        File.AppendAllText(currentFileName, temp.Replace("|?", "\n").Replace("#&", " : "));
        if (type == LogType.Error || type == LogType.Exception)
        {
            File.AppendAllText(currentFileName,"Stack Trace: " + stackTrace + "\n");
        }
        // fileStream.Write(temp);
        logStringBuilder.Append(temp);
    }
    public string GetFormatedText(string inputString = "")
    {
        string newText = inputString == "" ? new string(logStringBuilder.ToString()) : inputString;
        newText = newText.Replace($"|?", "\n");
        newText = newText.Replace($"{LogType.Log.ToString()}#&SPLog", "<color=\"purple\">-> <color=\"green\">"); // Add this BEFORE the regular Log line
        newText = newText.Replace($"{LogType.Log.ToString()}#&", "<color=\"purple\">-> <color=\"white\">");
        newText = newText.Replace($"{LogType.Warning.ToString()}#&", "<color=\"purple\">->  <color=\"yellow\">");
        newText = newText.Replace($"{LogType.Error.ToString()}#&", "<color=\"purple\">-> <color=\"red\">");
        newText = newText.Replace($"{LogType.Exception.ToString()}#&", "<color=\"purple\">-> <color=\"orange\">");
        return newText;
    }

    public string GetFormatedText(LogType type)
    {
        string[] lines = logStringBuilder.ToString().Split("|?");
        string lastString = "";
        foreach (string line in lines)
        {
            string[] newLine = line.Split("#&");
            if (type.ToString().Equals(newLine[0]))
            {
                lastString += line + "|?";
            }
        }
        return GetFormatedText(lastString);
    }
    public string GetFormatedError(string logString = null)
    {
        string[] lines = logString == null ? logStringBuilder.ToString().Split("|?") : logString.Split("|?");
        string lastString = "";
        foreach (string line in lines)
        {
            string[] newLine = line.Split("#&");
            if (LogType.Error.ToString().Equals(newLine[0]) || LogType.Exception.ToString().Equals(newLine[0]))
            {
                lastString += line + "|?";
            }
        }
        return GetFormatedText(lastString);
    }
    int specialLogIndex = 0;
    public void AddSpecialLog(string logString)
    {
        string log = specialLog + logString;
        specialLogsData.Insert(0,log);

        if (specialLogsData.Count > 20)
        {
            specialLogsData.RemoveAt(specialLogsData.Count - 1);
        }

    }
    public string GetSpecialLog()
    {
        string logString = "";
        foreach (string log in specialLogsData)
        {
            logString += log + "\n";
        }
        return logString;
    }
    public string GetFormatedSearch(string searchKey)
    {
        string[] lines = logStringBuilder.ToString().Split("|?");
        string lastString = "";
        foreach (string line in lines)
        {
            if (line.Contains(searchKey))
            {
                lastString += line + "|?";
            }
        }
        return GetFormatedText(lastString);
    }

    public string GetCurrentLog()
    {
        return GetFormatedError();
        return GetFormatedText(logStringBuilder.ToString());
    }
    public string GetPreviouseLog()
    {
        return GetFormatedText(File.ReadAllText(previousFileName));
    }
    public void ShareCurrentLog()
    {
        ShareFile(currentFileName);
    }
    public void SharePreviouseLog()
    {
        ShareFile(previousFileName);
    }

    public void ShareFile(string path)
    {
        NativeShare nativeShare = new NativeShare();
        nativeShare.AddFile(path, "text/plain");
        nativeShare.SetSubject("Console Logs");
        nativeShare.SetText("Console Logs from Sand Blast Game");
        nativeShare.SetTitle("Share Console Logs");
        nativeShare.Share();
    }

    // protected override bool ShouldPersist()
    // {
    //     return true;
    // }
}
