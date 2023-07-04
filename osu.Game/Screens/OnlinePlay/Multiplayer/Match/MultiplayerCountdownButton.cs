// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class MultiplayerCountdownButton : IconButton, IHasPopover
    {
        private static readonly TimeSpan[] available_delays =
        {
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(2)
        };

        public new Action<TimeSpan> Action;

        public Action CancelAction;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly Drawable background;

        public MultiplayerCountdownButton()
        {
            Icon = FontAwesome.Regular.Clock;

            Add(background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue
            });

            base.Action = this.ShowPopover;

            TooltipText = MultiplayerMatchStrings.CountdownSettings;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Green;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            multiplayerClient.RoomUpdated += onRoomUpdated;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            multiplayerClient.RoomUpdated -= onRoomUpdated;
        }

        private void onRoomUpdated() => Scheduler.AddOnce(() =>
        {
            bool countdownActive = multiplayerClient.Room?.ActiveCountdowns.Any(c => c is MatchStartCountdown) == true;

            if (countdownActive)
            {
                background
                    .FadeColour(colours.YellowLight, 100, Easing.In)
                    .Then()
                    .FadeColour(colours.YellowDark, 900, Easing.OutQuint)
                    .Loop();
            }
            else
            {
                background
                    .FadeColour(colours.Green, 200, Easing.OutQuint);
            }
        });

        public Popover GetPopover()
        {
            var flow = new FillFlowContainer
            {
                Width = 200,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2),
            };

            foreach (var duration in available_delays)
            {
                flow.Add(new RoundedButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = MultiplayerMatchStrings.StartMatchWithCountdown(duration.Humanize()),
                    BackgroundColour = colours.Green,
                    Action = () =>
                    {
                        Action(duration);
                        this.HidePopover();
                    }
                });
            }

            if (multiplayerClient.Room?.ActiveCountdowns.Any(c => c is MatchStartCountdown) == true && multiplayerClient.IsHost)
            {
                flow.Add(new RoundedButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = MultiplayerMatchStrings.StopCountdown,
                    BackgroundColour = colours.Red,
                    Action = () =>
                    {
                        CancelAction();
                        this.HidePopover();
                    }
                });
            }

            return new OsuPopover { Child = flow };
        }
    }
}
