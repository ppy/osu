// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods.Sections;
using osu.Game.Screens;

namespace osu.Game.Overlays.Mods
{
    public class ModSelectOverlay : WaveOverlayContainer
    {
        private const float content_width = 0.8f;

        protected Color4 LowMultiplierColour, HighMultiplierColour;

        protected readonly TriangleButton DeselectAllButton;
        protected readonly OsuSpriteText MultiplierLabel, UnrankedLabel;
        private readonly FillFlowContainer footerContainer;

        protected override bool BlockNonPositionalInput => false;

        protected override bool DimMainContent => false;

        protected readonly FillFlowContainer<ModSection> ModSectionsContainer;

        protected readonly Bindable<IEnumerable<Mod>> SelectedMods = new Bindable<IEnumerable<Mod>>(new Mod[] { });

        protected readonly IBindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, IBindable<RulesetInfo> ruleset, AudioManager audio, Bindable<IEnumerable<Mod>> selectedMods)
        {
            LowMultiplierColour = colours.Red;
            HighMultiplierColour = colours.Green;
            UnrankedLabel.Colour = colours.Blue;

            Ruleset.BindTo(ruleset);
            if (selectedMods != null) SelectedMods.BindTo(selectedMods);

            sampleOn = audio.Sample.Get(@"UI/check-on");
            sampleOff = audio.Sample.Get(@"UI/check-off");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Ruleset.BindValueChanged(rulesetChanged, true);
            SelectedMods.BindValueChanged(selectedModsChanged, true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Ruleset.UnbindAll();
            SelectedMods.UnbindAll();
        }

        private void rulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            if (e.NewValue == null) return;

            var instance = e.NewValue.CreateInstance();

            foreach (ModSection section in ModSectionsContainer.Children)
                section.Mods = instance.GetModsFor(section.ModType);

            // attempt to re-select any already selected mods.
            // this may be the first time we are receiving the ruleset, in which case they will still match.
            selectedModsChanged(new ValueChangedEvent<IEnumerable<Mod>>(SelectedMods.Value, SelectedMods.Value));

            // write the mods back to the SelectedMods bindable in the case a change was not applicable.
            // this generally isn't required as the previous line will perform deselection; just here for safety.
            refreshSelectedMods();
        }

        private void selectedModsChanged(ValueChangedEvent<IEnumerable<Mod>> e)
        {
            foreach (ModSection section in ModSectionsContainer.Children)
                section.SelectTypes(e.NewValue.Select(m => m.GetType()).ToList());

            updateMods();
        }

        private void updateMods()
        {
            double multiplier = 1.0;
            bool ranked = true;

            foreach (Mod mod in SelectedMods.Value)
            {
                multiplier *= mod.ScoreMultiplier;
                ranked &= mod.Ranked;
            }

            MultiplierLabel.Text = $"{multiplier:N2}x";
            if (multiplier > 1.0)
                MultiplierLabel.FadeColour(HighMultiplierColour, 200);
            else if (multiplier < 1.0)
                MultiplierLabel.FadeColour(LowMultiplierColour, 200);
            else
                MultiplierLabel.FadeColour(Color4.White, 200);

            UnrankedLabel.FadeTo(ranked ? 0 : 1, 200);
        }

        protected override void PopOut()
        {
            base.PopOut();

            footerContainer.MoveToX(footerContainer.DrawSize.X, WaveContainer.DISAPPEAR_DURATION, Easing.InSine);
            footerContainer.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.InSine);

            foreach (ModSection section in ModSectionsContainer.Children)
            {
                section.ButtonsContainer.TransformSpacingTo(new Vector2(100f, 0f), WaveContainer.DISAPPEAR_DURATION, Easing.InSine);
                section.ButtonsContainer.MoveToX(100f, WaveContainer.DISAPPEAR_DURATION, Easing.InSine);
                section.ButtonsContainer.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.InSine);
            }
        }

        protected override void PopIn()
        {
            base.PopIn();

            footerContainer.MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            footerContainer.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);

            foreach (ModSection section in ModSectionsContainer.Children)
            {
                section.ButtonsContainer.TransformSpacingTo(new Vector2(50f, 0f), WaveContainer.APPEAR_DURATION, Easing.OutQuint);
                section.ButtonsContainer.MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
                section.ButtonsContainer.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            }
        }

        public void DeselectAll()
        {
            foreach (ModSection section in ModSectionsContainer.Children)
                section.DeselectAll();

            refreshSelectedMods();
        }

        /// <summary>
        /// Deselect one or more mods.
        /// </summary>
        /// <param name="modTypes">The types of <see cref="Mod"/>s which should be deselected.</param>
        /// <param name="immediate">Set to true to bypass animations and update selections immediately.</param>
        public void DeselectTypes(Type[] modTypes, bool immediate = false)
        {
            if (modTypes.Length == 0) return;

            foreach (ModSection section in ModSectionsContainer.Children)
                section.DeselectTypes(modTypes, immediate);
        }

        private SampleChannel sampleOn, sampleOff;

        private void modButtonPressed(Mod selectedMod)
        {
            if (selectedMod != null)
            {
                if (State == Visibility.Visible) sampleOn?.Play();
                DeselectTypes(selectedMod.IncompatibleMods, true);
            }
            else
            {
                if (State == Visibility.Visible) sampleOff?.Play();
            }

            refreshSelectedMods();
        }

        private void refreshSelectedMods() => SelectedMods.Value = ModSectionsContainer.Children.SelectMany(s => s.SelectedMods).ToArray();

        public ModSelectOverlay()
        {
            Waves.FirstWaveColour = OsuColour.FromHex(@"19b0e2");
            Waves.SecondWaveColour = OsuColour.FromHex(@"2280a2");
            Waves.ThirdWaveColour = OsuColour.FromHex(@"005774");
            Waves.FourthWaveColour = OsuColour.FromHex(@"003a4e");

            Height = 510;
            Padding = new MarginPadding { Horizontal = -OsuScreen.HORIZONTAL_OVERFLOW_PADDING };

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
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 90),
                        new Dimension(GridSizeMode.Distributed),
                        new Dimension(GridSizeMode.Absolute, 70),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
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
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Width = content_width,
                                        Padding = new MarginPadding { Horizontal = OsuScreen.HORIZONTAL_OVERFLOW_PADDING },
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = @"Gameplay Mods",
                                                Font = OsuFont.GetFont(size: 22, weight: FontWeight.Bold),
                                                Shadow = true,
                                                Margin = new MarginPadding
                                                {
                                                    Bottom = 4,
                                                },
                                            },
                                            new OsuTextFlowContainer(text =>
                                            {
                                                text.Font = text.Font.With(size: 18);
                                                text.Shadow = true;
                                            })
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Text = "Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play.\nOthers are just for fun.",
                                            },
                                        },
                                    },
                                },
                            },
                        },
                        new Drawable[]
                        {
                            // Body
                            new OsuScrollContainer
                            {
                                ScrollbarVisible = false,
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Vertical = 10,
                                    Horizontal = OsuScreen.HORIZONTAL_OVERFLOW_PADDING
                                },
                                Child = ModSectionsContainer = new FillFlowContainer<ModSection>
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(0f, 10f),
                                    Width = content_width,
                                    Children = new ModSection[]
                                    {
                                        new DifficultyReductionSection { Action = modButtonPressed },
                                        new DifficultyIncreaseSection { Action = modButtonPressed },
                                        new AutomationSection { Action = modButtonPressed },
                                        new ConversionSection { Action = modButtonPressed },
                                        new FunSection { Action = modButtonPressed },
                                    }
                                },
                            },
                        },
                        new Drawable[]
                        {
                            // Footer
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
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
                                    footerContainer = new FillFlowContainer
                                    {
                                        Origin = Anchor.BottomCentre,
                                        Anchor = Anchor.BottomCentre,
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Width = content_width,
                                        Direction = FillDirection.Horizontal,
                                        Padding = new MarginPadding
                                        {
                                            Vertical = 15,
                                            Horizontal = OsuScreen.HORIZONTAL_OVERFLOW_PADDING
                                        },
                                        Children = new Drawable[]
                                        {
                                            DeselectAllButton = new TriangleButton
                                            {
                                                Width = 180,
                                                Text = "Deselect All",
                                                Action = DeselectAll,
                                                Margin = new MarginPadding
                                                {
                                                    Right = 20
                                                }
                                            },
                                            new OsuSpriteText
                                            {
                                                Text = @"Score Multiplier:",
                                                Font = OsuFont.GetFont(size: 30),
                                                Margin = new MarginPadding
                                                {
                                                    Top = 5,
                                                    Right = 10
                                                }
                                            },
                                            MultiplierLabel = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold),
                                                Margin = new MarginPadding
                                                {
                                                    Top = 5
                                                }
                                            },
                                            UnrankedLabel = new OsuSpriteText
                                            {
                                                Text = @"(Unranked)",
                                                Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold),
                                                Margin = new MarginPadding
                                                {
                                                    Top = 5,
                                                    Left = 10
                                                }
                                            }
                                        }
                                    }
                                },
                            }
                        },
                    },
                },
            };
        }
    }
}
