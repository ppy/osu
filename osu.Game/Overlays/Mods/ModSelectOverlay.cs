// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes;

namespace osu.Game.Overlays.Mods
{
    public class ModSelectOverlay : WaveOverlayContainer
    {
        private const int button_duration = 700;
        private const int ranked_multiplier_duration = 700;
        private const float content_width = 0.8f;

        private Color4 lowMultiplierColour, highMultiplierColour;

        private OsuSpriteText rankedLabel, multiplierLabel;
        private FillFlowContainer rankedMultiplerContainer;

        private FillFlowContainer<ModSection> modSectionsContainer;

        public Bindable<Mod[]> SelectedMods = new Bindable<Mod[]>();

        public readonly Bindable<PlayMode> PlayMode = new Bindable<PlayMode>();

        private void modeChanged(object sender, EventArgs eventArgs)
        {
            var ruleset = Ruleset.GetRuleset(PlayMode);

            modSectionsContainer.Children = new ModSection[]
            {
                new DifficultyReductionSection
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Action = modButtonPressed,
                    Buttons = ruleset.GetModsFor(ModType.DifficultyReduction).Select(m => new ModButton(m)).ToArray(),
                },
                new DifficultyIncreaseSection
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Action = modButtonPressed,
                    Buttons = ruleset.GetModsFor(ModType.DifficultyIncrease).Select(m => new ModButton(m)).ToArray(),
                },
                new AssistedSection
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Action = modButtonPressed,
                    Buttons = ruleset.GetModsFor(ModType.Special).Select(m => new ModButton(m)).ToArray(),
                },
            };
        }

        [BackgroundDependencyLoader(permitNulls:true)]
        private void load(OsuColour colours, OsuGame osu)
        {
            lowMultiplierColour = colours.Red;
            highMultiplierColour = colours.Green;

            if (osu != null)
                PlayMode.BindTo(osu.PlayMode);
            PlayMode.ValueChanged += modeChanged;
            PlayMode.TriggerChange();
        }

        protected override void PopOut()
        {
            base.PopOut();

            rankedMultiplerContainer.MoveToX(rankedMultiplerContainer.DrawSize.X, APPEAR_DURATION, EasingTypes.InSine);
            rankedMultiplerContainer.FadeOut(APPEAR_DURATION, EasingTypes.InSine);

            foreach (ModSection section in modSectionsContainer.Children)
            {
                section.ButtonsContainer.TransformSpacingTo(new Vector2(100f, 0f), APPEAR_DURATION, EasingTypes.InSine);
                section.ButtonsContainer.MoveToX(100f, APPEAR_DURATION, EasingTypes.InSine);
                section.ButtonsContainer.FadeOut(APPEAR_DURATION, EasingTypes.InSine);
            }

            TriggerFocusLost();
        }

        protected override void PopIn()
        {
            base.PopIn();

            rankedMultiplerContainer.MoveToX(0, ranked_multiplier_duration, EasingTypes.OutQuint);
            rankedMultiplerContainer.FadeIn(ranked_multiplier_duration, EasingTypes.OutQuint);

            foreach (ModSection section in modSectionsContainer.Children)
            {
                section.ButtonsContainer.TransformSpacingTo(new Vector2(50f, 0f), button_duration, EasingTypes.OutQuint);
                section.ButtonsContainer.MoveToX(0, button_duration, EasingTypes.OutQuint);
                section.ButtonsContainer.FadeIn(button_duration, EasingTypes.OutQuint);
            }

            Schedule(TriggerFocusContention);
        }

        public void DeselectAll()
        {
            foreach (ModSection section in modSectionsContainer.Children)
            {
                foreach (ModButton button in section.Buttons)
                {
                    button.Deselect();
                }
            }
        }

        public void DeselectMod(Modes.Mods modName)
        {
            foreach (ModSection section in modSectionsContainer.Children)
            {
                foreach (ModButton button in section.Buttons)
                {
                    foreach (Mod mod in button.Mods)
                    {
                        if (mod.Name == modName)
                        {
                            button.Deselect();
                            return;
                        }
                    }
                }
            }
        }

        private void modButtonPressed(Mod selectedMod)
        {
            if (selectedMod != null)
            {
                foreach (Modes.Mods disableMod in selectedMod.DisablesMods)
                {
                    DeselectMod(disableMod);
                }
            }

            refreshSelectedMods();

            double multiplier = 1.0;
            bool ranked = true;

            foreach (Mod mod in SelectedMods.Value)
            {
                multiplier *= mod.ScoreMultiplier;

                if (ranked)
                    ranked = mod.Ranked;
            }

            // 1.00x
            // 1.05x
            // 1.20x

            multiplierLabel.Text = string.Format("{0:N2}x", multiplier);
            string rankedString = ranked ? "Ranked" : "Unranked";
            rankedLabel.Text = $@"{rankedString}, Score Multiplier: ";

            if (multiplier > 1.0)
            {
                multiplierLabel.FadeColour(highMultiplierColour, 200);
            }
            else if (multiplier < 1.0)
            {
                multiplierLabel.FadeColour(lowMultiplierColour, 200);
            }
            else
            {
                multiplierLabel.FadeColour(Color4.White, 200);
            }
        }

        private void refreshSelectedMods()
        {
            List<Mod> selectedMods = new List<Mod>();

            foreach (ModSection section in modSectionsContainer.Children)
            {
                foreach (Mod mod in section.SelectedMods)
                {
                    selectedMods.Add(mod);
                }
            }

            SelectedMods.Value = selectedMods.ToArray();
        }

        public ModSelectOverlay()
        {
            FirstWaveColour = OsuColour.FromHex(@"19b0e2");
            SecondWaveColour = OsuColour.FromHex(@"2280a2");
            ThirdWaveColour = OsuColour.FromHex(@"005774");
            FourthWaveColour = OsuColour.FromHex(@"003a4e");

            Height = 510;
            Content.RelativeSizeAxes = Axes.X;
            Content.AutoSizeAxes = Axes.Y;
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
                            RelativeSizeAxes = Axes.X,
                            Height = Height, //set the height from the start to ensure correct triangle density.
                            ColourLight = new Color4(53, 66, 82, 255),
                            ColourDark = new Color4(41, 54, 70, 255),
                        },
                    },
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Direction = FillDirection.Down,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        // Header
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 82,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.Gray(10).Opacity(100),
                                },
                                new FillFlowContainer
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Down,
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
                        modSectionsContainer = new FillFlowContainer<ModSection>
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(0f, 10f),
                            Width = content_width,
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
                                rankedMultiplerContainer = new FillFlowContainer
                                {
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.BottomCentre,
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                    Width = content_width,
                                    Direction = FillDirection.Right,
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
