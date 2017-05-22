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
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private int lastBeat;
        private ControlPoint lastControlPoint;

        protected override void Update()
        {
            if (beatmap.Value?.Track == null)
                return;

            double currentTrackTime = beatmap.Value.Track.CurrentTime;
            ControlPoint overridePoint;
            ControlPoint controlPoint = beatmap.Value.Beatmap.TimingInfo.TimingPointAt(currentTrackTime, out overridePoint);

            if (controlPoint.BeatLength == 0)
                return;

            bool kiai = (overridePoint ?? controlPoint).KiaiMode;
            int beat = (int)((currentTrackTime - controlPoint.Time) / controlPoint.BeatLength);

            // The beats before the start of the first control point are off by 1, this should do the trick
            if (currentTrackTime < controlPoint.Time)
                beat--;

            if (controlPoint == lastControlPoint && beat == lastBeat)
                return;

            double offsetFromBeat = (controlPoint.Time - currentTrackTime) % controlPoint.BeatLength;

            using (BeginDelayedSequence(offsetFromBeat, true))
                OnNewBeat(beat, controlPoint.BeatLength, controlPoint.TimeSignature, kiai);

            lastBeat = beat;
            lastControlPoint = controlPoint;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap.BindTo(game.Beatmap);
        }

        protected virtual void OnNewBeat(int newBeat, double beatLength, TimeSignatures timeSignature, bool kiai)
        {
        }
    }
}
