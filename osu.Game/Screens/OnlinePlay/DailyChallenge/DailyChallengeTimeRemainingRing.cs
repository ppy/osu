// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeTimeRemainingRing : OnlinePlayComposite
    {
        private CircularProgress progress = null!;
        private OsuSpriteText timeText = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new SectionHeader("Time remaining"),
                new DrawSizePreservingFillContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 35 },
                    TargetDrawSize = new Vector2(200),
                    Strategy = DrawSizePreservationStrategy.Minimum,
                    Children = new Drawable[]
                    {
                        new CircularProgress
                        {
                            Size = new Vector2(180),
                            InnerRadius = 0.1f,
                            Progress = 1,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = colourProvider.Background5,
                        },
                        progress = new CircularProgress
                        {
                            Size = new Vector2(180),
                            InnerRadius = 0.1f,
                            Progress = 1,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Vertical,
                            Children = new[]
                            {
                                timeText = new OsuSpriteText
                                {
                                    Text = "00:00:00",
                                    Font = OsuFont.TorusAlternate.With(size: 40),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                new OsuSpriteText
                                {
                                    Text = "remaining",
                                    Font = OsuFont.Default.With(size: 20),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            StartDate.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            EndDate.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            updateState();
            FinishTransforms(true);
        }

        private ScheduledDelegate? scheduledUpdate;

        private void updateState()
        {
            scheduledUpdate?.Cancel();
            scheduledUpdate = null;

            const float transition_duration = 300;

            if (StartDate.Value == null || EndDate.Value == null || EndDate.Value < DateTimeOffset.Now)
            {
                timeText.Text = TimeSpan.Zero.ToString(@"hh\:mm\:ss");
                progress.Progress = 0;
                timeText.FadeColour(colours.Red2, transition_duration, Easing.OutQuint);
                progress.FadeColour(colours.Red2, transition_duration, Easing.OutQuint);
                return;
            }

            var roomDuration = EndDate.Value.Value - StartDate.Value.Value;
            var remaining = EndDate.Value.Value - DateTimeOffset.Now;

            timeText.Text = remaining.ToString(@"hh\:mm\:ss");
            progress.Progress = remaining.TotalSeconds / roomDuration.TotalSeconds;

            if (remaining < TimeSpan.FromMinutes(15))
            {
                timeText.Colour = progress.Colour = colours.Red1;
                timeText
                    .FadeColour(colours.Red1)
                    .Then().FlashColour(colours.Red0, transition_duration, Easing.OutQuint);
                progress
                    .FadeColour(colours.Red1)
                    .Then().FlashColour(colours.Red0, transition_duration, Easing.OutQuint);
            }
            else
            {
                timeText.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
                progress.FadeColour(colourProvider.Highlight1, transition_duration, Easing.OutQuint);
            }

            scheduledUpdate = Scheduler.AddDelayed(updateState, 1000);
        }
    }
}
