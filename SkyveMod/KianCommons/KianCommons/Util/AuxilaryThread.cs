namespace KianCommons {
    using System.Collections.Generic;
    using System.Threading;
    using System;

    internal static class AuxiluryThread {
        static AuxiluryThread() {
            Start();
        }

        static Thread thread_;
        private static Queue<Action> tasks_ = new Queue<Action>(1023);

        public static void EnqueueAction(Action act) {
            lock (tasks_)
                tasks_.Enqueue(act);
        }
        private static Action DequeueAction() {
            lock (tasks_) {
                if (tasks_.Count == 0)
                    return null;
                return tasks_.Dequeue();
            }
        }

        public static void Start() {
            thread_ = new Thread(ThreadTask);
            thread_.IsBackground = true;
            thread_.Name = typeof(AuxiluryThread).Assembly.Name();
            thread_.Priority = ThreadPriority.Lowest;
            thread_.Start();
        }

        public static void End() {
            thread_.Abort();
        }

        static void ThreadTask() {
            while (true) {
                try {
                    Action act = DequeueAction();
                    if (act != null) {
                        act();
                    } else {
                        Thread.Sleep(100);
                    }
                } catch (Exception ex) {
                    Log.Exception(ex);
                }
            }
        }
    }
}
