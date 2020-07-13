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

        /// <summary>
        /// An action to be invoked if a <see cref="ScorePanel"/> is clicked while in an expanded state.
        /// </summary>
        public Action PostExpandAction;

        public readonly Bindable<ScoreInfo> SelectedScore = new Bindable<ScoreInfo>();

        private readonly Flow flow;
        private readonly Scroll scroll;
        private ScorePanel expandedPanel;

        /// <summary>
        /// Creates a new <see cref="ScorePanelList"/>.
        /// </summary>
        public ScorePanelList()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = scroll = new Scroll
            {
                RelativeSizeAxes = Axes.Both,
                HandleScroll = () => expandedPanel?.IsHovered != true, // handle horizontal scroll only when not hovering the expanded panel.
                Child = flow = new Flow
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(panel_spacing, 0),
                    AutoSizeAxes = Axes.Both,
                }
            };
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
        public ScorePanel AddScore(ScoreInfo score)
        {
            var panel = new ScorePanel(score)
            {
                PostExpandAction = () => PostExpandAction?.Invoke()
            }.With(p =>
            {
                p.StateChanged += s =>
                {
                    if (s == PanelState.Expanded)
                        SelectedScore.Value = p.Score;
                };
            });

            flow.Add(panel.CreateTrackingContainer().With(d =>
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

            return panel;
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

        private bool handleInput = true;

        /// <summary>
        /// Whether this <see cref="ScorePanelList"/> or any of the <see cref="ScorePanel"/>s contained should handle scroll or click input.
        /// Setting to <c>false</c> will also hide the scrollbar.
        /// </summary>
        public bool HandleInput
        {
            get => handleInput;
            set
            {
                handleInput = value;
                scroll.ScrollbarVisible = value;
            }
        }

        public override bool PropagatePositionalInputSubTree => HandleInput && base.PropagatePositionalInputSubTree;

        public override bool PropagateNonPositionalInputSubTree => HandleInput && base.PropagateNonPositionalInputSubTree;

        /// <summary>
        /// Enumerates all <see cref="ScorePanel"/>s contained in this <see cref="ScorePanelList"/>.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScorePanel> GetScorePanels() => flow.Select(t => t.Panel);

        /// <summary>
        /// Finds the <see cref="ScorePanel"/> corresponding to a <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to find the corresponding <see cref="ScorePanel"/> for.</param>
        /// <returns>The <see cref="ScorePanel"/>.</returns>
        public ScorePanel GetPanelForScore(ScoreInfo score) => flow.Single(t => t.Panel.Score == score).Panel;

        /// <summary>
        /// Detaches a <see cref="ScorePanel"/> from its <see cref="ScorePanelTrackingContainer"/>, allowing the panel to be moved elsewhere in the hierarchy.
        /// </summary>
        /// <param name="panel">The <see cref="ScorePanel"/> to detach.</param>
        /// <exception cref="InvalidOperationException">If <paramref name="panel"/> is not a part of this <see cref="ScorePanelList"/>.</exception>
        public void Detach(ScorePanel panel)
        {
            var container = flow.SingleOrDefault(t => t.Panel == panel);
            if (container == null)
                throw new InvalidOperationException("Panel is not contained by the score panel list.");

            container.Detach();
        }

        /// <summary>
        /// Attaches a <see cref="ScorePanel"/> to its <see cref="ScorePanelTrackingContainer"/> in this <see cref="ScorePanelList"/>.
        /// </summary>
        /// <param name="panel">The <see cref="ScorePanel"/> to attach.</param>
        /// <exception cref="InvalidOperationException">If <paramref name="panel"/> is not a part of this <see cref="ScorePanelList"/>.</exception>
        public void Attach(ScorePanel panel)
        {
            var container = flow.SingleOrDefault(t => t.Panel == panel);
            if (container == null)
                throw new InvalidOperationException("Panel is not contained by the score panel list.");

            container.Attach();
        }

        private class Flow : FillFlowContainer<ScorePanelTrackingContainer>
        {
            public override IEnumerable<Drawable> FlowingChildren => applySorting(AliveInternalChildren);

            public int GetPanelIndex(ScoreInfo score) => applySorting(Children).TakeWhile(s => s.Panel.Score != score).Count();

            private IEnumerable<ScorePanelTrackingContainer> applySorting(IEnumerable<Drawable> drawables) => drawables.OfType<ScorePanelTrackingContainer>()
                                                                                                                       .OrderByDescending(s => s.Panel.Score.TotalScore)
                                                                                                                       .ThenBy(s => s.Panel.Score.OnlineScoreID);

            protected override int Compare(Drawable x, Drawable y)
            {
                var tX = (ScorePanelTrackingContainer)x;
                var tY = (ScorePanelTrackingContainer)y;

                int result = tY.Panel.Score.TotalScore.CompareTo(tX.Panel.Score.TotalScore);

                if (result != 0)
                    return result;

                if (tX.Panel.Score.OnlineScoreID == null || tY.Panel.Score.OnlineScoreID == null)
                    return base.Compare(x, y);

                return tX.Panel.Score.OnlineScoreID.Value.CompareTo(tY.Panel.Score.OnlineScoreID.Value);
            }
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
