//    File:        TaskManager.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ModUpdater.Utility
{
    public static class TaskManager
    {
        public delegate void Task();
        private delegate void ThreadExit(Thread t);
        private static event ThreadExit te;
        private static int CurrentTaskId = 0;
        public delegate void Error(Exception e);
        public static event Error ExceptionRaised;
        /// <summary>
        /// Runs the task on a new thread.
        /// </summary>
        /// <param name="t">The Task to run.</param>
        public static void AddAsyncTask(Task t)
        {
            if (te == null) te += new ThreadExit(TaskManager_ThreadExit);
            Thread tr = new Thread(new ThreadStart(delegate { PerformTask(t); }));
            tr.IsBackground = true;
            tr.Name = "Task: " + CurrentTaskId.ToString();
            tr.Start();
        }

        static void TaskManager_ThreadExit(Thread t)
        {
            if (t != Thread.CurrentThread)
            {
                t.Join();
            }
        }
        /// <summary>
        /// Runs a task on a new thread after a spefifyed amount of time.
        /// </summary>
        /// <param name="t">The Task to run</param>
        /// <param name="delayInMs">The time to wait before running the task.  In miliseconds</param>
        public static void AddDelayedAsyncTask(Task t, int delayInMs)
        {
            if (te == null) te += new ThreadExit(TaskManager_ThreadExit);
            Thread tr = new Thread(new ThreadStart(delegate { Thread.Sleep(delayInMs); PerformTask(t); }));
            tr.IsBackground = true;
            tr.Name = "Task: " + CurrentTaskId.ToString();
            tr.Start();
        }
        /// <summary>
        /// Runs a task on the same thread.
        /// </summary>
        /// <param name="t">The task to run</param>
        public static void AddSyncTask(Task t)
        {
            PerformTask(t);
        }
        /// <summary>
        /// Runs a task after a spefifyed amount of time.
        /// </summary>
        /// <param name="t">The Task to run</param>
        /// <param name="delayInMs">The time to wait before running the task.  In miliseconds</param>
        public static void AddDelayedSyncTask(Task t, int delayInMs)
        {
            Thread.Sleep(delayInMs);
            PerformTask(t);
        }
        private static void PerformTask(Task t)
        {
            int tid = CurrentTaskId;
            CurrentTaskId++;
            try
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Running Task Id: " + tid.ToString());
                if (t != null)
                {
                    t.Invoke();
                    MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Task " + tid.ToString() + " Done");
                }
            }
            catch (Exception e) 
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Error, "Error on task " + tid.ToString());
                MinecraftModUpdater.Logger.Log(e);
                if (ExceptionRaised != null)
                    ExceptionRaised.Invoke(e);
            }
            Thread.CurrentThread.Abort();
            //te.Invoke(Thread.CurrentThread);
        }
    }
}
