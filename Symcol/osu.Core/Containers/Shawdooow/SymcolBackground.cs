using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using OpenTK;

namespace osu.Core.Containers.Shawdooow
{
    public class SymcolBackground : BeatSyncedContainer
    {
        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            Scale = new Vector2(1);
            using (BeginDelayedSequence(100))
                this.ScaleTo(1.0025f , Math.Max(0, timingPoint.BeatLength - 100) , Easing.OutQuad);
        }
    }
}
