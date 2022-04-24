// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit
{
    /// <summary>
    /// A toolbox composite for osu!-specific controls.
    /// </summary>
    // todo: once catch supports distance spacing, the control here should move out to a base "DistancingRulesetToolboxComposite" class or something better.
    public class OsuToolboxComposite : CompositeDrawable
    {
        private ExpandingToolboxContainer expandingContainer;
        private ExpandableSlider<double, SizeSlider<double>> distanceSpacingSlider;

        private readonly Bindable<double> distanceSpacing = new BindableDouble(1.0)
        {
            MinValue = 0.1,
            MaxValue = 6.0,
            Precision = 0.01,
        };

        public IBindable<double> DistanceSpacing => distanceSpacing;

        private bool distanceSpacingScrollActive;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = expandingContainer = new ExpandingToolboxContainer
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Child = new EditorToolboxGroup("snapping")
                {
                    Child = distanceSpacingSlider = new ExpandableSlider<double, SizeSlider<double>>
                    {
                        Current = { BindTarget = distanceSpacing },
                        KeyboardStep = 0.1f,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            distanceSpacing.Value = editorBeatmap.BeatmapInfo.DistanceSpacing;
            distanceSpacing.BindValueChanged(v =>
            {
                distanceSpacingSlider.ContractedLabelText = $"D. S. ({v.NewValue:0.##x})";
                distanceSpacingSlider.ExpandedLabelText = $"Distance Spacing ({v.NewValue:0.##x})";
                editorBeatmap.BeatmapInfo.DistanceSpacing = v.NewValue;
            }, true);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.AltPressed && e.Key == Key.D && !e.Repeat)
            {
                expandingContainer.Expanded.Value = true;
                distanceSpacingScrollActive = true;
                return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (distanceSpacingScrollActive && (!e.AltPressed || e.Key == Key.D))
            {
                expandingContainer.Expanded.Value = false;
                distanceSpacingScrollActive = false;
            }
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (distanceSpacingScrollActive)
            {
                distanceSpacing.Value += e.ScrollDelta.Y * (e.IsPrecise ? 0.01f : 0.1f);
                return true;
            }

            return base.OnScroll(e);
        }

        private class ExpandingToolboxContainer : ExpandingContainer
        {
            protected override double HoverExpansionDelay => 250;

            public ExpandingToolboxContainer()
                : base(130, 250)
            {
                RelativeSizeAxes = Axes.Y;
                Padding = new MarginPadding { Left = 10 };

                FillFlow.Spacing = new Vector2(10);
            }
        }
    }
}
