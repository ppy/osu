﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
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

        public override bool HandleKeyboardInput => false;
        public override bool HandleMouseInput => false;

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
            BeginTracking(this, frameworkConfig);
        }

        private readonly Dictionary<(object, IConfigManager), TrackedSettings> trackedConfigManagers = new Dictionary<(object, IConfigManager), TrackedSettings>();

        /// <summary>
        /// Registers a <see cref="ConfigManager{T}"/> to have its settings tracked by this <see cref="OnScreenDisplay"/>.
        /// </summary>
        /// <param name="source">The object that is registering the <see cref="ConfigManager{T}"/> to be tracked.</param>
        /// <param name="configManager">The <see cref="ConfigManager{T}"/> to be tracked.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="configManager"/> is null.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="configManager"/> is already being tracked from the same <paramref name="source"/>.</exception>
        public void BeginTracking(object source, ITrackableConfigManager configManager)
        {
            if (configManager == null)　throw new ArgumentNullException(nameof(configManager));

            if (trackedConfigManagers.ContainsKey((source, configManager)))
                throw new InvalidOperationException($"{nameof(configManager)} is already registered.");

            var trackedSettings = configManager.CreateTrackedSettings();
            if (trackedSettings == null)
                return;

            configManager.LoadInto(trackedSettings);
            trackedSettings.SettingChanged += display;

            trackedConfigManagers.Add((source, configManager), trackedSettings);
        }

        /// <summary>
        /// Unregisters a <see cref="ConfigManager{T}"/> from having its settings tracked by this <see cref="OnScreenDisplay"/>.
        /// </summary>
        /// <param name="source">The object that registered the <see cref="ConfigManager{T}"/> to be tracked.</param>
        /// <param name="configManager">The <see cref="ConfigManager{T}"/> that is being tracked.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="configManager"/> is null.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="configManager"/> is not being tracked from the same <see cref="source"/>.</exception>
        public void StopTracking(object source, ITrackableConfigManager configManager)
        {
            if (configManager == null)　throw new ArgumentNullException(nameof(configManager));

            if (!trackedConfigManagers.TryGetValue((source, configManager), out var existing))
                throw new InvalidOperationException($"{nameof(configManager)} is not registered.");

            existing.Unload();
            existing.SettingChanged -= display;

            trackedConfigManagers.Remove((source, configManager));
        }

        private void display(SettingDescription description)
        {
            Schedule(() =>
            {
                textLine1.Text = description.Name.ToUpper();
                textLine2.Text = description.Value;
                textLine3.Text = description.Shortcut.ToUpper();

                box.Animate(
                    b => b.FadeIn(500, Easing.OutQuint),
                    b => b.ResizeHeightTo(height, 500, Easing.OutQuint)
                ).Then(
                    b => b.FadeOutFromOne(1500, Easing.InQuint),
                    b => b.ResizeHeightTo(height_contracted, 1500, Easing.InQuint)
                );

                int optionCount = 0;
                int selectedOption = -1;

                if (description.RawValue is bool)
                {
                    optionCount = 1;
                    if ((bool)description.RawValue) selectedOption = 0;
                }
                else if (description.RawValue is Enum)
                {
                    var values = Enum.GetValues(description.RawValue.GetType());
                    optionCount = values.Length;
                    selectedOption = Convert.ToInt32(description.RawValue);
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
