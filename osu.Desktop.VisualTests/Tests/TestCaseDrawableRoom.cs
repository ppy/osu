// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Multiplayer;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;
using osu.Game.Database;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseDrawableRoom : TestCase
    {
        public override string Description => @"Select your favourite room";

        public override void Reset()
        {
            base.Reset();

            DrawableRoom first;
            DrawableRoom second;
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 500f,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    first = new DrawableRoom(new Room()),
                    second = new DrawableRoom(new Room()),
                }
            });

            first.Room.Name.Value = @"Great Room Right Here";
            first.Room.Host.Value = new User { Username = @"Naeferith", Id = 9492835, Country = new Country { FlagName = @"FR" } };
            first.Room.Status.Value = new RoomStatusOpen();
            first.Room.Beatmap.Value = new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Title = @"Seiryu",
                    Artist = @"Critical Crystal",
                },
            };

            second.Room.Name.Value = @"Relax It's The Weekend";
            second.Room.Host.Value = new User { Username = @"peppy", Id = 2, Country = new Country { FlagName = @"AU" } };
            second.Room.Status.Value = new RoomStatusPlaying();
            second.Room.Beatmap.Value = new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Title = @"Serendipity",
                    Artist = @"ZAQ",
                },
            };

            AddStep(@"change state", () =>
            {
                first.Room.Status.Value = new RoomStatusPlaying();
            });

            AddStep(@"change name", () =>
            {
                first.Room.Name.Value = @"I Changed Name";
            });

            AddStep(@"change host", () =>
            {
                first.Room.Host.Value = new User { Username = @"DrabWeb", Id = 6946022, Country = new Country { FlagName = @"CA" } };
            });

            AddStep(@"change beatmap", () =>
            {
                first.Room.Beatmap.Value = null;
            });

            AddStep(@"change state", () =>
            {
                first.Room.Status.Value = new RoomStatusOpen();
            });
        }
    }
}
