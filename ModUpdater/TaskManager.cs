using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ModUpdater
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
                Console.WriteLine("Running Task Id: " + tid.ToString());
                if (t != null)
                {
                    t.Invoke();
                    Console.WriteLine("Task " + tid.ToString() + " Done");
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
