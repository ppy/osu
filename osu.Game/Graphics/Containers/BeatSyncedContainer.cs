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
        /// <summary>
        /// A new beat will not be sent if the time since the beat is larger than this tolerance.
        /// </summary>
        private const int seek_tolerance = 20;

        private Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private int lastBeat;
        private ControlPoint lastControlPoint;

        protected override void Update()
        {
            if (beatmap.Value?.Track == null)
                return;

            double currentTrackTime = beatmap.Value.Track.CurrentTime;
            ControlPoint overridePoint;
            ControlPoint controlPoint = beatmap.Value.Beatmap.TimingInfo.TimingPointAt(currentTrackTime, out overridePoint);

            bool kiai = (overridePoint ?? controlPoint).KiaiMode;
            int beat = controlPoint.BeatLength > 0 ? (int)((currentTrackTime - controlPoint.Time) / controlPoint.BeatLength) : 0;

            // The beats before the start of the first control point are off by 1, this should do the trick
            if (currentTrackTime < controlPoint.Time)
                beat--;

            if (controlPoint == lastControlPoint && beat == lastBeat)
                return;

            if ((currentTrackTime - controlPoint.Time) % controlPoint.BeatLength > seek_tolerance)
                return;

            OnNewBeat(beat, controlPoint.BeatLength, controlPoint.TimeSignature, kiai);

            lastBeat = beat;
            lastControlPoint = controlPoint;
        }

        protected virtual void OnNewBeat(int newBeat, double beatLength, TimeSignatures timeSignature, bool kiai)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap.BindTo(game.Beatmap);
        }
    }
}
