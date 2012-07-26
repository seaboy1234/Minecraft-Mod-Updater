//    File:        TaskThread.cs
//    Copyright:   Copyright (C) 2012 Christian Wilson. All rights reserved.
//    Website:     https://github.com/seaboy1234/Minecraft-Mod-Updater
//    Description: This is intended to help Minecraft server owners who use mods make the experience of adding new mods and updating old ones easier for everyone.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System.Threading;

namespace ModUpdater.Utility
{
    public class TaskThread
    {
        public ThreadRole Role { get; private set; }
        public ThreadStart ThreadDelegate { get; private set; }
        public int ThreadID { get; private set; }
        public Thread Thread { get; private set; }
        public bool Background { get; private set; }
        public bool IsAlive { get; private set; }
        public bool Busy
        {
            get { return busy; }
            set
            {
                if (Thread.CurrentThread == Thread)
                {
                    busy = value;
                }
            }
        }

        private bool busy;

        public static TaskThread SpawnTaskThread(ThreadRole Role, ThreadStart Void, int ID, bool IsBackground)
        {
            return new TaskThread { Role = Role, ThreadDelegate = Void, ThreadID = ID, Background = IsBackground };
        }
        public void Start()
        {
            Thread = new Thread(ThreadDelegate);
            Thread.IsBackground = Background;
            Thread.Name = string.Format("{0} Task Thread (TID: {1})", Role, ThreadID);
            Thread.Start();
            IsAlive = true;
        }
        public void Stop()
        {
            if (Thread == null)
            {
                IsAlive = false;
                return;
            }
            if (Thread.IsAlive)
                Thread.Abort();
            IsAlive = false;
        }
    }
}
