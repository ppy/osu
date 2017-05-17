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

        public BeatSyncedContainer()
        {
        }

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
                    double beatLenght = double.MinValue;
                    int oldBeat = beat;
                    bool kiai = false;

                    beatLenght = controlPoint.BeatLength;
                    timingPointStart = controlPoint.Time;
                    kiai = kiaiControlPoint?.KiaiMode ?? false;

                    beat = beatLenght > min_beat_length ? (int)((currentTime - timingPointStart) / beatLenght) : 0;

                    //should we handle negative beats? (before the start of the controlPoint)
                    //The beats before the start of the first control point are off by 1, this should do the trick
                    if (currentTime <= timingPointStart)
                        beat--;

                    if ((timingPointStart != oldTimingPointStart || beat != oldBeat) && (int)((currentTime - timingPointStart) % (beatLenght)) <= seek_tolerance)
                        OnNewBeat(beat, controlPoint.BeatLength, controlPoint.TimeSignature, kiai);
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