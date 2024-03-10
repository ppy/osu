// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Drawings.Components
{
    public partial class ScrollingTeamContainer : Container
    {
        public event Action? OnScrollStarted;
        public event Action<TournamentTeam>? OnSelected;

        private readonly List<TournamentTeam> availableTeams = new List<TournamentTeam>();

        private readonly Container tracker;

#pragma warning disable 649
        // set via reflection.
        private float speed;
#pragma warning restore 649

        private int expiredCount;

        private float offset;
        private float timeOffset;
        private float leftPos => offset + timeOffset + expiredCount * ScrollingTeam.WIDTH;

        private double lastTime;

        private ScheduledDelegate? delayedStateChangeDelegate;

        public ScrollingTeamContainer()
        {
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                tracker = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    AutoSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.33f),

                    Masking = true,
                    CornerRadius = 10f,
                    Alpha = 0,

                    Children = new[]
                    {
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            Size = new Vector2(2, 55),

                            Colour = ColourInfo.GradientVertical(Color4.Transparent, Color4.White)
                        },
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(2, 55),

                            Colour = ColourInfo.GradientVertical(Color4.White, Color4.Transparent)
                        }
                    }
                }
            };
        }

        private ScrollState scrollState;

        private void setScrollState(ScrollState newState)
        {
            if (scrollState == newState)
                return;

            delayedStateChangeDelegate?.Cancel();

            switch (scrollState = newState)
            {
                case ScrollState.Scrolling:
                    resetSelected();

                    OnScrollStarted?.Invoke();

                    speedTo(1000f, 200);
                    tracker.FadeOut(100);
                    break;

                case ScrollState.Stopping:
                    speedTo(0f, 2000);
                    tracker.FadeIn(200);

                    delayedStateChangeDelegate = Scheduler.AddDelayed(() => setScrollState(ScrollState.Stopped), 2300);
                    break;

                case ScrollState.Stopped:
                    // Find closest to center
                    if (!Children.Any())
                        break;

                    ScrollingTeam? closest = null;

                    foreach (var c in Children)
                    {
                        if (!(c is ScrollingTeam stc))
                            continue;

                        if (closest == null)
                        {
                            closest = stc;
                            continue;
                        }

                        float o = Math.Abs(c.Position.X + c.DrawWidth / 2f - DrawWidth / 2f);
                        float lastOffset = Math.Abs(closest.Position.X + closest.DrawWidth / 2f - DrawWidth / 2f);

                        if (o < lastOffset)
                            closest = stc;
                    }

                    Debug.Assert(closest != null, "closest != null");

                    offset += DrawWidth / 2f - (closest.Position.X + closest.DrawWidth / 2f);

                    ScrollingTeam st = closest;

                    availableTeams.RemoveAll(at => at == st.Team);

                    st.Selected = true;
                    OnSelected?.Invoke(st.Team.AsNonNull());

                    delayedStateChangeDelegate = Scheduler.AddDelayed(() => setScrollState(ScrollState.Idle), 10000);
                    break;

                case ScrollState.Idle:
                    resetSelected();

                    OnScrollStarted?.Invoke();

                    speedTo(40f, 200);
                    tracker.FadeOut(100);
                    break;
            }
        }

        public void AddTeam(TournamentTeam team)
        {
            if (availableTeams.Contains(team))
                return;

            availableTeams.Add(team);

            RemoveAll(c => c is ScrollingTeam, true);
            setScrollState(ScrollState.Idle);
        }

        public void AddTeams(IEnumerable<TournamentTeam>? teams)
        {
            if (teams == null)
                return;

            foreach (TournamentTeam t in teams)
                AddTeam(t);
        }

        public void ClearTeams()
        {
            availableTeams.Clear();
            RemoveAll(c => c is ScrollingTeam, true);
            setScrollState(ScrollState.Idle);
        }

        public void RemoveTeam(TournamentTeam team)
        {
            availableTeams.Remove(team);

            foreach (var c in Children)
            {
                if (c is ScrollingTeam st)
                {
                    if (st.Team == team)
                    {
                        st.FadeOut(200);
                        st.Expire();
                    }
                }
            }
        }

        public void StartScrolling()
        {
            if (availableTeams.Count == 0)
                return;

            setScrollState(ScrollState.Scrolling);
        }

        public void StopScrolling()
        {
            if (availableTeams.Count == 0)
                return;

            switch (scrollState)
            {
                case ScrollState.Stopped:
                case ScrollState.Idle:
                    return;
            }

            setScrollState(ScrollState.Stopping);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            setScrollState(ScrollState.Idle);
        }

        protected override void UpdateAfterChildren()
        {
            timeOffset -= (float)(Time.Current - lastTime) / 1000 * speed;
            lastTime = Time.Current;

            if (availableTeams.Count > 0)
            {
                // Fill more than required to account for transformation + scrolling speed
                while (Children.Count(c => c is ScrollingTeam) < DrawWidth * 2 / ScrollingTeam.WIDTH)
                    addFlags();
            }

            float pos = leftPos;

            foreach (var c in Children)
            {
                if (!(c is ScrollingTeam))
                    continue;

                if (c.Position.X + c.DrawWidth < 0)
                {
                    c.ClearTransforms();
                    c.Expire();
                    expiredCount++;
                }
                else
                {
                    c.MoveToX(pos, 100);
                    c.FadeTo(1.0f - Math.Abs(pos - DrawWidth / 2f) / (DrawWidth / 2.5f), 100);
                }

                pos += ScrollingTeam.WIDTH;
            }
        }

        private void addFlags()
        {
            foreach (TournamentTeam t in availableTeams)
            {
                Add(new ScrollingTeam(t)
                {
                    X = leftPos + DrawWidth
                });
            }
        }

        private void resetSelected()
        {
            foreach (var c in Children)
            {
                if (c is ScrollingTeam st)
                {
                    if (st.Selected)
                    {
                        st.Selected = false;
                        RemoveTeam(st.Team);
                    }
                }
            }
        }

        private void speedTo(float value, double duration = 0, Easing easing = Easing.None) =>
            this.TransformTo(nameof(speed), value, duration, easing);

        protected enum ScrollState
        {
            None,
            Idle,
            Stopping,
            Stopped,
            Scrolling
        }

        public partial class ScrollingTeam : DrawableTournamentTeam
        {
            public new TournamentTeam Team => base.Team.AsNonNull();

            public const float WIDTH = 58;
            public const float HEIGHT = 44;

            private readonly Box outline;

            private bool selected;

            public bool Selected
            {
                get => selected;

                set
                {
                    selected = value;

                    if (selected)
                        outline.FadeIn(100);
                    else
                        outline.FadeOut(100);
                }
            }

            public ScrollingTeam(TournamentTeam team)
                : base(team)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                Size = new Vector2(WIDTH, HEIGHT);
                Masking = true;
                CornerRadius = 8f;

                Alpha = 0;

                Flag.Anchor = Anchor.Centre;
                Flag.Origin = Anchor.Centre;
                Flag.Scale = new Vector2(0.7f);

                InternalChildren = new Drawable[]
                {
                    outline = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.33f),
                        Alpha = 0
                    },
                    Flag
                };
            }
        }
    }
}
