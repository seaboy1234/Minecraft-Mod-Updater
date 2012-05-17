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
