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

            DrawableRoom p;
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 500f,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    p = new DrawableRoom(new Room
                    {
                        Name = @"Great Room Right Here",
                        Host = new User { Username = @"Naeferith", Id = 9492835, Country = new Country { FlagName = @"FR" }},
                        Status = RoomStatus.Open,
                        Beatmap = new BeatmapMetadata { Title = @"Seiryu", Artist = @"Critical Crystal" },
                    }),
                    new DrawableRoom(new Room
                    {
                        Name = @"Relax It's The Weekend",
                        Host = new User{ Username = @"peppy", Id = 2, Country = new Country { FlagName = @"AU" }},
                        Status = RoomStatus.Playing,
                        Beatmap = new BeatmapMetadata { Title = @"ZAQ", Artist = @"Serendipity" },
                    }),
                }
            });

            AddStep(@"change state", () =>
            {
                p.Room.Value.Status = RoomStatus.Playing;
                p.Room.TriggerChange();
            });

            AddStep(@"change name", () =>
            {
                p.Room.Value.Name = @"I Changed Name";
                p.Room.TriggerChange();
            });

            AddStep(@"change host", () =>
            {
                p.Room.Value.Host = new User { Username = @"DrabWeb", Id = 6946022, Country = new Country { FlagName = @"CA" } };
                p.Room.TriggerChange();
            });

            AddStep(@"change beatmap", () =>
            {
                p.Room.Value.Beatmap = null;
                p.Room.TriggerChange();
            });

            AddStep(@"change state", () =>
            {
                p.Room.Value.Status = RoomStatus.Open;
                p.Room.TriggerChange();
            });
        }
    }
}
