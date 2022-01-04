// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.OSD
{
    public class TrackedSettingToast : Toast
    {
        private const int lights_bottom_margin = 40;

        private readonly int optionCount;
        private readonly int selectedOption = -1;

        private Sample sampleOn;
        private Sample sampleOff;
        private Sample sampleChange;

        private Bindable<double?> lastPlaybackTime;

        public TrackedSettingToast(SettingDescription description)
            : base(description.Name, description.Value, description.Shortcut)
        {
            FillFlowContainer<OptionLight> optionLights;

            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = lights_bottom_margin },
                    Children = new Drawable[]
                    {
                        optionLights = new FillFlowContainer<OptionLight>
                        {
                            Margin = new MarginPadding { Bottom = 5 },
                            Spacing = new Vector2(5, 0),
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both
                        },
                    }
                }
            };

            switch (description.RawValue)
            {
                case bool val:
                    optionCount = 1;
                    if (val) selectedOption = 0;
                    break;

                case Enum _:
                    var values = Enum.GetValues(description.RawValue.GetType());
                    optionCount = values.Length;
                    selectedOption = Convert.ToInt32(description.RawValue);
                    break;
            }

            ValueText.Origin = optionCount > 0 ? Anchor.BottomCentre : Anchor.Centre;

            for (int i = 0; i < optionCount; i++)
                optionLights.Add(new OptionLight { Glowing = i == selectedOption });
        }

        [Resolved]
        private SessionStatics statics { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playSound();
        }

        private void playSound()
        {
            // This debounce code roughly follows what we're using in HoverSampleDebounceComponent.
            // We're sharing the existing static for hover sounds because it doesn't really matter if they block each other.
            // This is a simple solution, but if this ever becomes a problem (or other performance issues arise),
            // the whole toast system should be rewritten to avoid recreating this drawable each time a value changes.
            lastPlaybackTime = statics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime);

            bool enoughTimePassedSinceLastPlayback = !lastPlaybackTime.Value.HasValue || Time.Current - lastPlaybackTime.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;

            if (!enoughTimePassedSinceLastPlayback) return;

            if (optionCount == 1)
            {
                if (selectedOption == 0)
                    sampleOn?.Play();
                else
                    sampleOff?.Play();
            }
            else
            {
                if (sampleChange == null) return;

                sampleChange.Frequency.Value = 1 + (double)selectedOption / (optionCount - 1) * 0.25f;
                sampleChange.Play();
            }

            lastPlaybackTime.Value = Time.Current;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleOn = audio.Samples.Get("UI/osd-on");
            sampleOff = audio.Samples.Get("UI/osd-off");
            sampleChange = audio.Samples.Get("UI/osd-change");
        }

        private class OptionLight : Container
        {
            private Color4 glowingColour, idleColour;

            private const float transition_speed = 300;

            private const float glow_strength = 0.4f;

            private readonly Box fill;

            public OptionLight()
            {
                Children = new[]
                {
                    fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 1,
                    },
                };
            }

            private bool glowing;

            public bool Glowing
            {
                set
                {
                    glowing = value;
                    if (!IsLoaded) return;

                    updateGlow();
                }
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                fill.Colour = idleColour = Color4.White.Opacity(0.4f);
                glowingColour = Color4.White;

                Size = new Vector2(25, 5);

                Masking = true;
                CornerRadius = 3;

                EdgeEffect = new EdgeEffectParameters
                {
                    Colour = colours.BlueDark.Opacity(glow_strength),
                    Type = EdgeEffectType.Glow,
                    Radius = 8,
                };
            }

            protected override void LoadComplete()
            {
                updateGlow();
                FinishTransforms(true);
            }

            private void updateGlow()
            {
                if (glowing)
                {
                    fill.FadeColour(glowingColour, transition_speed, Easing.OutQuint);
                    FadeEdgeEffectTo(glow_strength, transition_speed, Easing.OutQuint);
                }
                else
                {
                    FadeEdgeEffectTo(0, transition_speed, Easing.OutQuint);
                    fill.FadeColour(idleColour, transition_speed, Easing.OutQuint);
                }
            }
        }
    }
}
