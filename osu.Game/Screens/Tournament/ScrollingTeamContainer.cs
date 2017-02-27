using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Input;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Timing;
using osu.Framework.Threading;
using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Screens.Tournament
{
    public class ScrollingTeamContainer : Container
    {
        public Action<ScrollingTeam> OnSelected;
        public List<Team> AvailableTeams;

        private Container tracker;

        private float speed = 0f;
        private int expiredCount = 0;

        private float offset;
        private float timeOffset;
        private float leftPos => offset + timeOffset + expiredCount * ScrollingTeam.WIDTH;

        private double lastTime;

        private ScrollState _scrollState;
        private ScrollState scrollState
        {
            get { return _scrollState; }
            set
            {
                if (_scrollState == value)
                    return;
                _scrollState = value;

                switch (value)
                {
                    case ScrollState.Scrolling:
                        idleDelegate?.Cancel();

                        resetSelected();

                        speedTo(600f, 200);
                        tracker.FadeOut(100);
                        break;
                    case ScrollState.Stopping:
                        speedTo(0f, 2000);
                        tracker.FadeIn(200);

                        Delay(2300).Schedule(() => scrollState = ScrollState.Stopped);
                        DelayReset();
                        break;
                    case ScrollState.Stopped:
                        // Find closest to center
                        Drawable closest = null;
                        foreach (var c in Children)
                        {
                            if (!(c is ScrollingTeam))
                                continue;

                            if (closest == null)
                            {
                                closest = c;
                                continue;
                            }

                            float offset = Math.Abs(c.Position.X + c.DrawWidth / 2f - DrawWidth / 2f);
                            float lastOffset = Math.Abs(closest.Position.X + closest.DrawWidth / 2f - DrawWidth / 2f);

                            if (offset < lastOffset)
                                closest = c;
                        }

                        offset += DrawWidth / 2f - (closest.Position.X + closest.DrawWidth / 2f);

                        (closest as ScrollingTeam).Selected = true;
                        OnSelected?.Invoke(closest as ScrollingTeam);

                        idleDelegate = Delay(10000).Schedule(() => scrollState = ScrollState.Idle);
                        break;
                    case ScrollState.Idle:
                        resetSelected();

                        speedTo(40f, 200);
                        tracker.FadeOut(100);
                        break;
                }
            }
        }

        private ScheduledDelegate idleDelegate;

        public ScrollingTeamContainer()
        {
            Masking = true;
            Clock = new FramedClock();

            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                tracker = new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Masking = true,
                    CornerRadius = 10f,

                    AutoSizeAxes = Axes.Both,

                    Children = new[]
                    {
                        new Box()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            Size = new Vector2(2, 100),

                            ColourInfo = ColourInfo.GradientVertical(Color4.Transparent, Color4.White)
                        },
                        new Box()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(2, 100),

                            ColourInfo = ColourInfo.GradientVertical(Color4.White, Color4.Transparent)
                        }
                    }
                }
            };

            scrollState = ScrollState.Idle;
        }

        protected override void UpdateAfterChildren()
        {
            base.Update();

            if ((AvailableTeams?.Count ?? 0) == 0)
                return;

            timeOffset -= (float)(Time.Current - lastTime) / 1000 * speed;
            lastTime = Time.Current;

            // Fill more than required to account for transformation + scrolling speed
            while (Children.Count(c => c is ScrollingTeam) < DrawWidth * 2 / ScrollingTeam.WIDTH)
                addFlags();

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
                    c.MoveToX(pos, 100);

                pos += ScrollingTeam.WIDTH;
            }
        }

        private void addFlags()
        {
            for (int i = 0; i < AvailableTeams.Count; i++)
            {
                Add(new ScrollingTeam(AvailableTeams[i])
                {
                    X = leftPos + DrawWidth
                });
            }
        }

        private void resetSelected()
        {
            foreach (var c in Children)
            {
                ScrollingTeam st = c as ScrollingTeam;
                if (st == null)
                    continue;

                if (st.Selected)
                    st.Selected = false;

                RemoveTeam(st.Team);
                AvailableTeams.Remove(st.Team);
            }
        }

        public void RemoveTeam(Team team)
        {
            foreach (var c in Children)
            {
                ScrollingTeam st = c as ScrollingTeam;

                if (st == null)
                    continue;

                if (st.Team == team)
                {
                    st.FadeOut(200);
                    st.Expire();
                }
            }
        }

        public void StartScrolling()
        {
            scrollState = ScrollState.Scrolling;
        }

        public void StopScrolling()
        {
            scrollState = ScrollState.Stopping;
        }

        private void speedTo(float value, double duration = 0, EasingTypes easing = EasingTypes.None)
        {
            DelayReset();

            UpdateTransformsOfType(typeof(TransformScrollSpeed));
            TransformFloatTo(speed, value, duration, easing, new TransformScrollSpeed());
        }

        enum ScrollState
        {
            Idle,
            Stopping,
            Stopped,
            Scrolling
        }

        public class TransformScrollSpeed : TransformFloat
        {
            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as ScrollingTeamContainer).speed = CurrentValue;
            }
        }

        public class ScrollingTeam : Container
        {
            public const float WIDTH = 58;
            public const float HEIGHT = 41;

            public Team Team;

            private Sprite flagSprite;
            private Box outline;

            private bool selected;
            public bool Selected
            {
                get { return selected; }
                set
                {
                    selected = value;

                    if (selected)
                        outline.FadeIn(100);
                    else
                        outline.FadeOut(100);
                }
            }

            public ScrollingTeam(Team team)
            {
                Team = team;

                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                Size = new Vector2(WIDTH, HEIGHT);
                Masking = true;
                CornerRadius = 8f;

                Children = new Drawable[]
                {
                    outline = new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0
                    },
                    flagSprite = new Sprite()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        Size = new Vector2(WIDTH, HEIGHT) - new Vector2(8)
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                flagSprite.Texture = textures.Get($@"Flags/{Team.FlagName}");
            }
        }
    }
}
