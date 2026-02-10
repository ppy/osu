// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayBackgroundScreen : BackgroundScreen
    {
        public RankedPlayBackground Background { get; }

        public RankedPlayBackgroundScreen()
        {
            InternalChild = Background = new RankedPlayBackground
            {
                RelativeSizeAxes = Axes.Both,
                GradientOutside = Color4Extensions.FromHex("716BE0"),
                GradientInside = Color4Extensions.FromHex("#71308F"),
                DotsColour = Color4Extensions.FromHex("#CC46F6").Opacity(0.5f),
            };
        }
    }
}
