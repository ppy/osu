// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public partial class NewComboTernaryButton : CompositeDrawable, IHasCurrentValue<TernaryState>
    {
        public Bindable<TernaryState> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<TernaryState> current = new BindableWithCurrent<TernaryState>();

        private readonly BindableList<HitObject> selectedHitObjects = new BindableList<HitObject>();
        private readonly BindableList<Colour4> comboColours = new BindableList<Colour4>();

        private readonly Bindable<bool> expanded = new Bindable<bool>(true);

        private Container mainButtonContainer = null!;
        private ColourPickerButton pickerButton = null!;
        private DrawableTernaryButton mainButton = null!;

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap editorBeatmap, IExpandingContainer? expandableParent)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                mainButtonContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = mainButton = new DrawableTernaryButton
                    {
                        Current = Current,
                        Description = "New combo",
                        CreateIcon = () => new SpriteIcon { Icon = OsuIcon.EditorNewComboA },
                    },
                },
                pickerButton = new ColourPickerButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    ComboColours = { BindTarget = comboColours }
                }
            };

            selectedHitObjects.BindTo(editorBeatmap.SelectedHitObjects);
            if (editorBeatmap.BeatmapSkin != null)
                comboColours.BindTo(editorBeatmap.BeatmapSkin.ComboColours);

            if (expandableParent != null)
                expanded.BindTo(expandableParent.Expanded);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedHitObjects.BindCollectionChanged((_, _) => updateState());
            comboColours.BindCollectionChanged((_, _) => updateState());
            expanded.BindValueChanged(_ => updateState());
            Current.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            if (Current.Value == TernaryState.True && selectedHitObjects.Count == 1 && selectedHitObjects.Single() is IHasComboInformation hasCombo && comboColours.Count > 1)
            {
                float targetPickerButtonWidth = expanded.Value ? 25 : 10;

                pickerButton.ResizeWidthTo(targetPickerButtonWidth, ExpandingContainer.TRANSITION_DURATION, Easing.OutQuint);
                pickerButton.SelectedHitObject.Value = hasCombo;
                pickerButton.Icon.Alpha = expanded.Value ? 1 : 0;

                mainButtonContainer.TransformTo(nameof(mainButtonContainer.Padding), new MarginPadding { Right = targetPickerButtonWidth + 5 }, ExpandingContainer.TRANSITION_DURATION, Easing.OutQuint);
                mainButton.Icon.MoveToX(expanded.Value ? 10 : 2.5f, ExpandingContainer.TRANSITION_DURATION, Easing.OutQuint);
            }
            else
            {
                pickerButton.ResizeWidthTo(0, ExpandingContainer.TRANSITION_DURATION, Easing.OutQuint);

                mainButtonContainer.TransformTo(nameof(mainButtonContainer.Padding), new MarginPadding(), ExpandingContainer.TRANSITION_DURATION, Easing.OutQuint);
                mainButton.Icon.MoveToX(10, ExpandingContainer.TRANSITION_DURATION, Easing.OutQuint);
            }
        }

        private partial class ColourPickerButton : OsuButton, IHasPopover
        {
            public BindableList<Colour4> ComboColours { get; } = new BindableList<Colour4>();
            public Bindable<IHasComboInformation?> SelectedHitObject { get; } = new Bindable<IHasComboInformation?>();

            [Resolved]
            private EditorBeatmap editorBeatmap { get; set; } = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public SpriteIcon Icon { get; private set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Add(Icon = new SpriteIcon
                {
                    Icon = FontAwesome.Solid.Palette,
                    Size = new Vector2(16),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

                Action = this.ShowPopover;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                ComboColours.BindCollectionChanged((_, _) => updateState());
                SelectedHitObject.BindValueChanged(val =>
                {
                    if (val.OldValue != null)
                        val.OldValue.ComboIndexWithOffsetsBindable.ValueChanged -= onComboIndexChanged;

                    updateState();

                    if (val.NewValue != null)
                        val.NewValue.ComboIndexWithOffsetsBindable.ValueChanged += onComboIndexChanged;
                }, true);
            }

            private void onComboIndexChanged(ValueChangedEvent<int> _) => updateState();

            private void updateState()
            {
                Enabled.Value = SelectedHitObject.Value != null;

                if (SelectedHitObject.Value == null || SelectedHitObject.Value.ComboOffset == 0 || ComboColours.Count <= 1 || !SelectedHitObject.Value.NewCombo)
                {
                    BackgroundColour = colourProvider.Background3;
                    Icon.Colour = BackgroundColour.Darken(0.5f);
                    Icon.Blending = BlendingParameters.Additive;
                }
                else
                {
                    BackgroundColour = ComboColours[comboIndexFor(SelectedHitObject.Value, ComboColours)];
                    Icon.Colour = OsuColour.ForegroundTextColourFor(BackgroundColour);
                    Icon.Blending = BlendingParameters.Inherit;
                }
            }

            public Popover GetPopover() => new ComboColourPalettePopover(ComboColours, SelectedHitObject.Value.AsNonNull(), editorBeatmap);
        }

        private partial class ComboColourPalettePopover : OsuPopover
        {
            private readonly IReadOnlyList<Colour4> comboColours;
            private readonly IHasComboInformation hasComboInformation;
            private readonly EditorBeatmap editorBeatmap;

            public ComboColourPalettePopover(IReadOnlyList<Colour4> comboColours, IHasComboInformation hasComboInformation, EditorBeatmap editorBeatmap)
            {
                this.comboColours = comboColours;
                this.hasComboInformation = hasComboInformation;
                this.editorBeatmap = editorBeatmap;

                AllowableAnchors = [Anchor.CentreRight];
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Debug.Assert(comboColours.Count > 0);
                var hitObject = hasComboInformation as HitObject;
                Debug.Assert(hitObject != null);

                FillFlowContainer container;

                Child = container = new FillFlowContainer
                {
                    Width = 230,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                };

                int selectedColourIndex = comboIndexFor(hasComboInformation, comboColours);

                for (int i = 0; i < comboColours.Count; i++)
                {
                    int index = i;

                    if (getPreviousHitObjectWithCombo(editorBeatmap, hitObject) is IHasComboInformation previousHasCombo
                        && index == comboIndexFor(previousHasCombo, comboColours)
                        && !canReuseLastComboColour(editorBeatmap, hitObject))
                    {
                        continue;
                    }

                    container.Add(new OsuClickableContainer
                    {
                        Size = new Vector2(50),
                        Masking = true,
                        CornerRadius = 25,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = comboColours[index],
                            },
                            selectedColourIndex == index
                                ? new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.Check,
                                    Size = new Vector2(24),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = OsuColour.ForegroundTextColourFor(comboColours[index]),
                                }
                                : Empty()
                        },
                        Action = () =>
                        {
                            int comboDifference = index - selectedColourIndex;
                            if (comboDifference == 0)
                                return;

                            int newOffset = hasComboInformation.ComboOffset + comboDifference;
                            // `newOffset` must be positive to serialise correctly - this implements the true math "modulus" rather than the built-in "remainder" % op
                            // which can return negative results when the first operand is negative
                            newOffset -= (int)Math.Floor((double)newOffset / comboColours.Count) * comboColours.Count;

                            hasComboInformation.ComboOffset = newOffset;
                            editorBeatmap.BeginChange();
                            editorBeatmap.Update((HitObject)hasComboInformation);
                            editorBeatmap.EndChange();
                            this.HidePopover();
                        }
                    });
                }
            }

            private static IHasComboInformation? getPreviousHitObjectWithCombo(EditorBeatmap editorBeatmap, HitObject hitObject)
                => editorBeatmap.HitObjects.TakeWhile(ho => ho != hitObject).LastOrDefault() as IHasComboInformation;

            private static bool canReuseLastComboColour(EditorBeatmap editorBeatmap, HitObject hitObject)
            {
                double? closestBreakEnd = editorBeatmap.Breaks.Select(b => b.EndTime)
                                                       .Where(t => t <= hitObject.StartTime)
                                                       .OrderBy(t => t)
                                                       .LastOrDefault();

                if (closestBreakEnd == null)
                    return false;

                return editorBeatmap.HitObjects.FirstOrDefault(ho => ho.StartTime >= closestBreakEnd) == hitObject;
            }
        }

        // compare `EditorBeatmapSkin.updateColours()` et al. for reasoning behind the off-by-one index rotation
        private static int comboIndexFor(IHasComboInformation hasComboInformation, IReadOnlyCollection<Colour4> comboColours)
            => (hasComboInformation.ComboIndexWithOffsets + comboColours.Count - 1) % comboColours.Count;
    }
}
