// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class PreciseScalePopover : OsuPopover
    {
        private readonly OsuSelectionScaleHandler scaleHandler;

        private readonly OsuGridToolboxGroup gridToolbox;

        private readonly Bindable<PreciseScaleInfo> scaleInfo = new Bindable<PreciseScaleInfo>(new PreciseScaleInfo(1, EditorOrigin.GridCentre, true, true));

        private SliderWithTextBoxInput<float> scaleInput = null!;
        private BindableNumber<float> scaleInputBindable = null!;
        private EditorRadioButtonCollection scaleOrigin = null!;

        private RadioButton gridCentreButton = null!;
        private RadioButton playfieldCentreButton = null!;
        private RadioButton selectionCentreButton = null!;

        private OsuCheckbox xCheckBox = null!;
        private OsuCheckbox yCheckBox = null!;

        private Bindable<EditorOrigin> configScaleOrigin = null!;

        private BindableList<HitObject> selectedItems { get; } = new BindableList<HitObject>();

        public PreciseScalePopover(OsuSelectionScaleHandler scaleHandler, OsuGridToolboxGroup gridToolbox)
        {
            this.scaleHandler = scaleHandler;
            this.gridToolbox = gridToolbox;

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };
        }

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap editorBeatmap, OsuConfigManager config)
        {
            selectedItems.BindTo(editorBeatmap.SelectedHitObjects);

            configScaleOrigin = config.GetBindable<EditorOrigin>(OsuSetting.EditorScaleOrigin);

            Child = new FillFlowContainer
            {
                Width = 220,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    scaleInput = new SliderWithTextBoxInput<float>("Scale:")
                    {
                        Current = scaleInputBindable = new BindableNumber<float>
                        {
                            MinValue = 0.05f,
                            MaxValue = 2,
                            Precision = 0.001f,
                            Value = 1,
                            Default = 1,
                        },
                        KeyboardStep = 0.01f,
                        Instantaneous = true
                    },
                    scaleOrigin = new EditorRadioButtonCollection
                    {
                        RelativeSizeAxes = Axes.X,
                        Items = new[]
                        {
                            gridCentreButton = new RadioButton("Grid centre",
                                () => setOrigin(EditorOrigin.GridCentre),
                                () => new SpriteIcon { Icon = FontAwesome.Regular.PlusSquare }),
                            playfieldCentreButton = new RadioButton("Playfield centre",
                                () => setOrigin(EditorOrigin.PlayfieldCentre),
                                () => new SpriteIcon { Icon = FontAwesome.Regular.Square }),
                            selectionCentreButton = new RadioButton("Selection centre",
                                () => setOrigin(EditorOrigin.SelectionCentre),
                                () => new SpriteIcon { Icon = FontAwesome.Solid.VectorSquare })
                        }
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(4),
                        Children = new Drawable[]
                        {
                            xCheckBox = new OsuCheckbox(false)
                            {
                                RelativeSizeAxes = Axes.X,
                                LabelText = "X-axis",
                                Current = { Value = true },
                            },
                            yCheckBox = new OsuCheckbox(false)
                            {
                                RelativeSizeAxes = Axes.X,
                                LabelText = "Y-axis",
                                Current = { Value = true },
                            },
                        }
                    },
                }
            };
            gridCentreButton.Selected.DisabledChanged += isDisabled =>
            {
                gridCentreButton.TooltipText = isDisabled ? "The current selection cannot be scaled relative to grid centre." : string.Empty;
            };
            playfieldCentreButton.Selected.DisabledChanged += isDisabled =>
            {
                playfieldCentreButton.TooltipText = isDisabled ? "The current selection cannot be scaled relative to playfield centre." : string.Empty;
            };
            selectionCentreButton.Selected.DisabledChanged += isDisabled =>
            {
                selectionCentreButton.TooltipText = isDisabled ? "The current selection cannot be scaled relative to its centre." : string.Empty;
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => scaleInput.TakeFocus());
            scaleInput.Current.BindValueChanged(scale => scaleInfo.Value = scaleInfo.Value with { Scale = scale.NewValue });

            xCheckBox.Current.BindValueChanged(_ =>
            {
                if (!xCheckBox.Current.Value && !yCheckBox.Current.Value)
                {
                    yCheckBox.Current.Value = true;
                    return;
                }

                updateAxes();
            });
            yCheckBox.Current.BindValueChanged(_ =>
            {
                if (!xCheckBox.Current.Value && !yCheckBox.Current.Value)
                {
                    xCheckBox.Current.Value = true;
                    return;
                }

                updateAxes();
            });

            selectionCentreButton.Selected.Disabled = !(scaleHandler.CanScaleX.Value || scaleHandler.CanScaleY.Value);
            playfieldCentreButton.Selected.Disabled = scaleHandler.IsScalingSlider.Value && !selectionCentreButton.Selected.Disabled;
            gridCentreButton.Selected.Disabled = playfieldCentreButton.Selected.Disabled;

            bool didSelect = false;

            configScaleOrigin.BindValueChanged(val =>
            {
                switch (configScaleOrigin.Value)
                {
                    case EditorOrigin.GridCentre:
                        if (!gridCentreButton.Selected.Disabled)
                        {
                            gridCentreButton.Select();
                            didSelect = true;
                        }

                        break;

                    case EditorOrigin.PlayfieldCentre:
                        if (!playfieldCentreButton.Selected.Disabled)
                        {
                            playfieldCentreButton.Select();
                            didSelect = true;
                        }

                        break;

                    case EditorOrigin.SelectionCentre:
                        if (!selectionCentreButton.Selected.Disabled)
                        {
                            selectionCentreButton.Select();
                            didSelect = true;
                        }

                        break;
                }
            }, true);

            if (!didSelect)
                scaleOrigin.Items.First(b => !b.Selected.Disabled).Select();

            gridCentreButton.Selected.BindValueChanged(b =>
            {
                if (b.NewValue) configScaleOrigin.Value = EditorOrigin.GridCentre;
            });
            playfieldCentreButton.Selected.BindValueChanged(b =>
            {
                if (b.NewValue) configScaleOrigin.Value = EditorOrigin.PlayfieldCentre;
            });
            selectionCentreButton.Selected.BindValueChanged(b =>
            {
                if (b.NewValue) configScaleOrigin.Value = EditorOrigin.SelectionCentre;
            });

            scaleInfo.BindValueChanged(scale =>
            {
                // can happen if the popover is dismissed by a keyboard key press while dragging UI controls
                if (!scaleHandler.OperationInProgress.Value)
                    return;

                var newScale = new Vector2(scale.NewValue.Scale, scale.NewValue.Scale);
                scaleHandler.Update(newScale, getOriginPosition(scale.NewValue), getAdjustAxis(scale.NewValue), getRotation(scale.NewValue));
            });
        }

        private void updateAxes()
        {
            scaleInfo.Value = scaleInfo.Value with { XAxis = xCheckBox.Current.Value, YAxis = yCheckBox.Current.Value };
            updateMinMaxScale();
        }

        private void updateAxisCheckBoxesEnabled()
        {
            if (scaleInfo.Value.Origin != EditorOrigin.SelectionCentre)
            {
                toggleAxisAvailable(xCheckBox.Current, true);
                toggleAxisAvailable(yCheckBox.Current, true);
            }
            else
            {
                toggleAxisAvailable(xCheckBox.Current, scaleHandler.CanScaleX.Value);
                toggleAxisAvailable(yCheckBox.Current, scaleHandler.CanScaleY.Value);
            }
        }

        private void toggleAxisAvailable(Bindable<bool> axisBindable, bool available)
        {
            // enable the bindable to allow setting the value
            axisBindable.Disabled = false;
            // restore the presumed default value given the axis's new availability state
            axisBindable.Value = available;
            axisBindable.Disabled = !available;
        }

        private void updateMinMaxScale()
        {
            if (!scaleHandler.OriginalSurroundingQuad.HasValue)
                return;

            const float min_scale = 0.05f;
            const float max_scale = 10;

            var scale = scaleHandler.ClampScaleToPlayfieldBounds(new Vector2(max_scale), getOriginPosition(scaleInfo.Value), getAdjustAxis(scaleInfo.Value), getRotation(scaleInfo.Value));

            if (!scaleInfo.Value.XAxis)
                scale.X = max_scale;
            if (!scaleInfo.Value.YAxis)
                scale.Y = max_scale;

            scaleInputBindable.MaxValue = MathF.Max(1, MathF.Min(scale.X, scale.Y));

            scale = scaleHandler.ClampScaleToPlayfieldBounds(new Vector2(min_scale), getOriginPosition(scaleInfo.Value), getAdjustAxis(scaleInfo.Value), getRotation(scaleInfo.Value));

            if (!scaleInfo.Value.XAxis)
                scale.X = min_scale;
            if (!scaleInfo.Value.YAxis)
                scale.Y = min_scale;

            scaleInputBindable.MinValue = MathF.Min(1, MathF.Max(scale.X, scale.Y));
        }

        private void setOrigin(EditorOrigin origin)
        {
            scaleInfo.Value = scaleInfo.Value with { Origin = origin };
            updateMinMaxScale();
            updateAxisCheckBoxesEnabled();
        }

        private Vector2? getOriginPosition(PreciseScaleInfo scale)
        {
            switch (scale.Origin)
            {
                case EditorOrigin.GridCentre:
                    return gridToolbox.StartPosition.Value;

                case EditorOrigin.PlayfieldCentre:
                    return OsuPlayfield.BASE_SIZE / 2;

                case EditorOrigin.SelectionCentre:
                    if (selectedItems.Count == 1 && selectedItems.First() is Slider slider)
                        return slider.Position;

                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scale));
            }
        }

        private Axes getAdjustAxis(PreciseScaleInfo scale)
        {
            var result = Axes.None;

            if (scale.XAxis)
                result |= Axes.X;

            if (scale.YAxis)
                result |= Axes.Y;

            return result;
        }

        private float getRotation(PreciseScaleInfo scale) => scale.Origin == EditorOrigin.GridCentre ? gridToolbox.GridLinesRotation.Value : 0;

        protected override void PopIn()
        {
            base.PopIn();
            scaleHandler.Begin();
            updateMinMaxScale();
        }

        protected override void PopOut()
        {
            base.PopOut();

            if (IsLoaded) scaleHandler.Commit();
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.Select && !e.Repeat)
            {
                this.HidePopover();
                return true;
            }

            return base.OnPressed(e);
        }
    }

    public record PreciseScaleInfo(float Scale, EditorOrigin Origin, bool XAxis, bool YAxis);
}
