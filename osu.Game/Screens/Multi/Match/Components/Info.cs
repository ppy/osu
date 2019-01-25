﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class Info : Container
    {
        public Action OnStart;

        private readonly RoomBindings bindings = new RoomBindings();

        public Info(Room room)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            ReadyButton readyButton;
            ViewBeatmapButton viewBeatmapButton;
            HostInfo hostInfo;
            RoomStatusInfo statusInfo;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"28242d"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING + OsuScreen.HORIZONTAL_OVERFLOW_PADDING },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 10),
                            Padding = new MarginPadding { Vertical = 20 },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            TextSize = 30,
                                            Current = bindings.Name
                                        },
                                        new RoomStatusInfo(room),
                                    }
                                },
                                hostInfo = new HostInfo(),
                            },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            AutoSizeAxes = Axes.X,
                            Height = 70,
                            Spacing = new Vector2(10, 0),
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                viewBeatmapButton = new ViewBeatmapButton(),
                                readyButton = new ReadyButton(room)
                                {
                                    Action = () => OnStart?.Invoke()
                                }
                            }
                        }
                    },
                },
            };

            viewBeatmapButton.Beatmap.BindTo(bindings.CurrentBeatmap);
            readyButton.Beatmap.BindTo(bindings.CurrentBeatmap);
            hostInfo.Host.BindTo(bindings.Host);

            bindings.Room = room;
        }
    }
}
