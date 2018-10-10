// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer
    {
        private const int bottom_bar_height = 5;

        private static readonly Vector2 handle_size = new Vector2(14, 25);

        private const float transition_duration = 200;

        private readonly SongProgressBar bar;
        private readonly SongProgressGraph graph;
        private readonly SongProgressInfo info;

        public Action<double> OnSeek;

        public override bool HandleNonPositionalInput => AllowSeeking;
        public override bool HandlePositionalInput => AllowSeeking;

        private IClock audioClock;
        public IClock AudioClock { set { audioClock = info.AudioClock = value; } }

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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
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
                    Origin =  Anchor.BottomLeft,
                    OnSeek = position => OnSeek?.Invoke(position),
                },
            };
        }

        protected override void LoadComplete()
        {
            State = Visibility.Visible;

            replayLoaded.ValueChanged += v => AllowSeeking = v;
            replayLoaded.TriggerChange();
        }

        public void BindRulestContainer(RulesetContainer rulesetContainer)
        {
            replayLoaded.BindTo(rulesetContainer.HasReplayLoaded);
        }

        private bool allowSeeking;

        public bool AllowSeeking
        {
            get
            {
                return allowSeeking;
            }

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

            double position = audioClock?.CurrentTime ?? Time.Current;
            double progress = (position - firstHitTime) / (lastHitTime - firstHitTime);

            if (progress < 1)
            {
                bar.CurrentTime = position;
                graph.Progress = (int)(graph.ColumnCount * progress);
            }
        }
    }
}
