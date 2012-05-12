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
using System.Windows.Forms;

namespace ModUpdater.Utility
{
    public static class TaskManager
    {
        public delegate void Task();
        private static int CurrentTaskId = 0;
        public delegate void Error(Exception e);
        public static event Error ExceptionRaised = delegate { };
        private static List<Thread> TaskThreads;
        private static object taskLock;
        private static Queue<Task> TaskQueue;
        private static Queue<Task> ImportantTaskQueue;
        private static Dictionary<Task, int> DelayedTasks;

        static TaskManager()
        {
            TaskThreads = new List<Thread>(16);
            TaskQueue = new Queue<Task>();
            ImportantTaskQueue = new Queue<Task>();
            DelayedTasks = new Dictionary<Task,int>();
            taskLock = new object();
            for (int i = 0; i < 15; i++)
            {
                SpawnTaskThread(ThreadRole.Standard);
            }
            SpawnTaskThread(ThreadRole.Important);
            SpawnTaskThread(ThreadRole.Delayed);
            Application.ApplicationExit += new EventHandler(ApplicationExit);
        }
        /// <summary>
        /// Runs the task on a new thread.
        /// </summary>
        /// <param name="t">The Task to run.</param>
        /// <param name="r">The ThreadRole to be used.</param>
        /// <param name="delayLen">The time to wait, in ms, until the task is run.</param>
        public static void AddAsyncTask(Task t, ThreadRole r = ThreadRole.Standard, int delayLen = 1000)
        {
            switch (r)
            {
                case ThreadRole.Standard:
                    TaskQueue.Enqueue(t);
                    break;
                case ThreadRole.Important:
                    ImportantTaskQueue.Enqueue(t);
                    break;
                case ThreadRole.Delayed:
                    DelayedTasks.Add(t, delayLen);
                    break;

            }
        }
        private static void PerformTask(Task t)
        {
            CurrentTaskId++;
            int tid = CurrentTaskId;
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
                ExceptionRaised.Invoke(e);
            }
        }
        public static void SpawnTaskThread(ThreadRole role)
        {
            Thread t;
            switch (role) //Set properties.
            {
                case ThreadRole.Delayed:
                    t = new Thread(new ThreadStart(ManageTaskDelayThread));
                    t.IsBackground = true;
                    t.Name = "Delayed Task Manager";
                    break;
                case ThreadRole.Important:
                    t = new Thread(new ThreadStart(ManageImportantTaskThread));
                    t.IsBackground = false;
                    t.Name = "Important Task Manager";
                    break;
                case ThreadRole.Standard:
                default:
                    t = new Thread(new ThreadStart(ManageTaskThread));
                    t.IsBackground = true;
                    t.Name = "Standard Task Manager";
                    break;
            }
            t.Start(); //Start the new thread.
        }
        #region Thread Managers
        /// <summary>
        /// Manage the current thread as a task thread.
        /// </summary>
        private static void ManageTaskThread()
        {
            TaskThreads.Add(Thread.CurrentThread);
            int tLoopId = TaskThreads.Count;
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Task Thread " + tLoopId + "  has started.");
            while (true)
            {
                try
                {
                        if (TaskQueue.Peek() != null)
                        {
                            Task t;
                            lock (taskLock)
                            {
                                t = TaskQueue.Dequeue();
                            }
                            PerformTask(t);
                        }
                }
                catch { } //Queue is most likely empty, no real need to do anything.
                Thread.Sleep(250);
            }
        }
        private static void ManageImportantTaskThread()
        {
            TaskThreads.Add(Thread.CurrentThread);
            int tLoopId = TaskThreads.Count;
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Task Thread " + tLoopId + "  has started. \r\nThis thread manages important tasks");
            while (TaskThreads[0].IsAlive)
            {
                try
                {
                    if (ImportantTaskQueue.Peek() != null)
                    {
                        Task t;
                        lock (taskLock)
                        {
                            t = ImportantTaskQueue.Dequeue();
                        }
                        PerformTask(t);
                    }
                }
                catch { } //Queue is most likely empty, no real need to do anything.
                Thread.Sleep(250);
            }
        }
        private static void ManageTaskDelayThread()
        {
            TaskThreads.Add(Thread.CurrentThread);
            int tLoopId = TaskThreads.Count;
            MinecraftModUpdater.Logger.Log(Logger.Level.Info, "Task Thread " + tLoopId + "  has started.\r\nThis thread manages delayed tasks.");
            while (TaskThreads[0].IsAlive)
            {
                try
                {
                    foreach (var v in DelayedTasks)
                    {
                        //if (v.Key == null) continue;
                        DelayedTasks[v.Key] -= 10;
                        if (v.Value < 1)
                        {
                            TaskQueue.Enqueue(v.Key);
                            DelayedTasks.Remove(v.Key);
                        }

                    }
                    Thread.Sleep(10);
                }
                catch { } //Queue is most likely empty, no real need to do anything.
            }
        }
        #endregion
        //Used to ensure all threads are exited when the app closes.
        private static void ApplicationExit(object sender, EventArgs e)
        {
            foreach (Thread t in TaskThreads)
            {
                t.Abort();
            }
        }
    }
    public enum ThreadRole
    {
        Standard,
        Important,
        Delayed
    }
}
