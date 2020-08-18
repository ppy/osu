// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNightcore : ModDoubleTime
    {
        public override string Name => "Nightcore";
        public override string Acronym => "NC";
        public override IconUsage? Icon => OsuIcon.ModNightcore;
        public override string Description => "Uguuuuuuuu...";

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModMetronome)).ToArray();

        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);

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
    }

    public abstract class ModNightcore<TObject> : ModNightcore, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new NightcoreBeatContainer());
        }

        public class NightcoreBeatContainer : SoundOnBeatContainer
        {
            private SkinnableSound hatSample;
            private SkinnableSound clapSample;
            private SkinnableSound kickSample;
            private SkinnableSound finishSample;

            public NightcoreBeatContainer()
            {
                Divisor = 2;
            }

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

            protected override void PlayOnBeat(int beatIndex, TimeSignatures signature)
            {
                if (beatIndex == 0)
                    finishSample?.Play();

                switch (signature)
                {
                    case TimeSignatures.SimpleTriple:
                        switch (beatIndex % 6)
                        {
                            case 0:
                                kickSample?.Play();
                                break;

                            case 3:
                                clapSample?.Play();
                                break;

                            default:
                                hatSample?.Play();
                                break;
                        }

                        break;

                    case TimeSignatures.SimpleQuadruple:
                        switch (beatIndex % 4)
                        {
                            case 0:
                                kickSample?.Play();
                                break;

                            case 2:
                                clapSample?.Play();
                                break;

                            default:
                                hatSample?.Play();
                                break;
                        }

                        break;
                }
            }
        }
    }
}
