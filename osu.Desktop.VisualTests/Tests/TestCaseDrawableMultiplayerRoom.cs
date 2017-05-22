// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;
using osu.Game.Screens.Multiplayer;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;
using osu.Game.Database;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseDrawableMultiplayerRoom : TestCase
    {
        public override string Description => @"Select your favourite room";

        public override void Reset()
        {
            base.Reset();

            DrawableMultiplayerRoom p;
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 500f,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    p = new DrawableMultiplayerRoom(new MultiplayerRoom
                    {
                        Name = @"Great Room Right Here",
                        Host = new User { Username = @"Naeferith", Country = new Country { FlagName = @"FR" }},
                        Status = MultiplayerRoomStatus.Open,
                        CurrentBeatmap = new BeatmapMetadata { Title = @"Seiryu", Artist = @"Critical Crystal" },
                    }),
                    new DrawableMultiplayerRoom(new MultiplayerRoom
                    {
                        Name = @"Relax It's The Weekend",
                        Host = new User{ Username = @"Someone", Country = new Country { FlagName = @"CA" }},
                        Status = MultiplayerRoomStatus.Playing,
                        CurrentBeatmap = new BeatmapMetadata { Title = @"ZAQ", Artist = @"Serendipity" },
                    }),
                }
            });

            AddStep(@"change state", () => { p.Room.Status = MultiplayerRoomStatus.Playing; });
        }
    }
}
