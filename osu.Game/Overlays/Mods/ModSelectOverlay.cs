// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
using osu.Game.Rulesets.Mods;
using osu.Game.Screens;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public abstract class ModSelectOverlay : WaveOverlayContainer
    {
        public const float HEIGHT = 510;

        protected readonly TriangleButton DeselectAllButton;
        protected readonly TriangleButton CustomiseButton;
        protected readonly TriangleButton CloseButton;

        protected readonly Drawable MultiplierSection;
        protected readonly OsuSpriteText MultiplierLabel;

        protected readonly FillFlowContainer FooterContainer;

        protected override bool BlockNonPositionalInput => false;

        protected override bool DimMainContent => false;

        /// <summary>
        /// Whether <see cref="Mod"/>s underneath the same <see cref="MultiMod"/> instance should appear as stacked buttons.
        /// </summary>
        protected virtual bool Stacked => true;

        /// <summary>
        /// Whether configurable <see cref="Mod"/>s can be configured by the local user.
        /// </summary>
        protected virtual bool AllowConfiguration => true;

        [NotNull]
        private Func<Mod, bool> isValidMod = m => true;

        /// <summary>
        /// A function that checks whether a given mod is selectable.
        /// </summary>
        [NotNull]
        public Func<Mod, bool> IsValidMod
        {
            get => isValidMod;
            set
            {
                isValidMod = value ?? throw new ArgumentNullException(nameof(value));
                updateAvailableMods();
            }
        }

        protected readonly FillFlowContainer<ModSection> ModSectionsContainer;

        protected readonly ModSettingsContainer ModSettingsContainer;

        public readonly Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> availableMods;

        protected Color4 LowMultiplierColour;
        protected Color4 HighMultiplierColour;

        private const float content_width = 0.8f;
        private const float footer_button_spacing = 20;

        private Sample sampleOn, sampleOff;

        protected ModSelectOverlay()
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
                                                Children = new[]
                                                {
                                                    CreateModSection(ModType.DifficultyReduction).With(s =>
                                                    {
                                                        s.ToggleKeys = new[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P };
                                                        s.Action = modButtonPressed;
                                                    }),
                                                    CreateModSection(ModType.DifficultyIncrease).With(s =>
                                                    {
                                                        s.ToggleKeys = new[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L };
                                                        s.Action = modButtonPressed;
                                                    }),
                                                    CreateModSection(ModType.Automation).With(s =>
                                                    {
                                                        s.ToggleKeys = new[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M };
                                                        s.Action = modButtonPressed;
                                                    }),
                                                    CreateModSection(ModType.Conversion).With(s =>
                                                    {
                                                        s.Action = modButtonPressed;
                                                    }),
                                                    CreateModSection(ModType.Fun).With(s =>
                                                    {
                                                        s.Action = modButtonPressed;
                                                    }),
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
                            new Container
                            {
                                Name = "Footer content",
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
                                    FooterContainer = new FillFlowContainer
                                    {
                                        Origin = Anchor.BottomCentre,
                                        Anchor = Anchor.BottomCentre,
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        RelativePositionAxes = Axes.X,
                                        Width = content_width,
                                        Spacing = new Vector2(footer_button_spacing, footer_button_spacing / 2),
                                        Padding = new MarginPadding
                                        {
                                            Vertical = 15,
                                            Horizontal = OsuScreen.HORIZONTAL_OVERFLOW_PADDING
                                        },
                                        Children = new[]
                                        {
                                            DeselectAllButton = new TriangleButton
                                            {
                                                Width = 180,
                                                Text = "Deselect All",
                                                Action = deselectAll,
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                            },
                                            CustomiseButton = new TriangleButton
                                            {
                                                Width = 180,
                                                Text = "Customisation",
                                                Action = () => ModSettingsContainer.ToggleVisibility(),
                                                Enabled = { Value = false },
                                                Alpha = AllowConfiguration ? 1 : 0,
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
                                            MultiplierSection = new FillFlowContainer
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

        private void deselectAll()
        {
            foreach (var section in ModSectionsContainer.Children)
                section.DeselectAll();

            refreshSelectedMods();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            availableMods.BindValueChanged(_ => updateAvailableMods(), true);

            // intentionally bound after the above line to avoid a potential update feedback cycle.
            // i haven't actually observed this happening but as updateAvailableMods() changes the selection it is plausible.
            SelectedMods.BindValueChanged(_ => updateSelectedButtons());
        }

        protected override void PopOut()
        {
            base.PopOut();

            foreach (var section in ModSectionsContainer)
            {
                section.FlushAnimation();
            }

            FooterContainer.MoveToX(content_width, WaveContainer.DISAPPEAR_DURATION, Easing.InSine);
            FooterContainer.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.InSine);

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

            FooterContainer.MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            FooterContainer.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);

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

        private void updateAvailableMods()
        {
            if (availableMods?.Value == null)
                return;

            foreach (var section in ModSectionsContainer.Children)
            {
                IEnumerable<Mod> modEnumeration = availableMods.Value[section.ModType];

                if (!Stacked)
                    modEnumeration = ModUtils.FlattenMods(modEnumeration);

                section.Mods = modEnumeration.Select(getValidModOrNull).Where(m => m != null);
            }

            updateSelectedButtons();
            OnAvailableModsChanged();
        }

        /// <summary>
        /// Returns a valid form of a given <see cref="Mod"/> if possible, or null otherwise.
        /// </summary>
        /// <remarks>
        /// This is a recursive process during which any invalid mods are culled while preserving <see cref="MultiMod"/> structures where possible.
        /// </remarks>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        /// <returns>A valid form of <paramref name="mod"/> if exists, or null otherwise.</returns>
        [CanBeNull]
        private Mod getValidModOrNull([NotNull] Mod mod)
        {
            if (!(mod is MultiMod multi))
                return IsValidMod(mod) ? mod : null;

            var validSubset = multi.Mods.Select(getValidModOrNull).Where(m => m != null).ToArray();

            if (validSubset.Length == 0)
                return null;

            return validSubset.Length == 1 ? validSubset[0] : new MultiMod(validSubset);
        }

        private void updateSelectedButtons()
        {
            // Enumeration below may update the bindable list.
            var selectedMods = SelectedMods.Value.ToList();

            foreach (var section in ModSectionsContainer.Children)
                section.UpdateSelectedButtons(selectedMods);

            updateMultiplier();
        }

        private void updateMultiplier()
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
                if (State.Value == Visibility.Visible)
                    Scheduler.AddOnce(playSelectedSound);

                OnModSelected(selectedMod);

                if (selectedMod.RequiresConfiguration && AllowConfiguration)
                    ModSettingsContainer.Show();
            }
            else
            {
                if (State.Value == Visibility.Visible)
                    Scheduler.AddOnce(playDeselectedSound);
            }

            refreshSelectedMods();
        }

        private void playSelectedSound() => sampleOn?.Play();
        private void playDeselectedSound() => sampleOff?.Play();

        /// <summary>
        /// Invoked after <see cref="availableMods"/> has changed.
        /// </summary>
        protected virtual void OnAvailableModsChanged()
        {
        }

        /// <summary>
        /// Invoked when a new <see cref="Mod"/> has been selected.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> that has been selected.</param>
        protected virtual void OnModSelected(Mod mod)
        {
        }

        private void refreshSelectedMods() => SelectedMods.Value = ModSectionsContainer.Children.SelectMany(s => s.SelectedMods).ToArray();

        /// <summary>
        /// Creates a <see cref="ModSection"/> that groups <see cref="Mod"/>s with the same <see cref="ModType"/>.
        /// </summary>
        /// <param name="type">The <see cref="ModType"/> of <see cref="Mod"/>s in the section.</param>
        /// <returns>The <see cref="ModSection"/>.</returns>
        protected virtual ModSection CreateModSection(ModType type) => new ModSection(type);

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
