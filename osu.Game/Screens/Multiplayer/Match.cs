// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Multiplayer
{
    public class Match : MultiplayerScreen
    {
        public override string Title => "room";
        public override string Name => "One Awesome Room"; //todo: temporary

        public Match()
        {
            Child = new TriangleButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 100,
                Text = @"Match",
                Action = () => Push(new Match()),
            };
        }
    }
}
