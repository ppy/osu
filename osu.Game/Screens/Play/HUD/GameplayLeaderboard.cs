// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public abstract class GameplayLeaderboard : CompositeDrawable
    {

        private const int duration = 100;

        private readonly Bindable<bool> configVisibility = new Bindable<bool>();
        private readonly Cached sorting = new Cached();

        public Bindable<bool> Expanded = new Bindable<bool>();

        protected readonly FillFlowContainer<GameplayLeaderboardScore> Flow;

        private bool requiresScroll;
        private readonly OsuScrollContainer scroll;

        private GameplayLeaderboardScore? trackedScore;

        private const int max_panels = 8;

        /// <summary>
        /// Create a new leaderboard.
        /// </summary>
        protected GameplayLeaderboard()
        {
            Width = GameplayLeaderboardScore.EXTENDED_WIDTH + GameplayLeaderboardScore.SHEAR_WIDTH;

            InternalChildren = new Drawable[]
            {
                scroll = new InputDisabledScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Flow = new FillFlowContainer<GameplayLeaderboardScore>
                    {
                        RelativeSizeAxes = Axes.X,
                        X = GameplayLeaderboardScore.SHEAR_WIDTH,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(2.5f),
                        LayoutDuration = 450,
                        LayoutEasing = Easing.OutQuint,
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.GameplayLeaderboard, configVisibility);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scheduler.AddDelayed(sort, 1000, true);

            configVisibility.BindValueChanged(_ => updateVisibility(), true);
        }

        /// <summary>
        /// Adds a player to the leaderboard.
        /// </summary>
        /// <param name="user">The player.</param>
        /// <param name="isTracked">
        /// Whether the player should be tracked on the leaderboard.
        /// Set to <c>true</c> for the local player or a player whose replay is currently being played.
        /// </param>
        public ILeaderboardScore Add(IUser? user, bool isTracked)
        {
            var drawable = CreateLeaderboardScoreDrawable(user, isTracked);

            if (isTracked)
            {
                if (trackedScore != null)
                    throw new InvalidOperationException("Cannot track more than one score.");

                trackedScore = drawable;
            }

            drawable.Expanded.BindTo(Expanded);

            Flow.Add(drawable);
            drawable.TotalScore.BindValueChanged(_ => sorting.Invalidate(), true);
            drawable.DisplayOrder.BindValueChanged(_ => sorting.Invalidate(), true);

            int displayCount = Math.Min(Flow.Count, max_panels);
            Height = displayCount * (GameplayLeaderboardScore.PANEL_HEIGHT + Flow.Spacing.Y);
            // Add extra margin space to flow equal to height of leaderboard.
            // This ensures the content is always on screen, but also accounts for the fact that scroll operations
            // without animation were actually forcing the local score to a location it can't usually reside at.
            //
            // Basically, the local score was in the scroll extension region (due to always trying to scroll the
            // local player to the middle of the display, but there being no other content below the local player
            // to scroll up by).
            Flow.Margin = new MarginPadding { Bottom = Height };
            requiresScroll = displayCount != Flow.Count;

            return drawable;
        }

        public void Clear()
        {
            Flow.Clear();
            trackedScore = null;
            scroll.ScrollToStart(false);
        }

        protected virtual GameplayLeaderboardScore CreateLeaderboardScoreDrawable(IUser? user, bool isTracked) =>
            new GameplayLeaderboardScore(user, isTracked);

        protected override void Update()
        {
            base.Update();

            if (requiresScroll && trackedScore != null)
            {
                float scrollTarget = scroll.GetChildPosInContent(trackedScore) + trackedScore.DrawHeight / 2 - scroll.DrawHeight / 2;
                scroll.ScrollTo(scrollTarget);
            }

            const float panel_height = GameplayLeaderboardScore.PANEL_HEIGHT;

            float fadeBottom = scroll.Current + scroll.DrawHeight;
            float fadeTop = scroll.Current + panel_height;

            if (scroll.Current <= 0) fadeTop -= panel_height;
            if (!scroll.IsScrolledToEnd()) fadeBottom -= panel_height;

            // logic is mostly shared with Leaderboard, copied here for simplicity.
            foreach (var c in Flow.Children)
            {
                float topY = c.ToSpaceOfOtherDrawable(Vector2.Zero, Flow).Y;
                float bottomY = topY + panel_height;

                bool requireTopFade = requiresScroll && topY <= fadeTop;
                bool requireBottomFade = requiresScroll && bottomY >= fadeBottom;

                if (!requireTopFade && !requireBottomFade)
                    c.Colour = Color4.White;
                else if (topY > fadeBottom + panel_height || bottomY < fadeTop - panel_height)
                    c.Colour = Color4.Transparent;
                else
                {
                    if (requireBottomFade)
                    {
                        c.Colour = ColourInfo.GradientVertical(
                            Color4.White.Opacity(Math.Min(1 - (topY - fadeBottom) / panel_height, 1)),
                            Color4.White.Opacity(Math.Min(1 - (bottomY - fadeBottom) / panel_height, 1)));
                    }
                    else if (requiresScroll)
                    {
                        c.Colour = ColourInfo.GradientVertical(
                            Color4.White.Opacity(Math.Min(1 - (fadeTop - topY) / panel_height, 1)),
                            Color4.White.Opacity(Math.Min(1 - (fadeTop - bottomY) / panel_height, 1)));
                    }
                }
            }
        }

        private void sort()
        {
            if (sorting.IsValid)
                return;

            var orderedByScore = Flow
                                 .OrderByDescending(i => i.TotalScore.Value)
                                 .ThenBy(i => i.DisplayOrder.Value)
                                 .ToList();

            for (int i = 0; i < Flow.Count; i++)
            {
                Flow.SetLayoutPosition(orderedByScore[i], i);
                orderedByScore[i].ScorePosition = i + 1;
            }

            sorting.Validate();
        }

        private class InputDisabledScrollContainer : OsuScrollContainer
        {
            public InputDisabledScrollContainer()
            {
                ScrollbarVisible = false;
            }

            public override bool HandlePositionalInput => false;
            public override bool HandleNonPositionalInput => false;
        }

        private void updateVisibility() =>
            Flow.FadeTo(configVisibility.Value ? 1 : 0, duration);
    }
}
