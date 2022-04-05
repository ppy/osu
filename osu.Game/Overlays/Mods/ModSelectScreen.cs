// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public class ModSelectScreen : OsuFocusedOverlayContainer
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Cached]
        public Bindable<IReadOnlyList<Mod>> SelectedMods { get; private set; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        protected override bool StartHidden => true;

        private readonly BindableBool customisationVisible = new BindableBool();

        private DifficultyMultiplierDisplay multiplierDisplay;
        private ModSettingsArea modSettingsArea;
        private FillFlowContainer<ModColumn> columnFlow;
        private GridContainer grid;
        private Container mainContent;

        private PopupScreenTitle header;
        private Container footer;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                mainContent = new Container
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        grid = new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 75),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    header = new PopupScreenTitle
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Title = "Mod Select",
                                        Description = "Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play. Others are just for fun.",
                                        Close = Hide
                                    }
                                },
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        AutoSizeAxes = Axes.X,
                                        RelativePositionAxes = Axes.X,
                                        X = 0.3f,
                                        Height = DifficultyMultiplierDisplay.HEIGHT,
                                        Margin = new MarginPadding
                                        {
                                            Horizontal = 100,
                                            Vertical = 10
                                        },
                                        Child = multiplierDisplay = new DifficultyMultiplierDisplay
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre
                                        }
                                    }
                                },
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        Depth = float.MaxValue,
                                        RelativeSizeAxes = Axes.Both,
                                        RelativePositionAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            new OsuScrollContainer(Direction.Horizontal)
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Masking = false,
                                                ClampExtension = 100,
                                                ScrollbarOverlapsContent = false,
                                                Child = columnFlow = new ModColumnContainer
                                                {
                                                    Direction = FillDirection.Horizontal,
                                                    RelativeSizeAxes = Axes.Y,
                                                    AutoSizeAxes = Axes.X,
                                                    Spacing = new Vector2(10, 0),
                                                    Margin = new MarginPadding { Right = 70 },
                                                    Children = new[]
                                                    {
                                                        new ModColumn(ModType.DifficultyReduction, false, new[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P }),
                                                        new ModColumn(ModType.DifficultyIncrease, false, new[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L }),
                                                        new ModColumn(ModType.Automation, false, new[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M }),
                                                        new ModColumn(ModType.Conversion, false),
                                                        new ModColumn(ModType.Fun, false)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new[] { Empty() }
                            }
                        },
                        footer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 50,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Colour = colourProvider.Background5
                                },
                                new ShearedToggleButton(200)
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Margin = new MarginPadding { Vertical = 14, Left = 70 },
                                    Text = "Mod Customisation",
                                    Active = { BindTarget = customisationVisible }
                                }
                            }
                        },
                        new ClickToReturnContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            HandleMouse = { BindTarget = customisationVisible },
                            OnClicked = () => customisationVisible.Value = false
                        }
                    }
                },
                modSettingsArea = new ModSettingsArea
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Height = 0
                }
            };

            columnFlow.Shear = new Vector2(ModPanel.SHEAR_X, 0);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ((IBindable<IReadOnlyList<Mod>>)modSettingsArea.SelectedMods).BindTo(SelectedMods);

            SelectedMods.BindValueChanged(val =>
            {
                updateMultiplier();
                updateCustomisation(val);
                updateSelectionFromBindable();
            }, true);

            foreach (var column in columnFlow)
            {
                column.SelectedMods.BindValueChanged(_ => updateBindableFromSelection());
            }

            customisationVisible.BindValueChanged(_ => updateCustomisationVisualState(), true);
        }

        private void updateMultiplier()
        {
            double multiplier = 1.0;

            foreach (var mod in SelectedMods.Value)
                multiplier *= mod.ScoreMultiplier;

            multiplierDisplay.Current.Value = multiplier;
        }

        private void updateCustomisation(ValueChangedEvent<IReadOnlyList<Mod>> valueChangedEvent)
        {
            bool anyCustomisableMod = false;
            bool anyModWithRequiredCustomisationAdded = false;

            foreach (var mod in SelectedMods.Value)
            {
                anyCustomisableMod |= mod.GetSettingsSourceProperties().Any();
                anyModWithRequiredCustomisationAdded |= !valueChangedEvent.OldValue.Contains(mod) && mod.RequiresConfiguration;
            }

            if (anyCustomisableMod)
            {
                customisationVisible.Disabled = false;

                if (anyModWithRequiredCustomisationAdded && !customisationVisible.Value)
                    customisationVisible.Value = true;
            }
            else
            {
                if (customisationVisible.Value)
                    customisationVisible.Value = false;

                customisationVisible.Disabled = true;
            }
        }

        private void updateCustomisationVisualState()
        {
            const double transition_duration = 300;

            grid.FadeColour(customisationVisible.Value ? Colour4.Gray : Colour4.White, transition_duration, Easing.InOutCubic);

            float modAreaHeight = customisationVisible.Value ? ModSettingsArea.HEIGHT : 0;

            modSettingsArea.ResizeHeightTo(modAreaHeight, transition_duration, Easing.InOutCubic);
            mainContent.TransformTo(nameof(Margin), new MarginPadding { Bottom = modAreaHeight }, transition_duration, Easing.InOutCubic);
        }

        private bool selectionBindableSyncInProgress;

        private void updateSelectionFromBindable()
        {
            if (selectionBindableSyncInProgress)
                return;

            selectionBindableSyncInProgress = true;

            foreach (var column in columnFlow)
                column.SelectedMods.Value = SelectedMods.Value.Where(mod => mod.Type == column.ModType).ToArray();

            selectionBindableSyncInProgress = false;
        }

        private void updateBindableFromSelection()
        {
            if (selectionBindableSyncInProgress)
                return;

            selectionBindableSyncInProgress = true;

            SelectedMods.Value = columnFlow.SelectMany(column => column.SelectedMods.Value).ToArray();

            selectionBindableSyncInProgress = false;
        }

        protected override void PopIn()
        {
            const double fade_in_duration = 400;

            base.PopIn();
            this.FadeIn(fade_in_duration, Easing.OutQuint);

            header.MoveToY(0, fade_in_duration, Easing.OutQuint);
            footer.MoveToY(0, fade_in_duration, Easing.OutQuint);

            multiplierDisplay
                .Delay(fade_in_duration * 0.65f)
                .FadeIn(fade_in_duration / 2, Easing.OutQuint)
                .ScaleTo(1, fade_in_duration, Easing.OutElastic);

            for (int i = 0; i < columnFlow.Count; i++)
            {
                columnFlow[i].TopLevelContent
                             .Delay(i * 30)
                             .MoveToY(0, fade_in_duration, Easing.OutQuint)
                             .FadeIn(fade_in_duration, Easing.OutQuint);
            }
        }

        protected override void PopOut()
        {
            const double fade_out_duration = 500;

            base.PopOut();
            this.FadeOut(fade_out_duration, Easing.OutQuint);

            multiplierDisplay
                .FadeOut(fade_out_duration / 2, Easing.OutQuint)
                .ScaleTo(0.75f, fade_out_duration, Easing.OutQuint);

            header.MoveToY(-header.DrawHeight, fade_out_duration, Easing.OutQuint);
            footer.MoveToY(footer.DrawHeight, fade_out_duration, Easing.OutQuint);

            for (int i = 0; i < columnFlow.Count; i++)
            {
                const float distance = 700;

                columnFlow[i].TopLevelContent
                             .MoveToY(i % 2 == 0 ? -distance : distance, fade_out_duration, Easing.OutQuint)
                             .FadeOut(fade_out_duration, Easing.OutQuint);
            }
        }

        private class ModColumnContainer : FillFlowContainer<ModColumn>
        {
            private readonly LayoutValue drawSizeLayout = new LayoutValue(Invalidation.DrawSize);

            public ModColumnContainer()
            {
                AddLayout(drawSizeLayout);
            }

            public override void Add(ModColumn column)
            {
                base.Add(column);

                Debug.Assert(column != null);
                column.Shear = Vector2.Zero;
            }

            protected override void Update()
            {
                base.Update();

                if (!drawSizeLayout.IsValid)
                {
                    Padding = new MarginPadding
                    {
                        Left = DrawHeight * ModPanel.SHEAR_X,
                        Bottom = 10
                    };

                    drawSizeLayout.Validate();
                }
            }
        }

        private class ClickToReturnContainer : Container
        {
            public BindableBool HandleMouse { get; } = new BindableBool();

            public Action OnClicked { get; set; }

            protected override bool Handle(UIEvent e)
            {
                if (!HandleMouse.Value)
                    return base.Handle(e);

                switch (e)
                {
                    case ClickEvent _:
                        OnClicked?.Invoke();
                        return true;

                    case MouseEvent _:
                        return true;
                }

                return base.Handle(e);
            }
        }
    }
}
