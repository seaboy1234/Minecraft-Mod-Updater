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
        private static List<TaskThread> TaskThreads;
        private static object taskLock;
        private static Queue<Task> TaskQueue;
        private static Queue<Task> ImportantTaskQueue;
        private static Dictionary<Task, int> DelayedTasks;

        static TaskManager()
        {
            TaskThreads = new List<TaskThread>(16);
            TaskQueue = new Queue<Task>();
            ImportantTaskQueue = new Queue<Task>();
            DelayedTasks = new Dictionary<Task,int>();
            taskLock = new object();
            for (int i = 0; i < 3; i++)
            {
                SpawnTaskThread(ThreadRole.Standard);
            }
            Thread.Sleep(1);
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
            if (GetTaskThread(r).Thread == null)
            {
                SpawnTaskThread(r);
            }
        }
        private static void PerformTask(Task t)
        {
            CurrentTaskId++;
            int tid = CurrentTaskId;
            try
            {
                Thread.BeginCriticalRegion(); // It is important that this task not be stopped here
                MinecraftModUpdater.Logger.Log(Logger.Level.Debug, "Running Task Id: " + tid.ToString());
                if (t != null)
                {
                    t.Invoke();
                    MinecraftModUpdater.Logger.Log(Logger.Level.Debug, "Task " + tid.ToString() + " Done");
                }
            }
            catch (Exception e)
            {
                MinecraftModUpdater.Logger.Log(Logger.Level.Error, "Error on task " + tid.ToString());
                MinecraftModUpdater.Logger.Log(e);
                ExceptionRaised.Invoke(e);
            }
            finally
            {
                Thread.EndCriticalRegion(); // Thread can now be stopped.
            }
        }
        #region Misic Functions
        /// <summary>
        /// Spawns a new task thread and adds it to the list of threads.
        /// </summary>
        /// <param name="role">The role of the thread.</param>
        public static void SpawnTaskThread(ThreadRole role)
        {
            TaskThread t;
            switch (role) //Set properties.
            {
                case ThreadRole.Delayed:
                    t = TaskThread.SpawnTaskThread(role, ManageTaskDelayThread, TaskThreads.Count + 1, true);
                    break;
                case ThreadRole.Important:
                    t = TaskThread.SpawnTaskThread(role, ManageImportantTaskThread, TaskThreads.Count + 1, false);
                    break;
                default:
                case ThreadRole.Standard:
                    t = TaskThread.SpawnTaskThread(role, ManageTaskThread, TaskThreads.Count + 1, true);
                    break;
            }
            TaskThreads.Add(t);
            t.Start(); //Start the new thread.
        }
        /// <summary>
        /// Stops a task thread assocated with the givin role.
        /// </summary>
        /// <param name="role">The role of the thread to kill.</param>
        /// <returns>Whether the thread was killed or not.</returns>
        public static bool KillTaskThread(ThreadRole role)
        {
            TaskThread t = GetTaskThread(role);
            return KillTaskThread(t);
        }
        public static bool KillTaskThread(TaskThread t)
        {
            if (t == null) return false;
            t.Stop();
            TaskThreads.Remove(t);
            return true;
        }
        /// <summary>
        /// Gets a TaskThread object of the role supplied.
        /// </summary>
        /// <param name="role">The role of the TaskThread.</param>
        /// <returns>The first instance matching the supplied role.</returns>
        public static TaskThread GetTaskThread(ThreadRole role)
        {
            return TaskThreads.Find(new Predicate<TaskThread>(delegate(TaskThread tr)
            {
                if (tr.Role == role)
                    return true;
                return false;
            }));
        }
        public static TaskThread GetTaskThread(Thread thread)
        {
            return TaskThreads.Find(new Predicate<TaskThread>(delegate(TaskThread tr)
            {
                if (tr.Thread == thread)
                    return true;
                return false;
            }));
        }
        public static TaskThread[] GetTaskThreads()
        {
            return TaskThreads.ToArray();
        }
        
        #endregion
        #region Thread Managers
        /// <summary>
        /// Manage the current thread as a task thread.
        /// </summary>
        private static void ManageTaskThread()
        {
            while (true)
            {
                try
                {
                    while (TaskQueue.Peek() == null) Thread.Sleep(250);
                    Task t;
                    lock (taskLock)
                    {
                        t = TaskQueue.Dequeue();
                    }
                    PerformTask(t);
                }
                catch { } //Task invoked by another thread.  No real need to do anything.
                Thread.Sleep(250);
            }
        }
        private static void ManageImportantTaskThread()
        {
            while (GetTaskThread(ThreadRole.Standard) == null) ;
            while (GetTaskThread(ThreadRole.Standard).IsAlive)
            {
                try
                {
                    while (TaskQueue.Peek() == null) Thread.Sleep(250);
                    Task t;
                    lock (taskLock)
                    {
                        t = ImportantTaskQueue.Dequeue();
                    }
                    PerformTask(t);
                }
                catch { } //Queue is most likely empty, no real need to do anything.
                Thread.Sleep(250);
            }
        }
        private static void ManageTaskDelayThread()
        {
            while (GetTaskThread(ThreadRole.Standard) == null) ;
            while (GetTaskThread(ThreadRole.Standard).IsAlive)
            {
                try
                {
                    foreach (var v in DelayedTasks)
                    {
                        DelayedTasks[v.Key] -= 10;
                        if (v.Value < 1)
                        {
                            TaskQueue.Enqueue(v.Key);
                            DelayedTasks.Remove(v.Key);
                        }

                    }
                    Thread.Sleep(10);
                }
                catch { }
            }
        }
        #endregion
        //Used to ensure all threads are exited when the app closes.
        private static void ApplicationExit(object sender, EventArgs e)
        {
            KillTaskThread(ThreadRole.Important);
            KillTaskThread(ThreadRole.Delayed);
            foreach (TaskThread t in TaskThreads.ToArray())
            {
                try
                {
                    KillTaskThread(t);
                }
                catch (Exception ex)
                {
                    MinecraftModUpdater.Logger.Log(ex);
                }
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
