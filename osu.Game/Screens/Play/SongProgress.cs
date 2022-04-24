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
using osu.Game.Skinning;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer, ISkinnableDrawable
    {
        public const float MAX_HEIGHT = info_height + bottom_bar_height + graph_height + handle_height;

        private const float info_height = 20;
        private const float bottom_bar_height = 5;
        private const float graph_height = SquareGraph.Column.WIDTH * 6;
        private const float handle_height = 18;

        private static readonly Vector2 handle_size = new Vector2(10, handle_height);

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

        public override bool HandleNonPositionalInput => AllowSeeking.Value;
        public override bool HandlePositionalInput => AllowSeeking.Value;

        protected override bool BlockScrollInput => false;

        private double firstHitTime => objects.First().StartTime;

        //TODO: this isn't always correct (consider mania where a non-last object may last for longer than the last in the list).
        private double lastHitTime => objects.Last().GetEndTime() + 1;

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

        [Resolved(canBeNull: true)]
        private Player player { get; set; }

        [Resolved]
        private GameplayClock gameplayClock { get; set; }

        [Resolved(canBeNull: true)]
        private DrawableRuleset drawableRuleset { get; set; }

        private IClock referenceClock;

        public bool UsesFixedAnchor { get; set; }

        public SongProgress()
        {
            RelativeSizeAxes = Axes.X;
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;

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
                    OnSeek = time => player?.Seek(time),
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            base.LoadComplete();

            if (drawableRuleset != null)
            {
                if (player?.Configuration.AllowUserInteraction == true)
                    ((IBindable<bool>)AllowSeeking).BindTo(drawableRuleset.HasReplayLoaded);

                referenceClock = drawableRuleset.FrameStableClock;
                Objects = drawableRuleset.Objects;
            }

            config.BindWith(OsuSetting.ShowProgressGraph, ShowGraph);

            graph.FillColour = bar.FillColour = colours.BlueLighter;
        }

        protected override void LoadComplete()
        {
            Show();

            AllowSeeking.BindValueChanged(_ => updateBarVisibility(), true);
            ShowGraph.BindValueChanged(_ => updateGraphVisibility(), true);
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
            double frameStableTime = referenceClock?.CurrentTime ?? gameplayTime;

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
            graph.FadeTo(ShowGraph.Value ? 1 : 0, transition_duration, Easing.In);

            updateInfoMargin();
        }

        private void updateInfoMargin()
        {
            float finalMargin = bottom_bar_height + (AllowSeeking.Value ? handle_size.Y : 0) + (ShowGraph.Value ? graph_height : 0);
            info.TransformTo(nameof(info.Margin), new MarginPadding { Bottom = finalMargin }, transition_duration, Easing.In);
        }
    }
}
