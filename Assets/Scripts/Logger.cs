using UnityEngine;
using System.Collections.Generic;

public enum LogTypeEx
{
    Info,
    Warning,
    Error,
    Fatal
}

public struct LogStyle
{
    public Color Color;
    public string Prefix;

    public LogStyle(Color color, string prefix)
    {
        Color = color;
        Prefix = prefix;
    }
}

public class Logger : MonoBehaviour
{
    private static readonly Dictionary<LogTypeEx, LogStyle> Styles =
        new Dictionary<LogTypeEx, LogStyle>
        {
            { LogTypeEx.Info,    new LogStyle(Color.blue,  "INFO") },
            { LogTypeEx.Warning, new LogStyle(Color.yellow, "WARN") },
            { LogTypeEx.Error,   new LogStyle(Color.red,    "ERROR") },
            { LogTypeEx.Fatal,   new LogStyle(Color.red,"FATAL") }
        };

    public void Log(string msg, LogTypeEx type)
    {
        var style = Styles[type];
        string colorHex = ColorUtility.ToHtmlStringRGB(style.Color);
        string formattedMsg = $"<color=#{colorHex}>[{style.Prefix}] {msg}</color>";
        Debug.LogFormat(formattedMsg);
        if (type == LogTypeEx.Fatal)
        {
            Application.Quit();
        }
    }

    public void Info(string msg)
    {
        Log(msg, LogTypeEx.Info);
    }

    public void Warn(string msg)
    {
        Log(msg, LogTypeEx.Warning);
    }

    public void Error(string msg)
    {
        Log(msg, LogTypeEx.Error);
    }
    public void Fatal(string msg)
    {
        Log(msg, LogTypeEx.Fatal);
    }
}
