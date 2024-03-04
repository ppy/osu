// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Simple <see cref="ResumeOverlay"/> that resumes after 800ms.
    /// </summary>
    public partial class DelayedResumeOverlay : ResumeOverlay
    {
        private const double countdown_time = 800;

        protected override LocalisableString Message => string.Empty;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private ScheduledDelegate? scheduledResume;
        private int countdownCount = 3;
        private double countdownStartTime;

        private Drawable content = null!;
        private Drawable background = null!;
        private SpriteText countdown = null!;

        public DelayedResumeOverlay()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(content = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Masking = true,
                BorderColour = colours.Yellow,
                BorderThickness = 1,
                Children = new[]
                {
                    background = new Box
                    {
                        Size = new Vector2(250, 40),
                        Colour = Color4.Black,
                        Alpha = 0.8f
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(5),
                                Colour = colours.Yellow,
                                Children = new Drawable[]
                                {
                                    // new Box
                                    // {
                                    //     Anchor = Anchor.Centre,
                                    //     Origin = Anchor.Centre,
                                    //     Size = new Vector2(40, 3)
                                    // },
                                    countdown = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        UseFullGlyphHeight = false,
                                        AlwaysPresent = true,
                                        Font = OsuFont.Numeric.With(size: 20, fixedWidth: true)
                                    },
                                    // new Box
                                    // {
                                    //     Anchor = Anchor.Centre,
                                    //     Origin = Anchor.Centre,
                                    //     Size = new Vector2(40, 3)
                                    // }
                                }
                            }
                        }
                    }
                }
            });
        }

        protected override void PopIn()
        {
            this.FadeIn();

            content.FadeInFromZero(150, Easing.OutQuint);
            content.ScaleTo(new Vector2(1.5f, 1)).Then().ScaleTo(1, 150, Easing.OutElasticQuarter);

            countdownCount = 3;
            countdownStartTime = Time.Current;

            scheduledResume?.Cancel();
            scheduledResume = Scheduler.AddDelayed(Resume, countdown_time);
        }

        protected override void Update()
        {
            base.Update();
            updateCountdown();
        }

        private void updateCountdown()
        {
            double amountTimePassed = Math.Min(countdown_time, Time.Current - countdownStartTime) / countdown_time;
            int newCount = 3 - (int)Math.Floor(amountTimePassed * 3);

            if (newCount > 0)
            {
                countdown.Alpha = 1;
                countdown.Text = newCount.ToString();
            }
            else
                countdown.Alpha = 0;

            if (newCount != countdownCount)
            {
                if (newCount == 0)
                    content.ScaleTo(new Vector2(1.5f, 1), 150, Easing.OutQuint);
                else
                    content.ScaleTo(new Vector2(1.05f, 1), 50, Easing.OutQuint).Then().ScaleTo(1, 50, Easing.Out);
            }

            countdownCount = newCount;
        }

        protected override void PopOut()
        {
            this.Delay(150).FadeOut();

            content.FadeOut(150, Easing.OutQuint);

            scheduledResume?.Cancel();
        }
    }
}
