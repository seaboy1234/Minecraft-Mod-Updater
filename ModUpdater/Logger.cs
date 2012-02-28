using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModUpdater
{
    public class Logger
    {
        List<string> StringLogs = new List<string>();
        List<Level> LevelLogs = new List<Level>();
        public enum Level
        {
            Info,
            Warning,
            Error
        }
        public void Log(Level l, string s)
        {
            StringLogs.Add(s);
            LevelLogs.Add(l);
            DebugMessageHandler.AssertCl("["+l.ToString().ToUpper()+"] " + s);
        }
        public void Log(Exception e)
        {
            Log(Level.Error, e.ToString());
        }
        public string[] GetMessages()
        {
            List<string> strs = new List<string>();
            for(int i = 0; i < StringLogs.Count; i++)
            {
                strs.Add(string.Format("[{0}] {1}", LevelLogs[i].ToString().ToUpper(), StringLogs[i]));
            }
            return strs.ToArray();
        }
    }
}
