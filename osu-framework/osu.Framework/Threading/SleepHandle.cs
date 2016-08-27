//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Threading;

namespace osu.Framework.Threading
{
    public class SleepHandle
    {
        private AutoResetEvent sleepTimeOut;
        private AutoResetEvent taskDone;
        private object locker = new object();
        private VoidDelegate task;
        private bool cleanLater;
        internal bool IsSleeping{ get; private set;}

        public SleepHandle()
        {
            sleepTimeOut = new AutoResetEvent(false);
            taskDone = new AutoResetEvent(false);
        }

        //sleep until we get disrupted and have a task
        public void Sleep(int timeMS)
        {
            IsSleeping = true;
            //we use datetime as it's a lot faster than the stopwatch and we don't need the accuracy.
            DateTime before = DateTime.Now;
            do
            {
                if (cleanLater)
                {
                    taskDone.Set();
                    cleanLater = false;
                }
                sleepTimeOut.WaitOne(timeMS);
                if (task != null)
                    executeTask(false);

            } while ((DateTime.Now - before).TotalMilliseconds < timeMS);
            IsSleeping = false;
            // in case task was trying to be inoked right after a an other task got executed
            if (task != null)
                executeTask(true);
        }

        private void executeTask(bool cleanDirectly)
        {
            task.Invoke();
            task = null;
            if (cleanDirectly)
                taskDone.Set();
            else
                cleanLater = true;
        }

        public void Invoke(VoidDelegate task)
        {
            lock (locker)
            {
                if (this.task!=null)
                    throw new Exception();
                this.task = task;
                //disrupt time handle
                sleepTimeOut.Set();
                taskDone.WaitOne();
                
            }
        }

    }

}
