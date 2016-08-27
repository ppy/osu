//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Timing;

namespace osu.Framework.Audio.Track
{
    class AudioTrackVirtual : AudioTrack
    {
        StopwatchClock clock = new StopwatchClock();

        double seekOffset;

        public override bool Seek(double seek)
        {
            double current = CurrentTime;

            seekOffset = seek;
            clock.Restart();

            if (Length > 0 && seekOffset > Length)
                seekOffset = Length;

            return current != seekOffset;
        }

        public override void Start()
        {
            clock.Start();
        }

        public override void Reset()
        {
            clock.Reset();
            seekOffset = 0;

            base.Reset();
        }

        public override void Stop()
        {
            clock.Stop();
        }

        public override bool IsRunning => clock.IsRunning;

        public override double CurrentTime => seekOffset + clock.CurrentTime;

        public override void Update()
        {
            if (Length > 0 && CurrentTime >= Length)
                Stop();
        }
    }
}
