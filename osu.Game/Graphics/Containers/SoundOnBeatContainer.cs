// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Graphics.Containers
{
    public abstract class SoundOnBeatContainer : BeatSyncedContainer
    {
        private int? firstBeat;

        private const int bars_per_segment = 4;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            int beatsPerBar = (int)timingPoint.TimeSignature;
            int segmentLength = beatsPerBar * Divisor * bars_per_segment;

            if (!IsBeatSyncedWithTrack)
            {
                firstBeat = null;
                return;
            }

            if (!firstBeat.HasValue || beatIndex < firstBeat)
                // decide on a good starting beat index if once has not yet been decided.
                firstBeat = beatIndex < 0 ? 0 : (beatIndex / segmentLength + 1) * segmentLength;

            if (beatIndex >= firstBeat)
                PlayOnBeat(beatIndex % segmentLength, timingPoint.TimeSignature);
        }

        //This function will be executed on every beat of the track
        protected virtual void PlayOnBeat(int beatIndex, TimeSignatures signature)
        {
        }
    }
}
