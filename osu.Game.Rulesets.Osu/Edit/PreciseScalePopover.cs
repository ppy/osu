// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class PreciseScalePopover : OsuPopover
    {
        private readonly SelectionScaleHandler scaleHandler;

        private readonly Bindable<PreciseScaleInfo> scaleInfo = new Bindable<PreciseScaleInfo>(new PreciseScaleInfo(1, ScaleOrigin.PlayfieldCentre, true, true));

        private SliderWithTextBoxInput<float> scaleInput = null!;
        private BindableNumber<float> scaleInputBindable = null!;
        private EditorRadioButtonCollection scaleOrigin = null!;

        private RadioButton selectionCentreButton = null!;

        public PreciseScalePopover(SelectionScaleHandler scaleHandler)
        {
            this.scaleHandler = scaleHandler;

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
                            new RadioButton("Playfield centre",
                                () => setOrigin(ScaleOrigin.PlayfieldCentre),
                                () => new SpriteIcon { Icon = FontAwesome.Regular.Square }),
                            selectionCentreButton = new RadioButton("Selection centre",
                                () => setOrigin(ScaleOrigin.SelectionCentre),
                                () => new SpriteIcon { Icon = FontAwesome.Solid.VectorSquare })
                        }
                    }
                }
            };
            selectionCentreButton.Selected.DisabledChanged += isDisabled =>
            {
                selectionCentreButton.TooltipText = isDisabled ? "Select more than one object to perform selection-based scaling." : string.Empty;
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
            scaleOrigin.Items.First().Select();

            scaleHandler.CanScaleX.BindValueChanged(e =>
            {
                selectionCentreButton.Selected.Disabled = !e.NewValue;
            }, true);

            scaleInfo.BindValueChanged(scale =>
            {
                var newScale = new Vector2(scale.NewValue.XAxis ? scale.NewValue.Scale : 1, scale.NewValue.YAxis ? scale.NewValue.Scale : 1);
                scaleHandler.Update(newScale, getOriginPosition(scale.NewValue));
            });
        }

        private void updateMaxScale()
        {
            if (!scaleHandler.OriginalSurroundingQuad.HasValue)
                return;

            const float max_scale = 10;
            var scale = scaleHandler.GetClampedScale(new Vector2(max_scale), getOriginPosition(scaleInfo.Value));

            if (!scaleInfo.Value.XAxis)
                scale.X = max_scale;
            if (!scaleInfo.Value.YAxis)
                scale.Y = max_scale;

            scaleInputBindable.MaxValue = MathF.Max(1, MathF.Min(scale.X, scale.Y));
        }

        private void setOrigin(ScaleOrigin origin)
        {
            scaleInfo.Value = scaleInfo.Value with { Origin = origin };
            updateMaxScale();
        }

        private Vector2? getOriginPosition(PreciseScaleInfo scale) => scale.Origin == ScaleOrigin.PlayfieldCentre ? OsuPlayfield.BASE_SIZE / 2 : null;

        protected override void PopIn()
        {
            base.PopIn();
            scaleHandler.Begin();
            updateMaxScale();
        }

        protected override void PopOut()
        {
            base.PopOut();

            if (IsLoaded)
                scaleHandler.Commit();
        }
    }

    public enum ScaleOrigin
    {
        PlayfieldCentre,
        SelectionCentre
    }

    public record PreciseScaleInfo(float Scale, ScaleOrigin Origin, bool XAxis, bool YAxis);
}
