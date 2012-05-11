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
        private static int CurrentTaskId = 0;
        public delegate void Error(Exception e);
        public static event Error ExceptionRaised = delegate { };
        private static List<Thread> TaskThreads;
        private static object taskLock;
        private static Queue<Task> TaskQueue;
        private static Queue<Task> ImportantTaskQueue;

        static TaskManager()
        {
            TaskThreads = new List<Thread>(16);
            TaskQueue = new Queue<Task>();
            ImportantTaskQueue = new Queue<Task>();
            taskLock = new object();
            Thread.Sleep(100);
            for (int i = 0; i < 15; i++)
            {
                Thread t = new Thread(ManageTaskThread, 10000);
                t.IsBackground = true;
                t.Name = "Task Thread " + (TaskThreads.Count + 1);
                t.Start();
            }
            Thread th = new Thread(ManageImportantTaskThread, 10000);
            th.IsBackground = true;
            th.Name = "Task Thread " + (TaskThreads.Count + 1);
            th.Start();
        }
        /// <summary>
        /// Runs the task on a new thread.
        /// </summary>
        /// <param name="t">The Task to run.</param>
        public static void AddAsyncTask(Task t)
        {
            TaskQueue.Enqueue(t);
        }
        /// <summary>
        /// Runs a task on a new thread.  This method will not run the the task on a background thread.
        /// </summary>
        /// <param name="t"></param>
        public static void AddAsyncImportantTask(Task t)
        {

        }
        /// <summary>
        /// Runs a task on a new thread after a spefifyed amount of time.
        /// </summary>
        /// <param name="t">The Task to run</param>
        /// <param name="delayInMs">The time to wait before running the task.  In miliseconds</param>
        public static void AddDelayedAsyncTask(Task t, int delayInMs)
        {
            AddAsyncTask(delegate
            {
                Thread.Sleep(delayInMs);
                PerformTask(t);
            });
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
        /// <summary>
        /// Manage the current thread as a task thread.
        /// </summary>
        private static void ManageTaskThread()
        {
            TaskThreads.Add(Thread.CurrentThread);
            int tLoopId = TaskThreads.Count;
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
                catch { } //Queue is most likly empty, no real need to do anything.
                Thread.Sleep(250);
            }
        }
        private static void ManageImportantTaskThread()
        {
            TaskThreads.Add(Thread.CurrentThread);
            int tLoopId = TaskThreads.Count;
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
                catch { } //Queue is most likly empty, no real need to do anything.
                Thread.Sleep(250);
            }
        }
    }
}
