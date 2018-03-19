using osu.Game.Graphics.Containers;
using System;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.ControlPoints;
using OpenTK;
using osu.Framework.Graphics;
using OpenTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Screens.OsuTalk
{
    public class OsuTalkBackground : BeatSyncedContainer
    {
        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            Scale = new Vector2(1);
            using (BeginDelayedSequence(100))
                this.ScaleTo(1.005f , Math.Max(0, timingPoint.BeatLength - 100) , Easing.OutQuad);
        }
    }
}
