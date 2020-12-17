// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Mods.Sections;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public class ModSelectOverlay : WaveOverlayContainer
    {
        public const float HEIGHT = 510;

        protected readonly TriangleButton DeselectAllButton;
        protected readonly TriangleButton CustomiseButton;
        protected readonly TriangleButton CloseButton;

        protected readonly OsuSpriteText MultiplierLabel;

        protected override bool BlockNonPositionalInput => false;

        protected override bool DimMainContent => false;

        protected readonly FillFlowContainer<ModSection> ModSectionsContainer;

        protected readonly ModSettingsContainer ModSettingsContainer;

        public readonly Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> availableMods;

        protected Color4 LowMultiplierColour;
        protected Color4 HighMultiplierColour;

        private const float content_width = 0.8f;
        private const float footer_button_spacing = 20;

        private readonly FillFlowContainer footerContainer;

        private SampleChannel sampleOn, sampleOff;

        public ModSelectOverlay()
        {
            Waves.FirstWaveColour = Color4Extensions.FromHex(@"19b0e2");
            Waves.SecondWaveColour = Color4Extensions.FromHex(@"2280a2");
            Waves.ThirdWaveColour = Color4Extensions.FromHex(@"005774");
            Waves.FourthWaveColour = Color4Extensions.FromHex(@"003a4e");

            RelativeSizeAxes = Axes.Both;

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
                            RelativeSizeAxes = Axes.Both,
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
                        new Dimension(GridSizeMode.AutoSize),
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
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
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
                                        Children = new Drawable[]
                                        {
                                            ModSectionsContainer = new FillFlowContainer<ModSection>
                                            {
                                                Origin = Anchor.TopCentre,
                                                Anchor = Anchor.TopCentre,
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Spacing = new Vector2(0f, 10f),
                                                Width = content_width,
                                                LayoutDuration = 200,
                                                LayoutEasing = Easing.OutQuint,
                                                Children = new ModSection[]
                                                {
                                                    new DifficultyReductionSection { Action = modButtonPressed },
                                                    new DifficultyIncreaseSection { Action = modButtonPressed },
                                                    new AutomationSection { Action = modButtonPressed },
                                                    new ConversionSection { Action = modButtonPressed },
                                                    new FunSection { Action = modButtonPressed },
                                                }
                                            },
                                        }
                                    },
                                    ModSettingsContainer = new ModSettingsContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.BottomRight,
                                        Origin = Anchor.BottomRight,
                                        Width = 0.3f,
                                        Alpha = 0,
                                        Padding = new MarginPadding(30),
                                        SelectedMods = { BindTarget = SelectedMods },
                                    },
                                }
                            },
                        },
                        new Drawable[]
                        {
                            // Footer
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
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
                                        Spacing = new Vector2(footer_button_spacing, footer_button_spacing / 2),
                                        LayoutDuration = 100,
                                        LayoutEasing = Easing.OutQuint,
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
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                            },
                                            CustomiseButton = new TriangleButton
                                            {
                                                Width = 180,
                                                Text = "Customisation",
                                                Action = () => ModSettingsContainer.ToggleVisibility(),
                                                Enabled = { Value = false },
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                            },
                                            CloseButton = new TriangleButton
                                            {
                                                Width = 180,
                                                Text = "Close",
                                                Action = Hide,
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                            },
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Spacing = new Vector2(footer_button_spacing / 2, 0),
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Text = @"Score Multiplier:",
                                                        Font = OsuFont.GetFont(size: 30),
                                                        Origin = Anchor.CentreLeft,
                                                        Anchor = Anchor.CentreLeft,
                                                    },
                                                    MultiplierLabel = new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold),
                                                        Origin = Anchor.CentreLeft,
                                                        Anchor = Anchor.CentreLeft,
                                                        Width = 70, // make width fixed so reflow doesn't occur when multiplier number changes.
                                                    },
                                                },
                                            },
                                        }
                                    }
                                },
                            }
                        },
                    },
                },
            };

            ((IBindable<bool>)CustomiseButton.Enabled).BindTo(ModSettingsContainer.HasSettingsForSelection);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, AudioManager audio, OsuGameBase osu)
        {
            LowMultiplierColour = colours.Red;
            HighMultiplierColour = colours.Green;

            availableMods = osu.AvailableMods.GetBoundCopy();

            sampleOn = audio.Samples.Get(@"UI/check-on");
            sampleOff = audio.Samples.Get(@"UI/check-off");
        }

        public void DeselectAll()
        {
            foreach (var section in ModSectionsContainer.Children)
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

            foreach (var section in ModSectionsContainer.Children)
                section.DeselectTypes(modTypes, immediate);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            availableMods.BindValueChanged(availableModsChanged, true);
            SelectedMods.BindValueChanged(selectedModsChanged, true);
        }

        protected override void PopOut()
        {
            base.PopOut();

            footerContainer.MoveToX(footerContainer.DrawSize.X, WaveContainer.DISAPPEAR_DURATION, Easing.InSine);
            footerContainer.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.InSine);

            foreach (var section in ModSectionsContainer.Children)
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

            foreach (var section in ModSectionsContainer.Children)
            {
                section.ButtonsContainer.TransformSpacingTo(new Vector2(50f, 0f), WaveContainer.APPEAR_DURATION, Easing.OutQuint);
                section.ButtonsContainer.MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
                section.ButtonsContainer.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // don't absorb control as ToolbarRulesetSelector uses control + number to navigate
            if (e.ControlPressed) return false;

            switch (e.Key)
            {
                case Key.Number1:
                    DeselectAllButton.Click();
                    return true;

                case Key.Number2:
                    CloseButton.Click();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        public override bool OnPressed(GlobalAction action) => false; // handled by back button

        private void availableModsChanged(ValueChangedEvent<Dictionary<ModType, IReadOnlyList<Mod>>> mods)
        {
            if (mods.NewValue == null) return;

            foreach (var section in ModSectionsContainer.Children)
                section.Mods = mods.NewValue[section.ModType];
        }

        private void selectedModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            foreach (var section in ModSectionsContainer.Children)
                section.SelectTypes(mods.NewValue.Select(m => m.GetType()).ToList());

            updateMods();
        }

        private void updateMods()
        {
            var multiplier = 1.0;

            foreach (var mod in SelectedMods.Value)
            {
                multiplier *= mod.ScoreMultiplier;
            }

            MultiplierLabel.Text = $"{multiplier:N2}x";
            if (multiplier > 1.0)
                MultiplierLabel.FadeColour(HighMultiplierColour, 200);
            else if (multiplier < 1.0)
                MultiplierLabel.FadeColour(LowMultiplierColour, 200);
            else
                MultiplierLabel.FadeColour(Color4.White, 200);
        }

        private void modButtonPressed(Mod selectedMod)
        {
            if (selectedMod != null)
            {
                if (State.Value == Visibility.Visible) sampleOn?.Play();

                DeselectTypes(selectedMod.IncompatibleMods, true);

                if (selectedMod.RequiresConfiguration) ModSettingsContainer.Show();
            }
            else
            {
                if (State.Value == Visibility.Visible) sampleOff?.Play();
            }

            refreshSelectedMods();
        }

        private void refreshSelectedMods() => SelectedMods.Value = ModSectionsContainer.Children.SelectMany(s => s.SelectedMods).ToArray();

        #region Disposal

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            availableMods?.UnbindAll();
            SelectedMods?.UnbindAll();
        }

        #endregion
    }
}
