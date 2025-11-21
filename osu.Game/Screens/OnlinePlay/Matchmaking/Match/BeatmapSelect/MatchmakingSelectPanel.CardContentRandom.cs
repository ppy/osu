// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanel
    {
        public partial class CardContentRandom : CardContent
        {
            public override AvatarOverlay SelectionOverlay => selectionOverlay;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            private AvatarOverlay selectionOverlay = null!;
            public SpriteIcon Dice { get; private set; } = null!;
            public OsuSpriteText Label { get; private set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Dark5,
                    },
                    new TrianglesV2
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.1f,
                    },
                    Label = new OsuSpriteText
                    {
                        Y = 20,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Random"
                    },
                    Dice = new SpriteIcon
                    {
                        Y = -10,
                        Size = new Vector2(28),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = randomDiceIcon(),
                    },
                    selectionOverlay = new AvatarOverlay
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    }
                };

                Dice.Spin(10_000, RotationDirection.Clockwise);
            }

            public void RollDice()
            {
                var icon = randomDiceIcon();

                while (icon.Equals(Dice.Icon))
                    icon = randomDiceIcon();

                Dice.ScaleTo(0.65f, 60, Easing.Out)
                    .Then()
                    .Schedule(() => Dice.Icon = icon)
                    .ScaleTo(1f, 400, Easing.OutElasticHalf);
            }

            private static IconUsage[] diceIcons => new[]
            {
                FontAwesome.Solid.DiceOne,
                FontAwesome.Solid.DiceTwo,
                FontAwesome.Solid.DiceThree,
                FontAwesome.Solid.DiceFour,
                FontAwesome.Solid.DiceFive,
                FontAwesome.Solid.DiceSix,
            };

            private static IconUsage randomDiceIcon() => diceIcons[RNG.Next(diceIcons.Length)];
        }
    }
}
