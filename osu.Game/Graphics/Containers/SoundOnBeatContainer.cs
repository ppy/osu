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

        protected const int BARS_PER_SEGMENT = 4;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            int beatsPerBar = (int)timingPoint.TimeSignature;
            int segmentLength = beatsPerBar * Divisor * BARS_PER_SEGMENT;

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

        /// <summary>
        /// The function executed on every beat of the track
        /// </summary>
        /// <param name="beatIndex"> The index of this beat in the current segment(<see cref="BARS_PER_SEGMENT"/> bars) </param>
        /// <param name="signature">The <see cref="TimeSignatures"/> of the current track</param>
        protected abstract void PlayOnBeat(int beatIndex, TimeSignatures signature);
    }
}
