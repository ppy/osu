// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerCountdownButton : IconButton, IHasPopover
    {
        private static readonly TimeSpan[] available_delays =
        {
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(2)
        };

        public new Action<TimeSpan> Action;

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

            TooltipText = "Countdown settings";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Green;
        }

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
                flow.Add(new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = $"Start match in {duration.Humanize()}",
                    BackgroundColour = background.Colour,
                    Action = () =>
                    {
                        Action(duration);
                        this.HidePopover();
                    }
                });
            }

            return new OsuPopover { Child = flow };
        }
    }
}
