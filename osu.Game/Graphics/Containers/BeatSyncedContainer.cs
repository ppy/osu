// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Graphics.Containers
{
    public class BeatSyncedContainer : Container
    {
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private int lastBeat;
        private TimingControlPoint lastTimingPoint;

        protected override void Update()
        {
            if (beatmap.Value?.Track == null)
                return;

            double currentTrackTime = beatmap.Value.Track.CurrentTime;

            TimingControlPoint timingPoint = beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(currentTrackTime);
            EffectControlPoint effectPoint = beatmap.Value.Beatmap.ControlPointInfo.EffectPointAt(currentTrackTime);

            if (timingPoint.BeatLength == 0)
                return;

            int beatIndex = (int)((currentTrackTime - timingPoint.Time) / timingPoint.BeatLength);

            // The beats before the start of the first control point are off by 1, this should do the trick
            if (currentTrackTime < timingPoint.Time)
                beatIndex--;

            if (timingPoint == lastTimingPoint && beatIndex == lastBeat)
                return;

            double offsetFromBeat = (timingPoint.Time - currentTrackTime) % timingPoint.BeatLength;

            using (BeginDelayedSequence(offsetFromBeat, true))
                OnNewBeat(beatIndex, timingPoint, effectPoint, beatmap.Value.Track.CurrentAmplitudes);

            lastBeat = beatIndex;
            lastTimingPoint = timingPoint;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap.BindTo(game.Beatmap);
        }

        protected virtual void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
        }
    }
}
