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
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer
    {
        private const int bottom_bar_height = 5;
        private const float graph_height = SquareGraph.Column.WIDTH * 6;
        private static readonly Vector2 handle_size = new Vector2(10, 18);

        private const float transition_duration = 200;

        private readonly SongProgressBar bar;
        private readonly SongProgressGraph graph;
        private readonly SongProgressInfo info;

        public Action<double> RequestSeek;
        public readonly Bindable<bool> CollapseGraph = new Bindable<bool>();

        public override bool HandleNonPositionalInput => AllowSeeking;
        public override bool HandlePositionalInput => AllowSeeking;

        private double firstHitTime => objects.First().StartTime;
        private double lastHitTime => ((objects.Last() as IHasEndTime)?.EndTime ?? objects.Last().StartTime) + 1;

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

        private bool allowSeeking;

        public bool AllowSeeking
        {
            get => allowSeeking;
            set
            {
                if (allowSeeking == value) return;

                allowSeeking = value;
                updateBarVisibility();
            }
        }

        private readonly BindableBool replayLoaded = new BindableBool();

        public IClock ReferenceClock;

        private IClock gameplayClock;

        public SongProgress()
        {
            Height = bottom_bar_height + graph_height + handle_size.Y;
            Y = bottom_bar_height;

            Children = new Drawable[]
            {
                info = new SongProgressInfo
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Bottom = bottom_bar_height + graph_height },
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
                    Alpha = 0,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    OnSeek = time => RequestSeek?.Invoke(time),
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, GameplayClock clock, OsuConfigManager config)
        {
            if (clock != null)
                gameplayClock = clock;

            config.BindWith(OsuSetting.CollapseProgressGraph, CollapseGraph);

            graph.FillColour = bar.FillColour = colours.BlueLighter;
        }

        protected override void LoadComplete()
        {
            Show();

            replayLoaded.BindValueChanged(loaded => AllowSeeking = loaded.NewValue, true);
            CollapseGraph.BindValueChanged(_ => updateGraphVisibility(), true);
        }

        public void BindDrawableRuleset(DrawableRuleset drawableRuleset)
        {
            replayLoaded.BindTo(drawableRuleset.HasReplayLoaded);
        }

        protected override void PopIn()
        {
            updateBarVisibility();
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
        }

        private void updateBarVisibility()
        {
            bar.FadeTo(allowSeeking ? 1 : 0, transition_duration, Easing.In);
            this.MoveTo(new Vector2(0, allowSeeking ? 0 : bottom_bar_height), transition_duration, Easing.In);

            updateInfoMargin();
        }

        private void updateGraphVisibility()
        {
            float barHeight = bottom_bar_height + handle_size.Y;

            bar.ResizeHeightTo(CollapseGraph.Value ? barHeight : barHeight + graph_height, transition_duration, Easing.In);
            graph.MoveToY(CollapseGraph.Value ? bottom_bar_height + graph_height : 0, transition_duration, Easing.In);

            updateInfoMargin();
        }

        private void updateInfoMargin()
        {
            float finalMargin = bottom_bar_height + (allowSeeking ? handle_size.Y : 0) + (CollapseGraph.Value ? 0 : graph_height);
            info.TransformTo(nameof(info.Margin), new MarginPadding { Bottom = finalMargin }, transition_duration, Easing.In);
        }
    }
}
