// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Graphics.Containers
{
    public class BeatSyncedContainer : Container
    {
        private Bindable<WorkingBeatmap> beatmap;
        private int lastBeat;
        private double lastTimingPointStart;
        //This is to avoid sending new beats when not at the very start of the beat
        private const int seek_tolerance = 20;
        private const double min_beat_length = 1E-100;

        protected override void Update()
        {
            if (beatmap.Value == null)
                return;

            double trackCurrentTime = beatmap.Value.Track.CurrentTime;
            ControlPoint kiaiControlPoint;
            ControlPoint controlPoint = beatmap.Value.Beatmap.TimingInfo.TimingPointAt(trackCurrentTime, out kiaiControlPoint);

            if (controlPoint == null)
                return;

            bool kiai = (controlPoint ?? controlPoint).KiaiMode;

            double beatLength = controlPoint.BeatLength;
            double timingPointStart = controlPoint.Time;
            int beat = beatLength > min_beat_length ? (int)((trackCurrentTime - timingPointStart) / beatLength) : 0;

            //The beats before the start of the first control point are off by 1, this should do the trick
            if (trackCurrentTime < timingPointStart)
                beat--;

            if ((timingPointStart != lastTimingPointStart || beat != lastBeat) && (int)((trackCurrentTime - timingPointStart) % beatLength) <= seek_tolerance)
                OnNewBeat(beat, beatLength, controlPoint.TimeSignature, kiai);
            lastBeat = beat;
            lastTimingPointStart = timingPointStart;
        }

        protected virtual void OnNewBeat(int newBeat, double beatLength, TimeSignatures timeSignature, bool kiai)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap = game.Beatmap;
        }
    }
}
