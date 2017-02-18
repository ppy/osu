// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Mods
{
    public class ModSelectOverlay : WaveOverlayContainer
    {
        private readonly int button_duration = 800; // 1000
        private readonly int ranked_multiplier_duration = 800; // 1000
        private readonly float content_width = 0.8f;

        private Color4 low_multiplier_colour;
        private Color4 high_multiplier_colour;

        private OsuSpriteText rankedLabel, multiplierLabel;
        private FlowContainer rankedMultiplerContainer;

        private FlowContainer modSectionsContainer;

        private DifficultyReductionSection difficultyReductionSection;
        private DifficultyIncreaseSection difficultyIncreaseSection;
        private AssistedSection assistedSection;
        private ModSection[] sections
        {
            get
            {
                return new ModSection[] { difficultyReductionSection, difficultyIncreaseSection, assistedSection };
            }
        }

        public Bindable<Mod[]> SelectedMods = new Bindable<Mod[]>();

        private PlayMode modMode;
        public PlayMode ModMode
        {
            get
            {
                return modMode;
            }
            set
            {
                if (value == modMode) return;
                modMode = value;

                modSectionsContainer.Children = new Drawable[]
                {
                    difficultyReductionSection = new DifficultyReductionSection
                    {
                        RelativeSizeAxes = Axes.X,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Action = modButtonPressed,
                    },
                    difficultyIncreaseSection = new DifficultyIncreaseSection
                    {
                        RelativeSizeAxes = Axes.X,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Action = modButtonPressed,
                    },
                    assistedSection = new AssistedSection(value)
                    {
                        RelativeSizeAxes = Axes.X,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Action = modButtonPressed,
                    },
                };
            }
        }

        protected override void TransitionIn()
        {
            rankedMultiplerContainer.MoveToX(0, ranked_multiplier_duration, EasingTypes.OutQuint);
            rankedMultiplerContainer.FadeIn(ranked_multiplier_duration, EasingTypes.OutQuint);

            foreach (ModSection section in sections)
            {
                section.ButtonsContainer.TransformSpacingTo(new Vector2(50f, 0f), button_duration, EasingTypes.OutQuint);
                section.ButtonsContainer.MoveToX(0, button_duration, EasingTypes.OutQuint);
                section.ButtonsContainer.FadeIn(button_duration, EasingTypes.OutQuint);
            }
        }

        protected override void TransitionOut()
        {
            rankedMultiplerContainer.MoveToX(rankedMultiplerContainer.DrawSize.X, close_duration, EasingTypes.InSine);
            rankedMultiplerContainer.FadeOut(close_duration, EasingTypes.InSine);

            foreach (ModSection section in sections)
            {
                section.ButtonsContainer.TransformSpacingTo(new Vector2(100f, 0f), close_duration, EasingTypes.InSine);
                section.ButtonsContainer.MoveToX(100f, close_duration, EasingTypes.InSine);
                section.ButtonsContainer.FadeTo(0.01f, close_duration, EasingTypes.InSine); // TODO: Fix this so 0.01 opacity isn't used
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            low_multiplier_colour = colours.Red;
            high_multiplier_colour = colours.Green;
        }

        private void modButtonPressed(Mod[] sectionSelectedMods)
        {
            // 
            // Inverse mod deselection
            // 
            // Hard Rock is the inverse of Easy
            // Sudden Death / Perfect is the inverse of No Fail, Relax, AutoPilot, and Auto
            // Double Time is the inverse of Half Time
            // 
            // TODO: Probably make a better way for inverse mod handling
            // 

            foreach (Mod sectionMod in sectionSelectedMods)
            {
                if (sectionMod.Name == Modes.Mods.HardRock)
                {
                    difficultyReductionSection.EasyButton?.Deselect();
                }
                else if (sectionMod.Name == Modes.Mods.Easy)
                {
                    difficultyIncreaseSection.HardRockButton?.Deselect();
                }

                if (sectionMod.Name == Modes.Mods.SuddenDeath || sectionMod.Name == Modes.Mods.Perfect)
                {
                    difficultyReductionSection.NoFailButton?.Deselect();
                    assistedSection.RelaxButton?.Deselect();
                    assistedSection.AutopilotButton?.Deselect();
                    assistedSection.AutoplayCinemaButton?.Deselect();
                }
                else if (sectionMod.Name == Modes.Mods.NoFail || sectionMod.Name == Modes.Mods.Relax || sectionMod.Name == Modes.Mods.Autopilot || sectionMod.Name == Modes.Mods.Autoplay || sectionMod.Name == Modes.Mods.Cinema)
                {
                    difficultyIncreaseSection.SuddenDeathButton?.Deselect();
                }

                if (sectionMod.Name == Modes.Mods.DoubleTime || sectionMod.Name == Modes.Mods.Nightcore)
                {
                    difficultyReductionSection.HalfTimeButton?.Deselect();
                }
                else if (sectionMod.Name == Modes.Mods.HalfTime)
                {
                    difficultyIncreaseSection.DoubleTimeNightcoreButton?.Deselect();
                }
            }

            refreshSelectedMods();

            double multiplier = 1;
            bool ranked = true;

            foreach (Mod mod in SelectedMods.Value)
            {
                multiplier *= mod.ScoreMultiplier(ModMode);

                if (ranked)
                {
                    ranked = mod.Ranked(ModMode);
                }
            }

            // 1.00x
            // 1.05x
            // 1.20x
            multiplierLabel.Text = string.Format("{0:N2}x", multiplier);
            rankedLabel.Text = $"{ranked ? @"Ranked" : @"Unranked"}, Score Multiplier: ";
            if (multiplier > 1.0)
            {
                multiplierLabel.FadeColour(high_multiplier_colour, 200);
            }
            else if (multiplier < 1.0)
            {
                multiplierLabel.FadeColour(low_multiplier_colour, 200);
            }
            else
            {
                multiplierLabel.FadeColour(Color4.White, 200);
            }
        }

        private void refreshSelectedMods()
        {
            List<Mod> selectedMods = new List<Mod>();

            foreach (Mod mod in difficultyReductionSection.SelectedMods)
            {
                selectedMods.Add(mod);
            }

            foreach (Mod mod in difficultyIncreaseSection.SelectedMods)
            {
                selectedMods.Add(mod);
            }

            foreach (Mod mod in assistedSection.SelectedMods)
            {
                selectedMods.Add(mod);
            }

            SelectedMods.Value = selectedMods.ToArray();
        }

        public ModSelectOverlay()
        {
            FirstWaveColour = OsuColour.FromHex(@"19b0e2");
            SecondWaveColour = OsuColour.FromHex(@"2280a2");
            ThirdWaveColour = OsuColour.FromHex(@"005774");
            FourthWaveColour = OsuColour.FromHex(@"003a4e");

            Height = 548; // TODO: Remove when autosize works
            //AutoSizeAxes = Axes.Y;
            //Content.RelativeSizeAxes = Axes.X;
            //Content.AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(36, 50, 68, 255)
                        },
                        new Triangles
                        {
                            TriangleScale = 5,
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = new Color4(53, 66, 82, 255),
                            ColourDark = new Color4(41, 54, 70, 255),
                        },
                    },
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Direction = FlowDirections.Vertical,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        // Header
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 90,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.Gray(10).Opacity(100),
                                },
                                new FlowContainer
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FlowDirections.Vertical,
                                    Width = content_width,
                                    Padding = new MarginPadding
                                    {
                                        Top = 10,
                                        Bottom = 10,
                                    },
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Font = @"Exo2.0-Bold",
                                            Text = @"Gameplay Mods",
                                            TextSize = 22,
                                            Shadow = true,
                                            Margin = new MarginPadding
                                            {
                                                Bottom = 4,
                                            },
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = @"Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play.",
                                            TextSize = 18,
                                            Shadow = true,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = @"Others are just for fun",
                                            TextSize = 18,
                                            Shadow = true,
                                        },
                                    },
                                },
                            },
                        },
                        // Body
                        modSectionsContainer = new FlowContainer
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(0f, 10f),
                            Width = content_width,
                            Children = new Drawable[]
                            {
                                difficultyReductionSection = new DifficultyReductionSection
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Action = modButtonPressed,
                                },
                                difficultyIncreaseSection = new DifficultyIncreaseSection
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Action = modButtonPressed,
                                },
                                assistedSection = new AssistedSection(PlayMode.Osu)
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Action = modButtonPressed,
                                },
                            },
                        },
                        // Footer
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 70,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(172, 20, 116, 255),
                                    Alpha = 0.5f,
                                },
                                rankedMultiplerContainer = new FlowContainer
                                {
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.BottomCentre,
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                    Width = content_width,
                                    Direction = FlowDirections.Horizontal,
                                    Padding = new MarginPadding
                                    {
                                        Top = 20,
                                        Bottom = 20,
                                    },
                                    Children = new Drawable[]
                                    {
                                        rankedLabel = new OsuSpriteText
                                        {
                                            Text = @"Ranked, Score Multiplier: ",
                                            TextSize = 30,
                                            Shadow = true,
                                        },
                                        multiplierLabel = new OsuSpriteText
                                        {
                                            Font = @"Exo2.0-Bold",
                                            Text = @"1.00x",
                                            TextSize = 30,
                                            Shadow = true,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }
    }
}
