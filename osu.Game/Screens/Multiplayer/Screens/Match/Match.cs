// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multiplayer.Screens.Match
{
    public class Match : MultiplayerScreen
    {
        private readonly Room room;

        public override string Title => "room";
        public override string Name => room.Name.Value;

        public Match(Room room)
        {
            this.room = room;

            Child = new TriangleButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 100,
                Text = @"Match",
                Action = () => Push(new Match(room)),
            };
        }
    }
}
