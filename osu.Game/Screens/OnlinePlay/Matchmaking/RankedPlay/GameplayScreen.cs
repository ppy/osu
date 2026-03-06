// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class GameplayScreen : RankedPlaySubScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            CenterColumn.Children =
            [
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(20),
                    Children =
                    [
                        new OsuSpriteText
                        {
                            Text = "Gameplay is in progress...",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.GetFont(typeface: Typeface.TorusAlternate, size: 42, weight: FontWeight.Regular),
                        },
                    ]
                },
            ];
        }
    }
}
