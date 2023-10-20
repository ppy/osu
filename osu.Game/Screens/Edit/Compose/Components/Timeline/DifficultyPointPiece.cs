// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Timing;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class DifficultyPointPiece : HitObjectPointPiece, IHasPopover
    {
        public readonly HitObject HitObject;

        private readonly BindableNumber<double> speedMultiplier;

        public DifficultyPointPiece(HitObject hitObject)
        {
            HitObject = hitObject;

            speedMultiplier = (hitObject as IHasSliderVelocity)?.SliderVelocityMultiplierBindable.GetBoundCopy();
        }

        protected override Color4 GetRepresentingColour(OsuColour colours) => colours.Lime1;

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

        public partial class DifficultyEditPopover : OsuPopover
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
                            sliderVelocitySlider = new IndeterminateSliderWithTextBoxInput<double>("Velocity", new BindableDouble(1)
                            {
                                Precision = 0.01,
                                MinValue = 0.1,
                                MaxValue = 10
                            })
                            {
                                KeyboardStep = 0.1f
                            },
                            new OsuTextFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Text = "Hold shift while dragging the end of an object to adjust velocity while snapping."
                            },
                            new SliderVelocityInspector(sliderVelocitySlider.Current),
                        }
                    }
                };

                // if the piece belongs to a currently selected object, assume that the user wants to change all selected objects.
                // if the piece belongs to an unselected object, operate on that object alone, independently of the selection.
                var relevantObjects = (beatmap.SelectedHitObjects.Contains(hitObject) ? beatmap.SelectedHitObjects : hitObject.Yield()).Where(o => o is IHasSliderVelocity).ToArray();

                // even if there are multiple objects selected, we can still display a value if they all have the same value.
                var selectedPointBindable = relevantObjects.Select(point => ((IHasSliderVelocity)point).SliderVelocityMultiplier).Distinct().Count() == 1
                    ? ((IHasSliderVelocity)relevantObjects.First()).SliderVelocityMultiplierBindable
                    : null;

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
                        ((IHasSliderVelocity)h).SliderVelocityMultiplier = val.NewValue.Value;
                        beatmap.Update(h);
                    }

                    beatmap.EndChange();
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                ScheduleAfterChildren(() => GetContainingInputManager().ChangeFocus(sliderVelocitySlider));
            }
        }
    }

    internal partial class SliderVelocityInspector : EditorInspector
    {
        private readonly Bindable<double?> current;

        public SliderVelocityInspector(Bindable<double?> current)
        {
            this.current = current;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            EditorBeatmap.TransactionBegan += updateInspectorText;
            EditorBeatmap.TransactionEnded += updateInspectorText;
            EditorBeatmap.BeatmapReprocessed += updateInspectorText;
            current.ValueChanged += _ => updateInspectorText();

            updateInspectorText();
        }

        private void updateInspectorText()
        {
            double beatmapVelocity = EditorBeatmap.Difficulty.SliderMultiplier;

            InspectorText.Clear();

            double[] sliderVelocities = EditorBeatmap.HitObjects.OfType<IHasSliderVelocity>().Select(sv => sv.SliderVelocityMultiplier).OrderBy(v => v).ToArray();

            AddHeader("Base velocity (from beatmap setup)");
            AddValue($"{beatmapVelocity:#,0.00}x");

            AddHeader("Final velocity");
            AddValue($"{beatmapVelocity * current.Value:#,0.00}x");

            if (sliderVelocities.Length == 0)
            {
                return;
            }

            if (sliderVelocities.First() != sliderVelocities.Last())
            {
                AddHeader("Beatmap velocity range");

                string range = $"{sliderVelocities.First():#,0.00}x - {sliderVelocities.Last():#,0.00}x";
                if (beatmapVelocity != 1)
                    range += $" ({beatmapVelocity * sliderVelocities.First():#,0.00}x - {beatmapVelocity * sliderVelocities.Last():#,0.00}x)";

                AddValue(range);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            EditorBeatmap.TransactionBegan -= updateInspectorText;
            EditorBeatmap.TransactionEnded -= updateInspectorText;
            EditorBeatmap.BeatmapReprocessed -= updateInspectorText;
        }
    }
}
