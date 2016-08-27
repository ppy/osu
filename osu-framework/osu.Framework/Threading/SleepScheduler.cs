//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Threading;

namespace osu.Framework.Threading
{
    public class SleepScheduler : Threading.Scheduler
    {
        private SleepHandle sleeper;
        public SleepScheduler(SleepHandle sleeper)
            : base()
        {
            this.sleeper = sleeper;
        }
        public override bool Add(VoidDelegate d, bool forceDelayed = false)
        {
            if (!sleeper.IsSleeping || isMainThread)
            {
                base.Add(d, forceDelayed);
                return true;
            }
            else
                ThreadPool.QueueUserWorkItem(State =>
                {
                    if (sleeper.IsSleeping)
                        sleeper.Invoke(new VoidDelegate(d));
                    else
                        Add(d, forceDelayed);
                });

            return false;
        }
    }
}
