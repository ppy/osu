// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Online.Rooms.GameTypes
{
    public class GameTypeTeamVersus : GameType
    {
        public override string Name => "Team Versus";

        public override Drawable GetIcon(OsuColour colours, float size)
        {
            return new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2f),
                Children = new[]
                {
                    new VersusRow(colours.Blue, colours.Pink, size * 0.5f),
                    new VersusRow(colours.Blue, colours.Pink, size * 0.5f),
                },
            };
        }
    }
}
