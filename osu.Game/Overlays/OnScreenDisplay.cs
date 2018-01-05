// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays
{
    public class OnScreenDisplay : Container
    {
        private readonly Container box;

        public override bool HandleInput => false;

        private readonly SpriteText textLine1;
        private readonly SpriteText textLine2;
        private readonly SpriteText textLine3;

        private const float height = 110;
        private const float height_contracted = height * 0.9f;

        private readonly FillFlowContainer<OptionLight> optionLights;

        public OnScreenDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                box = new Container
                {
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(0.5f, 0.75f),
                    Masking = true,
                    AutoSizeAxes = Axes.X,
                    Height = height_contracted,
                    Alpha = 0,
                    CornerRadius = 20,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.7f,
                        },
                        new Container // purely to add a minimum width
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 240,
                            RelativeSizeAxes = Axes.Y,
                        },
                        textLine1 = new OsuSpriteText
                        {
                            Padding = new MarginPadding(10),
                            Font = @"Exo2.0-Black",
                            Spacing = new Vector2(1, 0),
                            TextSize = 14,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        textLine2 = new OsuSpriteText
                        {
                            TextSize = 24,
                            Font = @"Exo2.0-Light",
                            Padding = new MarginPadding { Left = 10, Right = 10 },
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                optionLights = new FillFlowContainer<OptionLight>
                                {
                                    Padding = new MarginPadding { Top = 20, Bottom = 5 },
                                    Spacing = new Vector2(5, 0),
                                    Direction = FillDirection.Horizontal,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    AutoSizeAxes = Axes.Both
                                },
                                textLine3 = new OsuSpriteText
                                {
                                    Padding = new MarginPadding { Bottom = 15 },
                                    Font = @"Exo2.0-Bold",
                                    TextSize = 12,
                                    Alpha = 0.3f,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                            }
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            trackSetting(frameworkConfig.GetBindable<FrameSync>(FrameworkSetting.FrameSync), v => display(v, "Frame Limiter", v.GetDescription(), "Ctrl+F7"));
            trackSetting(frameworkConfig.GetBindable<string>(FrameworkSetting.AudioDevice), v => display(v, "Audio Device", string.IsNullOrEmpty(v) ? "Default" : v, v));
            trackSetting(frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowLogOverlay), v => display(v, "Debug Logs", v ? "visible" : "hidden", "Ctrl+F10"));

            Action displayResolution = delegate { display(null, "Screen Resolution", frameworkConfig.Get<int>(FrameworkSetting.Width) + "x" + frameworkConfig.Get<int>(FrameworkSetting.Height)); };

            trackSetting(frameworkConfig.GetBindable<int>(FrameworkSetting.Width), v => displayResolution());
            trackSetting(frameworkConfig.GetBindable<int>(FrameworkSetting.Height), v => displayResolution());

            trackSetting(frameworkConfig.GetBindable<double>(FrameworkSetting.CursorSensitivity), v => display(v, "Cursor Sensitivity", v.ToString(@"0.##x"), "Ctrl+Alt+R to reset"));
            trackSetting(frameworkConfig.GetBindable<string>(FrameworkSetting.ActiveInputHandlers),
                delegate (string v)
                {
                    bool raw = v.Contains("Raw");
                    display(raw, "Raw Input", raw ? "enabled" : "disabled", "Ctrl+Alt+R to reset");
                });

            trackSetting(frameworkConfig.GetBindable<WindowMode>(FrameworkSetting.WindowMode), v => display(v, "Screen Mode", v.ToString(), "Alt+Enter"));
        }

        private readonly List<IBindable> references = new List<IBindable>();

        private void trackSetting<T>(Bindable<T> bindable, Action<T> action)
        {
            // we need to keep references as we bind
            references.Add(bindable);

            bindable.ValueChanged += action;
        }

        private void display(object rawValue, string settingName, string settingValue, string shortcut = @"")
        {
            Schedule(() =>
            {
                textLine1.Text = settingName.ToUpper();
                textLine2.Text = settingValue;
                textLine3.Text = shortcut.ToUpper();

                box.Animate(
                    b => b.FadeIn(500, Easing.OutQuint),
                    b => b.ResizeHeightTo(height, 500, Easing.OutQuint)
                ).Then(
                    b => b.FadeOutFromOne(1500, Easing.InQuint),
                    b => b.ResizeHeightTo(height_contracted, 1500, Easing.InQuint)
                );

                int optionCount = 0;
                int selectedOption = -1;

                if (rawValue is bool)
                {
                    optionCount = 1;
                    if ((bool)rawValue) selectedOption = 0;
                }
                else if (rawValue is Enum)
                {
                    var values = Enum.GetValues(rawValue.GetType());
                    optionCount = values.Length;
                    selectedOption = Convert.ToInt32(rawValue);
                }

                textLine2.Origin = optionCount > 0 ? Anchor.BottomCentre : Anchor.Centre;
                textLine2.Y = optionCount > 0 ? 0 : 5;

                if (optionLights.Children.Count != optionCount)
                {
                    optionLights.Clear();
                    for (int i = 0; i < optionCount; i++)
                        optionLights.Add(new OptionLight());
                }

                for (int i = 0; i < optionCount; i++)
                    optionLights.Children[i].Glowing = i == selectedOption;
            });
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
        }
    }
}
