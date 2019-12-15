// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNightcore<TObject> : ModDoubleTime, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        public override string Name => "Nightcore";
        public override string Acronym => "NC";
        public override IconUsage Icon => OsuIcon.ModNightcore;
        public override string Description => "Uguuuuuuuu...";

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        protected ModNightcore()
        {
            SpeedChange.BindValueChanged(val =>
            {
                freqAdjust.Value = SpeedChange.Default;
                tempoAdjust.Value = val.NewValue / SpeedChange.Default;
            }, true);
        }

        public override void ApplyToTrack(Track track)
        {
            // base.ApplyToTrack() intentionally not called (different tempo adjustment is applied)
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new NightcoreBeatContainer());
        }

        public class NightcoreBeatContainer : BeatSyncedContainer
        {
            private SkinnableSound hatSample;
            private SkinnableSound clapSample;
            private SkinnableSound kickSample;
            private SkinnableSound finishSample;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    hatSample = new SkinnableSound(new SampleInfo("nightcore-hat")),
                    clapSample = new SkinnableSound(new SampleInfo("nightcore-clap")),
                    kickSample = new SkinnableSound(new SampleInfo("nightcore-kick")),
                    finishSample = new SkinnableSound(new SampleInfo("nightcore-finish")),
                };
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                if (beatIndex > -1)
                {
                    if (beatIndex % 16 == 0)
                        finishSample?.Play();
                    else if (beatIndex % 2 == 0)
                        kickSample?.Play();
                    else if (beatIndex % 2 == 1)
                        clapSample?.Play();
                    else
                        hatSample?.Play();
                }
            }
        }
    }
}
