// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer
    {
        private const int info_height = 20;
        private const int bottom_bar_height = 5;
        private const float graph_height = SquareGraph.Column.WIDTH * 6;
        private static readonly Vector2 handle_size = new Vector2(10, 18);

        private const float transition_duration = 200;

        private readonly SongProgressBar bar;
        private readonly SongProgressGraph graph;
        private readonly SongProgressInfo info;

        public Action<double> RequestSeek;

        /// <summary>
        /// Whether seeking is allowed and the progress bar should be shown.
        /// </summary>
        public readonly Bindable<bool> AllowSeeking = new Bindable<bool>();

        public readonly Bindable<bool> ShowGraph = new Bindable<bool>();

        //TODO: this isn't always correct (consider mania where a non-last object may last for longer than the last in the list).
        private double lastHitTime => objects.Last().GetEndTime() + 1;

        public override bool HandleNonPositionalInput => AllowSeeking.Value;
        public override bool HandlePositionalInput => AllowSeeking.Value;

        private double firstHitTime => objects.First().StartTime;

        private IEnumerable<HitObject> objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                graph.Objects = objects = value;

                info.StartTime = firstHitTime;
                info.EndTime = lastHitTime;

                bar.StartTime = firstHitTime;
                bar.EndTime = lastHitTime;
            }
        }

        public IClock ReferenceClock;

        private IClock gameplayClock;

        public SongProgress()
        {
            Masking = true;

            Children = new Drawable[]
            {
                info = new SongProgressInfo
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = info_height,
                },
                graph = new SongProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Height = graph_height,
                    Margin = new MarginPadding { Bottom = bottom_bar_height },
                },
                bar = new SongProgressBar(bottom_bar_height, graph_height, handle_size)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    OnSeek = time => RequestSeek?.Invoke(time),
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, GameplayClock clock, OsuConfigManager config)
        {
            base.LoadComplete();

            if (clock != null)
                gameplayClock = clock;

            config.BindWith(OsuSetting.ShowProgressGraph, ShowGraph);

            graph.FillColour = bar.FillColour = colours.BlueLighter;
        }

        protected override void LoadComplete()
        {
            Show();

            AllowSeeking.BindValueChanged(_ => updateBarVisibility(), true);
            ShowGraph.BindValueChanged(_ => updateGraphVisibility(), true);
        }

        public void BindDrawableRuleset(DrawableRuleset drawableRuleset)
        {
            AllowSeeking.BindTo(drawableRuleset.HasReplayLoaded);
        }

        protected override void PopIn()
        {
            this.FadeIn(500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(100);
        }

        protected override void Update()
        {
            base.Update();

            if (objects == null)
                return;

            double gameplayTime = gameplayClock?.CurrentTime ?? Time.Current;
            double frameStableTime = ReferenceClock?.CurrentTime ?? gameplayTime;

            double progress = Math.Min(1, (frameStableTime - firstHitTime) / (lastHitTime - firstHitTime));

            bar.CurrentTime = gameplayTime;
            graph.Progress = (int)(graph.ColumnCount * progress);

            Height = bottom_bar_height + graph_height + handle_size.Y + info_height - graph.Y;
        }

        private void updateBarVisibility()
        {
            bar.ShowHandle = AllowSeeking.Value;

            updateInfoMargin();
        }

        private void updateGraphVisibility()
        {
            float barHeight = bottom_bar_height + handle_size.Y;

            bar.ResizeHeightTo(ShowGraph.Value ? barHeight + graph_height : barHeight, transition_duration, Easing.In);
            graph.MoveToY(ShowGraph.Value ? 0 : bottom_bar_height + graph_height, transition_duration, Easing.In);

            updateInfoMargin();
        }

        private void updateInfoMargin()
        {
            float finalMargin = bottom_bar_height + (AllowSeeking.Value ? handle_size.Y : 0) + (ShowGraph.Value ? graph_height : 0);
            info.TransformTo(nameof(info.Margin), new MarginPadding { Bottom = finalMargin }, transition_duration, Easing.In);
        }
    }
}
