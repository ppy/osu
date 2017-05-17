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
        private int beat;
        private double timingPointStart;
        //This is to avoid sending new beats when not at the very start of the beat
        private const int seek_tolerance = 20;
        private const double min_beat_length = 1E-100;

        protected override void Update()
        {
            if (beatmap.Value != null)
            {
                double currentTime = beatmap.Value.Track.CurrentTime;
                ControlPoint kiaiControlPoint;
                ControlPoint controlPoint = beatmap.Value.Beatmap.TimingInfo.TimingPointAt(currentTime, out kiaiControlPoint);

                if (controlPoint != null)
                {
                    double oldTimingPointStart = timingPointStart;
                    double beatLength = controlPoint.BeatLength;
                    int oldBeat = beat;
                    bool kiai = kiaiControlPoint?.KiaiMode ?? false;
                    timingPointStart = controlPoint.Time;

                    beat = beatLength > min_beat_length ? (int)((currentTime - timingPointStart) / beatLength) : 0;

                    //should we handle negative beats? (before the start of the controlPoint)
                    //The beats before the start of the first control point are off by 1, this should do the trick
                    if (currentTime <= timingPointStart)
                        beat--;

                    if ((timingPointStart != oldTimingPointStart || beat != oldBeat) && (int)((currentTime - timingPointStart) % beatLength) <= seek_tolerance)
                        OnNewBeat(beat, beatLength, controlPoint.TimeSignature, kiai);
                }
            }
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