// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    public partial class StageDisplay
    {
        public partial class TimerText : CompositeDrawable
        {
            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private OsuSpriteText text = null!;

            private DateTimeOffset countdownEndTime;

            public TimerText()
            {
                AutoSizeAxes = Axes.X;
                Height = 18;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = text = new OsuSpriteText
                {
                    Height = 18,
                    Spacing = new Vector2(-1, 0),
                    Font = OsuFont.Style.Heading2.With(fixedWidth: true),
                    AlwaysPresent = true,
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                client.CountdownStarted += onCountdownStarted;
                client.CountdownStopped += onCountdownStopped;

                if (client.Room != null)
                {
                    foreach (var countdown in client.Room.ActiveCountdowns)
                        onCountdownStarted(countdown);
                }
            }

            protected override void Update()
            {
                base.Update();

                TimeSpan remaining = countdownEndTime - DateTimeOffset.Now;

                text.Alpha = remaining.TotalSeconds > 0 ? 1f : 0.2f;

                if (remaining.TotalSeconds > 10)
                    text.Font = text.Font.With(weight: FontWeight.SemiBold);
                else
                    text.Font = text.Font.With(weight: FontWeight.Bold);

                int minutes = (int)Math.Max(0, remaining.TotalMinutes);
                int seconds = Math.Max(0, remaining.Seconds);
                int ms = Math.Max(0, remaining.Milliseconds);

                text.Text = $"{minutes:00}:{seconds:00}.{ms:000}";
            }

            private void onCountdownStarted(MultiplayerCountdown countdown) => Scheduler.Add(() =>
            {
                if (countdown is MatchmakingStageCountdown)
                    countdownEndTime = DateTimeOffset.Now + countdown.TimeRemaining;
            });

            private void onCountdownStopped(MultiplayerCountdown countdown) => Scheduler.Add(() =>
            {
                if (countdown is not MatchmakingStageCountdown)
                    return;

                countdownEndTime = DateTimeOffset.Now;
            });

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (client.IsNotNull())
                {
                    client.CountdownStarted -= onCountdownStarted;
                    client.CountdownStopped -= onCountdownStopped;
                }
            }
        }
    }
}
