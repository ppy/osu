// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osu.Game.Configuration;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps.Timing;
using osu.Framework.Graphics;
using System.Linq;
using System;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMetronome : Mod
    {
        public override string Name => "Metronome";
        public override string Acronym => "MT";
        public override IconUsage? Icon => FontAwesome.Solid.TachometerAlt;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "Add a metronome";
        public override bool Ranked => false;
        public override double ScoreMultiplier => 0.0;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModNightcore)).ToArray();

        public enum TickFrequency
        {
            One = 1,
            Two = 2,
            Four = 4
        }

        [SettingSource("Tick frequency", "Number of metronome ticks per beat")]
        public Bindable<TickFrequency> Frequency { get; } = new Bindable<TickFrequency>(TickFrequency.One);

        [SettingSource("Different Sound on the first beat of a bar")]
        public Bindable<bool> SpecialSampleForFirstBeatOfBar { get; } = new Bindable<bool>(true);
    }

    public abstract class ModMetronome<TObject> : ModMetronome, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new MetronomeBeatContainer(Frequency, SpecialSampleForFirstBeatOfBar));
        }

        public class MetronomeBeatContainer : SoundOnBeatContainer
        {
            private SkinnableSound firstBeatOfBarSample;
            private SkinnableSound otherBeatOfBarSample;

            private bool specialSampleForFirstBeatOfBar;

            public MetronomeBeatContainer(Bindable<TickFrequency> tickFrequency, Bindable<bool> specialSampleForFirstBeatOfBar)
            {
                tickFrequency.BindValueChanged(val =>
                {
                    Divisor = (int)tickFrequency.Value;
                }, true);

                specialSampleForFirstBeatOfBar.BindValueChanged(val =>
                {
                    this.specialSampleForFirstBeatOfBar = specialSampleForFirstBeatOfBar.Value;
                }, true);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    // temporary samples
                    firstBeatOfBarSample = new SkinnableSound(new SampleInfo("nightcore-hat"))
                    {
                        Frequency = { Value = 2.0f }
                    },
                    otherBeatOfBarSample = new SkinnableSound(new SampleInfo("nightcore-hat")),
                };
            }

            protected override void PlayOnBeat(int beatIndex, TimeSignatures signature)
            {
                if ((beatIndex % ((int)signature * Divisor)) == 0 && specialSampleForFirstBeatOfBar)
                    firstBeatOfBarSample.Play();
                else
                    otherBeatOfBarSample.Play();
            }
        }
    }
}
