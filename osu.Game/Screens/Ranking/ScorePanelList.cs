// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public class ScorePanelList : CompositeDrawable
    {
        /// <summary>
        /// Normal spacing between all panels.
        /// </summary>
        private const float panel_spacing = 5;

        /// <summary>
        /// Spacing around both sides of the expanded panel. This is added on top of <see cref="panel_spacing"/>.
        /// </summary>
        private const float expanded_panel_spacing = 15;

        public readonly Bindable<ScoreInfo> SelectedScore = new Bindable<ScoreInfo>();

        private readonly Container<ScorePanel> panels;
        private readonly Flow flow;
        private readonly Scroll scroll;
        private ScorePanel expandedPanel;

        /// <summary>
        /// Creates a new <see cref="ScorePanelList"/>.
        /// </summary>
        /// <param name="panelTarget">The target container in which <see cref="ScorePanel"/>s should reside.
        /// <see cref="ScorePanel"/>s are set to track by default, but this allows
        /// This should be placed _before_ the <see cref="ScorePanelList"/> in the hierarchy.
        /// <remarks></remarks>
        /// </param>
        public ScorePanelList(Container<ScorePanel> panelTarget = null)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = scroll = new Scroll
            {
                RelativeSizeAxes = Axes.Both,
                Child = flow = new Flow
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(panel_spacing, 0),
                    AutoSizeAxes = Axes.Both,
                }
            };

            if (panelTarget == null)
            {
                // To prevent 1-frame sizing issues, the panel container is added _before_ the scroll + flow containers
                AddInternal(panels = new Container<ScorePanel>
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1
                });
            }
            else
                panels = panelTarget;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedScore.BindValueChanged(selectedScoreChanged, true);
        }

        /// <summary>
        /// Adds a <see cref="ScoreInfo"/> to this list.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to add.</param>
        public void AddScore(ScoreInfo score)
        {
            var panel = new ScorePanel(score)
            {
                Tracking = true
            }.With(p =>
            {
                p.StateChanged += s =>
                {
                    if (s == PanelState.Expanded)
                        SelectedScore.Value = p.Score;
                };
            });

            panels.Add(panel);
            flow.Add(panel.CreateTrackingComponent().With(d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
            }));

            if (SelectedScore.Value == score)
                selectedScoreChanged(new ValueChangedEvent<ScoreInfo>(SelectedScore.Value, SelectedScore.Value));
            else
            {
                // We want the scroll position to remain relative to the expanded panel. When a new panel is added after the expanded panel, nothing needs to be done.
                // But when a panel is added before the expanded panel, we need to offset the scroll position by the width of the new panel.
                if (expandedPanel != null && flow.GetPanelIndex(score) < flow.GetPanelIndex(expandedPanel.Score))
                {
                    // A somewhat hacky property is used here because we need to:
                    // 1) Scroll after the scroll container's visible range is updated.
                    // 2) Scroll before the scroll container's scroll position is updated.
                    // Without this, we would have a 1-frame positioning error which looks very jarring.
                    scroll.InstantScrollTarget = (scroll.InstantScrollTarget ?? scroll.Target) + ScorePanel.CONTRACTED_WIDTH + panel_spacing;
                }
            }
        }

        /// <summary>
        /// Brings a <see cref="ScoreInfo"/> to the centre of the screen and expands it.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to present.</param>
        private void selectedScoreChanged(ValueChangedEvent<ScoreInfo> score)
        {
            // Contract the old panel.
            foreach (var t in flow.Where(t => t.Panel.Score == score.OldValue))
            {
                t.Panel.State = PanelState.Contracted;
                t.Margin = new MarginPadding();
            }

            // Find the panel corresponding to the new score.
            var expandedTrackingComponent = flow.SingleOrDefault(t => t.Panel.Score == score.NewValue);
            expandedPanel = expandedTrackingComponent?.Panel;

            // handle horizontal scroll only when not hovering the expanded panel.
            scroll.HandleScroll = () => expandedPanel?.IsHovered != true;

            if (expandedPanel == null)
                return;

            Debug.Assert(expandedTrackingComponent != null);

            // Expand the new panel.
            expandedTrackingComponent.Margin = new MarginPadding { Horizontal = expanded_panel_spacing };
            expandedPanel.State = PanelState.Expanded;

            // Scroll to the new panel. This is done manually since we need:
            // 1) To scroll after the scroll container's visible range is updated.
            // 2) To account for the centre anchor/origins of panels.
            // In the end, it's easier to compute the scroll position manually.
            float scrollOffset = flow.GetPanelIndex(expandedPanel.Score) * (ScorePanel.CONTRACTED_WIDTH + panel_spacing);
            scroll.ScrollTo(scrollOffset);
        }

        protected override void Update()
        {
            base.Update();

            float offset = DrawWidth / 2f;

            // Add padding to both sides such that the centre of an expanded panel on either side is in the middle of the screen.

            if (SelectedScore.Value != null)
            {
                // The expanded panel has extra padding applied to it, so it needs to be included into the offset.
                offset -= ScorePanel.EXPANDED_WIDTH / 2f + expanded_panel_spacing;
            }
            else
                offset -= ScorePanel.CONTRACTED_WIDTH / 2f;

            flow.Padding = new MarginPadding { Horizontal = offset };
        }

        private class Flow : FillFlowContainer<ScorePanel.TrackingComponent>
        {
            public override IEnumerable<Drawable> FlowingChildren => applySorting(AliveInternalChildren);

            public int GetPanelIndex(ScoreInfo score) => applySorting(Children).TakeWhile(s => s.Panel.Score != score).Count();

            private IEnumerable<ScorePanel.TrackingComponent> applySorting(IEnumerable<Drawable> drawables) => drawables.OfType<ScorePanel.TrackingComponent>()
                                                                                                                        .OrderByDescending(s => s.Panel.Score.TotalScore)
                                                                                                                        .ThenBy(s => s.Panel.Score.OnlineScoreID);
        }

        private class Scroll : OsuScrollContainer
        {
            public new float Target => base.Target;

            public Scroll()
                : base(Direction.Horizontal)
            {
            }

            /// <summary>
            /// The target that will be scrolled to instantaneously next frame.
            /// </summary>
            public float? InstantScrollTarget;

            /// <summary>
            /// Whether this container should handle scroll trigger events.
            /// </summary>
            public Func<bool> HandleScroll;

            protected override void UpdateAfterChildren()
            {
                if (InstantScrollTarget != null)
                {
                    ScrollTo(InstantScrollTarget.Value, false);
                    InstantScrollTarget = null;
                }

                base.UpdateAfterChildren();
            }

            public override bool HandlePositionalInput => HandleScroll();

            public override bool HandleNonPositionalInput => HandleScroll();
        }
    }
}
