// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Layout;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public abstract class ModSelectScreen : ShearedOverlayContainer
    {
        protected override OverlayColourScheme ColourScheme => OverlayColourScheme.Green;

        [Cached]
        public Bindable<IReadOnlyList<Mod>> SelectedMods { get; private set; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private Func<Mod, bool> isValidMod = m => true;

        public Func<Mod, bool> IsValidMod
        {
            get => isValidMod;
            set
            {
                isValidMod = value ?? throw new ArgumentNullException(nameof(value));

                if (IsLoaded)
                    updateAvailableMods();
            }
        }

        /// <summary>
        /// Whether configurable <see cref="Mod"/>s can be configured by the local user.
        /// </summary>
        protected virtual bool AllowCustomisation => true;

        /// <summary>
        /// Whether the total score multiplier calculated from the current selected set of mods should be shown.
        /// </summary>
        protected virtual bool ShowTotalMultiplier => true;

        protected virtual ModColumn CreateModColumn(ModType modType, Key[]? toggleKeys = null) => new ModColumn(modType, false, toggleKeys);

        private readonly BindableBool customisationVisible = new BindableBool();

        private DifficultyMultiplierDisplay? multiplierDisplay;
        private ModSettingsArea modSettingsArea = null!;
        private OsuScrollContainer columnScroll = null!;
        private ColumnFlowContainer columnFlow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Header.Title = "Mod Select";
            Header.Description = "Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play. Others are just for fun.";

            AddRange(new Drawable[]
            {
                new ClickToReturnContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    HandleMouse = { BindTarget = customisationVisible },
                    OnClicked = () => customisationVisible.Value = false
                },
                modSettingsArea = new ModSettingsArea
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Height = 0
                }
            });

            // initialise the scroll early, as we will want to pass it to its children in the hierarchy initialisation below.
            columnScroll = new OsuScrollContainer(Direction.Horizontal);

            MainAreaContent.AddRange(new Drawable[]
            {
                new Container
                {
                    Padding = new MarginPadding
                    {
                        Top = (ShowTotalMultiplier ? DifficultyMultiplierDisplay.HEIGHT : 0) + PADDING,
                    },
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        columnScroll.With(col =>
                        {
                            col.RelativeSizeAxes = Axes.Both;
                            col.Masking = false;
                            col.ClampExtension = 100;
                            col.ScrollbarOverlapsContent = false;
                            col.Child = columnFlow = new ColumnFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                Shear = new Vector2(SHEAR, 0),
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Spacing = new Vector2(10, 0),
                                Margin = new MarginPadding { Horizontal = 70 },
                                Children = new[]
                                {
                                    createModColumnContent(columnScroll, ModType.DifficultyReduction, new[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P }),
                                    createModColumnContent(columnScroll, ModType.DifficultyIncrease, new[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L }),
                                    createModColumnContent(columnScroll, ModType.Automation, new[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M }),
                                    createModColumnContent(columnScroll, ModType.Conversion),
                                    createModColumnContent(columnScroll, ModType.Fun)
                                }
                            };
                        })
                    }
                }
            });

            if (ShowTotalMultiplier)
            {
                MainAreaContent.Add(new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.X,
                    Height = DifficultyMultiplierDisplay.HEIGHT,
                    Margin = new MarginPadding { Horizontal = 100 },
                    Child = multiplierDisplay = new DifficultyMultiplierDisplay
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                });
            }

            if (AllowCustomisation)
            {
                Footer.Add(new ShearedToggleButton(200)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Margin = new MarginPadding { Vertical = PADDING, Left = 70 },
                    Text = "Mod Customisation",
                    Active = { BindTarget = customisationVisible }
                });
            }
        }

        private ColumnDimContainer createModColumnContent(OsuScrollContainer scroll, ModType modType, Key[]? toggleKeys = null)
            => new ColumnDimContainer(scroll, CreateModColumn(modType, toggleKeys))
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y
            };

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

            foreach (var column in columnFlow.Columns)
            {
                column.SelectedMods.BindValueChanged(updateBindableFromSelection);
            }

            customisationVisible.BindValueChanged(_ => updateCustomisationVisualState(), true);

            updateAvailableMods();
        }

        private void updateMultiplier()
        {
            if (multiplierDisplay == null)
                return;

            double multiplier = 1.0;

            foreach (var mod in SelectedMods.Value)
                multiplier *= mod.ScoreMultiplier;

            multiplierDisplay.Current.Value = multiplier;
        }

        private void updateAvailableMods()
        {
            foreach (var column in columnFlow.Columns)
                column.Filter = isValidMod;
        }

        private void updateCustomisation(ValueChangedEvent<IReadOnlyList<Mod>> valueChangedEvent)
        {
            if (!AllowCustomisation)
                return;

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

            MainAreaContent.FadeColour(customisationVisible.Value ? Colour4.Gray : Colour4.White, transition_duration, Easing.InOutCubic);

            float modAreaHeight = customisationVisible.Value ? ModSettingsArea.HEIGHT : 0;

            modSettingsArea.ResizeHeightTo(modAreaHeight, transition_duration, Easing.InOutCubic);
            TopLevelContent.MoveToY(-modAreaHeight, transition_duration, Easing.InOutCubic);
        }

        private void updateSelectionFromBindable()
        {
            // note that selectionBindableSyncInProgress is purposefully not checked here.
            // this is because in the case of mod selection in solo gameplay, a user selection of a mod can actually lead to deselection of other incompatible mods.
            // to synchronise state correctly, updateBindableFromSelection() computes the final mods (including incompatibility rules) and updates SelectedMods,
            // and this method then runs unconditionally again to make sure the new visual selection accurately reflects the final set of selected mods.
            // selectionBindableSyncInProgress ensures that mutual infinite recursion does not happen after that unconditional call.
            foreach (var column in columnFlow.Columns)
                column.SelectedMods.Value = SelectedMods.Value.Where(mod => mod.Type == column.ModType).ToArray();
        }

        private bool selectionBindableSyncInProgress;

        private void updateBindableFromSelection(ValueChangedEvent<IReadOnlyList<Mod>> modSelectionChange)
        {
            if (selectionBindableSyncInProgress)
                return;

            selectionBindableSyncInProgress = true;

            SelectedMods.Value = ComputeNewModsFromSelection(
                modSelectionChange.NewValue.Except(modSelectionChange.OldValue),
                modSelectionChange.OldValue.Except(modSelectionChange.NewValue));

            selectionBindableSyncInProgress = false;
        }

        protected virtual IReadOnlyList<Mod> ComputeNewModsFromSelection(IEnumerable<Mod> addedMods, IEnumerable<Mod> removedMods)
            => columnFlow.Columns.SelectMany(column => column.SelectedMods.Value).ToArray();

        protected override void PopIn()
        {
            const double fade_in_duration = 400;

            base.PopIn();

            multiplierDisplay?
                .Delay(fade_in_duration * 0.65f)
                .FadeIn(fade_in_duration / 2, Easing.OutQuint)
                .ScaleTo(1, fade_in_duration, Easing.OutElastic);

            for (int i = 0; i < columnFlow.Count; i++)
            {
                columnFlow[i].Column
                             .TopLevelContent
                             .Delay(i * 30)
                             .MoveToY(0, fade_in_duration, Easing.OutQuint)
                             .FadeIn(fade_in_duration, Easing.OutQuint);
            }
        }

        protected override void PopOut()
        {
            const double fade_out_duration = 500;

            base.PopOut();

            multiplierDisplay?
                .FadeOut(fade_out_duration / 2, Easing.OutQuint)
                .ScaleTo(0.75f, fade_out_duration, Easing.OutQuint);

            for (int i = 0; i < columnFlow.Count; i++)
            {
                const float distance = 700;

                columnFlow[i].Column
                             .TopLevelContent
                             .MoveToY(i % 2 == 0 ? -distance : distance, fade_out_duration, Easing.OutQuint)
                             .FadeOut(fade_out_duration, Easing.OutQuint);
            }
        }

        private class ColumnFlowContainer : FillFlowContainer<ColumnDimContainer>
        {
            public IEnumerable<ModColumn> Columns => Children.Select(dimWrapper => dimWrapper.Column);

            private readonly LayoutValue drawSizeLayout = new LayoutValue(Invalidation.DrawSize);

            public ColumnFlowContainer()
            {
                AddLayout(drawSizeLayout);
            }

            public override void Add(ColumnDimContainer dimContainer)
            {
                base.Add(dimContainer);

                Debug.Assert(dimContainer != null);
                dimContainer.Column.Shear = Vector2.Zero;
            }

            protected override void Update()
            {
                base.Update();

                if (!drawSizeLayout.IsValid)
                {
                    Padding = new MarginPadding
                    {
                        Left = DrawHeight * SHEAR,
                        Bottom = 10
                    };

                    drawSizeLayout.Validate();
                }
            }
        }

        private class ColumnDimContainer : Container
        {
            public ModColumn Column { get; }

            private readonly ScrollContainer<Drawable> parentScroll;
            private readonly Bindable<bool> isFullyOnScreen = new BindableBool(true);

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public ColumnDimContainer(ScrollContainer<Drawable> parentScroll, ModColumn column)
            {
                this.parentScroll = parentScroll;
                Child = Column = column;
                column.Active.BindTo(isFullyOnScreen);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                isFullyOnScreen.BindValueChanged(_ => updateDim(), true);
                FinishTransforms();
            }

            private void updateDim()
            {
                Colour4 targetColour;

                if (isFullyOnScreen.Value)
                    targetColour = Colour4.White;
                else
                    targetColour = IsHovered ? colours.GrayC : colours.Gray8;

                this.FadeColour(targetColour, 300, Easing.OutQuint);
            }

            protected override void Update()
            {
                float leftX = DrawPosition.X;
                // DrawWidth does not include shear effects, and we want to know the full extents of the columns post-shear,
                // so we have to manually compensate.
                float rightX = DrawPosition.X + DrawWidth + DrawHeight * SHEAR;

                isFullyOnScreen.Value = Precision.AlmostBigger(leftX, parentScroll.Current)
                                        && Precision.DefinitelyBigger(parentScroll.Current + parentScroll.DrawWidth, rightX);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (!isFullyOnScreen.Value)
                    parentScroll.ScrollTo(Math.Clamp(DrawPosition.X - 70, 0, parentScroll.ScrollableExtent));

                return true;
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                updateDim();
                return isFullyOnScreen.Value;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                updateDim();
            }
        }

        private class ClickToReturnContainer : Container
        {
            public BindableBool HandleMouse { get; } = new BindableBool();

            public Action? OnClicked { get; set; }

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
