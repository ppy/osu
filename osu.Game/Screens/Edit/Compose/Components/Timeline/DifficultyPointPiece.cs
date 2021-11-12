// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
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
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class DifficultyPointPiece : HitObjectPointPiece, IHasPopover
    {
        public readonly HitObject HitObject;

        private readonly BindableNumber<double> speedMultiplier;

        public DifficultyPointPiece(HitObject hitObject)
            : base(hitObject.DifficultyControlPoint)
        {
            HitObject = hitObject;

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

        public Popover GetPopover() => new DifficultyEditPopover(HitObject);

        public class DifficultyEditPopover : OsuPopover
        {
            private readonly HitObject hitObject;

            private IndeterminateSliderWithTextBoxInput<double> sliderVelocitySlider;

            [Resolved(canBeNull: true)]
            private EditorBeatmap beatmap { get; set; }

            public DifficultyEditPopover(HitObject hitObject)
            {
                this.hitObject = hitObject;
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
                        Spacing = new Vector2(0, 15),
                        Children = new Drawable[]
                        {
                            sliderVelocitySlider = new IndeterminateSliderWithTextBoxInput<double>("Velocity", new DifficultyControlPoint().SliderVelocityBindable)
                            {
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

                // if the piece belongs to a currently selected object, assume that the user wants to change all selected objects.
                // if the piece belongs to an unselected object, operate on that object alone, independently of the selection.
                var relevantObjects = (beatmap.SelectedHitObjects.Contains(hitObject) ? beatmap.SelectedHitObjects : hitObject.Yield()).ToArray();
                var relevantControlPoints = relevantObjects.Select(h => h.DifficultyControlPoint).ToArray();

                // even if there are multiple objects selected, we can still display a value if they all have the same value.
                var selectedPointBindable = relevantControlPoints.Select(point => point.SliderVelocity).Distinct().Count() == 1 ? relevantControlPoints.First().SliderVelocityBindable : null;

                if (selectedPointBindable != null)
                {
                    // there may be legacy control points, which contain infinite precision for compatibility reasons (see LegacyDifficultyControlPoint).
                    // generally that level of precision could only be set by externally editing the .osu file, so at the point
                    // a user is looking to update this within the editor it should be safe to obliterate this additional precision.
                    sliderVelocitySlider.Current.Value = selectedPointBindable.Value;
                }

                sliderVelocitySlider.Current.BindValueChanged(val =>
                {
                    if (val.NewValue == null)
                        return;

                    beatmap.BeginChange();

                    foreach (var h in relevantObjects)
                    {
                        h.DifficultyControlPoint.SliderVelocity = val.NewValue.Value;
                        beatmap.Update(h);
                    }

                    beatmap.EndChange();
                });
            }
        }
    }
}
