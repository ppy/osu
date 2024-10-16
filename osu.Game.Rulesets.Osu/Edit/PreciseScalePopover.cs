// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class PreciseScalePopover : OsuPopover
    {
        private readonly OsuSelectionScaleHandler scaleHandler;

        private readonly OsuGridToolboxGroup gridToolbox;

        private readonly Bindable<PreciseScaleInfo> scaleInfo = new Bindable<PreciseScaleInfo>(new PreciseScaleInfo(1, ScaleOrigin.GridCentre, true, true));

        private SliderWithTextBoxInput<float> scaleInput = null!;
        private BindableNumber<float> scaleInputBindable = null!;
        private EditorRadioButtonCollection scaleOrigin = null!;

        private RadioButton gridCentreButton = null!;
        private RadioButton playfieldCentreButton = null!;
        private RadioButton selectionCentreButton = null!;

        private OsuCheckbox xCheckBox = null!;
        private OsuCheckbox yCheckBox = null!;

        private BindableList<HitObject> selectedItems { get; } = new BindableList<HitObject>();

        public PreciseScalePopover(OsuSelectionScaleHandler scaleHandler, OsuGridToolboxGroup gridToolbox)
        {
            this.scaleHandler = scaleHandler;
            this.gridToolbox = gridToolbox;

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };
        }

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap editorBeatmap)
        {
            selectedItems.BindTo(editorBeatmap.SelectedHitObjects);

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
                            MinValue = 0.5f,
                            MaxValue = 2,
                            Precision = 0.001f,
                            Value = 1,
                            Default = 1,
                        },
                        Instantaneous = true
                    },
                    scaleOrigin = new EditorRadioButtonCollection
                    {
                        RelativeSizeAxes = Axes.X,
                        Items = new[]
                        {
                            gridCentreButton = new RadioButton("Grid centre",
                                () => setOrigin(ScaleOrigin.GridCentre),
                                () => new SpriteIcon { Icon = FontAwesome.Regular.PlusSquare }),
                            playfieldCentreButton = new RadioButton("Playfield centre",
                                () => setOrigin(ScaleOrigin.PlayfieldCentre),
                                () => new SpriteIcon { Icon = FontAwesome.Regular.Square }),
                            selectionCentreButton = new RadioButton("Selection centre",
                                () => setOrigin(ScaleOrigin.SelectionCentre),
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

            ScheduleAfterChildren(() =>
            {
                scaleInput.TakeFocus();
                scaleInput.SelectAll();
            });
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

            scaleOrigin.Items.First(b => !b.Selected.Disabled).Select();

            scaleInfo.BindValueChanged(scale =>
            {
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
            if (scaleInfo.Value.Origin != ScaleOrigin.SelectionCentre)
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

            const float min_scale = 0.5f;
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

        private void setOrigin(ScaleOrigin origin)
        {
            scaleInfo.Value = scaleInfo.Value with { Origin = origin };
            updateMinMaxScale();
            updateAxisCheckBoxesEnabled();
        }

        private Vector2? getOriginPosition(PreciseScaleInfo scale)
        {
            switch (scale.Origin)
            {
                case ScaleOrigin.GridCentre:
                    return gridToolbox.StartPosition.Value;

                case ScaleOrigin.PlayfieldCentre:
                    return OsuPlayfield.BASE_SIZE / 2;

                case ScaleOrigin.SelectionCentre:
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

        private float getRotation(PreciseScaleInfo scale) => scale.Origin == ScaleOrigin.GridCentre ? gridToolbox.GridLinesRotation.Value : 0;

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
    }

    public enum ScaleOrigin
    {
        GridCentre,
        PlayfieldCentre,
        SelectionCentre
    }

    public record PreciseScaleInfo(float Scale, ScaleOrigin Origin, bool XAxis, bool YAxis);
}
