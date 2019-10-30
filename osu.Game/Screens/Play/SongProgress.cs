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
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer
    {
        private const int bottom_bar_height = 5;

        private static readonly Vector2 handle_size = new Vector2(10, 18);

        private const float transition_duration = 200;

        private readonly SongProgressBar bar;
        private readonly SongProgressGraph graph;
        private readonly SongProgressInfo info;

        public Action<double> RequestSeek;

        public override bool HandleNonPositionalInput => AllowSeeking;
        public override bool HandlePositionalInput => AllowSeeking;

        private double lastHitTime => ((objects.Last() as IHasEndTime)?.EndTime ?? objects.Last().StartTime) + 1;

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

        private readonly BindableBool replayLoaded = new BindableBool();

        public IClock ReferenceClock;

        private IClock gameplayClock;

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, GameplayClock clock)
        {
            if (clock != null)
                gameplayClock = clock;

            graph.FillColour = bar.FillColour = colours.BlueLighter;
        }

        public SongProgress()
        {
            const float graph_height = SquareGraph.Column.WIDTH * 6;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Show();

            replayLoaded.ValueChanged += loaded => AllowSeeking = loaded.NewValue;
            replayLoaded.TriggerChange();
        }

        public void BindDrawableRuleset(DrawableRuleset drawableRuleset)
        {
            replayLoaded.BindTo(drawableRuleset.HasReplayLoaded);
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

        private void updateBarVisibility()
        {
            bar.FadeTo(allowSeeking ? 1 : 0, transition_duration, Easing.In);
            this.MoveTo(new Vector2(0, allowSeeking ? 0 : bottom_bar_height), transition_duration, Easing.In);

            info.Margin = new MarginPadding { Bottom = Height - (allowSeeking ? 0 : handle_size.Y) };
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
    }
}
