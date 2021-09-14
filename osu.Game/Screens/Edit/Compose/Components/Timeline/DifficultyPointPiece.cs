// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Timing;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class DifficultyPointPiece : HitObjectPointPiece, IHasPopover
    {
        private readonly HitObject hitObject;

        private readonly BindableNumber<double> speedMultiplier;

        public DifficultyPointPiece(HitObject hitObject)
            : base(hitObject.DifficultyControlPoint)
        {
            this.hitObject = hitObject;

            speedMultiplier = hitObject.DifficultyControlPoint.SliderVelocityBindable.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            speedMultiplier.BindValueChanged(multiplier => Label.Text = $"{multiplier.NewValue:n2}x", true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            this.ShowPopover();
            return true;
        }

        public Popover GetPopover() => new DifficultyEditPopover(hitObject);

        public class DifficultyEditPopover : OsuPopover
        {
            private readonly HitObject hitObject;
            private readonly DifficultyControlPoint point;

            private SliderWithTextBoxInput<double> sliderVelocitySlider;

            [Resolved(canBeNull: true)]
            private EditorBeatmap beatmap { get; set; }

            public DifficultyEditPopover(HitObject hitObject)
            {
                this.hitObject = hitObject;
                point = hitObject.DifficultyControlPoint;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Width = 200,
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            sliderVelocitySlider = new SliderWithTextBoxInput<double>("Velocity")
                            {
                                Current = new DifficultyControlPoint().SliderVelocityBindable,
                                KeyboardStep = 0.1f
                            },
                            new OsuTextFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Text = "Hold shift while dragging the end of an object to adjust velocity while snapping."
                            }
                        }
                    }
                };

                var selectedPointBindable = point.SliderVelocityBindable;

                // there may be legacy control points, which contain infinite precision for compatibility reasons (see LegacyDifficultyControlPoint).
                // generally that level of precision could only be set by externally editing the .osu file, so at the point
                // a user is looking to update this within the editor it should be safe to obliterate this additional precision.
                double expectedPrecision = new DifficultyControlPoint().SliderVelocityBindable.Precision;
                if (selectedPointBindable.Precision < expectedPrecision)
                    selectedPointBindable.Precision = expectedPrecision;

                sliderVelocitySlider.Current = selectedPointBindable;
                sliderVelocitySlider.Current.BindValueChanged(_ => beatmap?.Update(hitObject));
            }
        }
    }
}
