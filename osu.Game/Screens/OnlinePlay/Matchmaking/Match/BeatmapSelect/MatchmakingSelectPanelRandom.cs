// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanelRandom : MatchmakingSelectPanel
    {
        public new MatchmakingPlaylistItemRandom Item => (MatchmakingPlaylistItemRandom)base.Item;

        public MatchmakingSelectPanelRandom(MatchmakingPlaylistItemRandom item)
            : base(item)
        {
        }

        private Container? randomPanelContent;
        private BeatmapCardMatchmakingBeatmapContent? beatmapPanelContent;
        private SpriteIcon dice = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Add(randomPanelContent = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
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
                    new OsuSpriteText
                    {
                        Y = 20,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Random"
                    },
                }
            });

            ScaleContainer.Add(dice = new SpriteIcon
            {
                Y = -10,
                Size = new Vector2(28),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = randomDiceIcon(),
            });

            dice.Spin(10_000, RotationDirection.Clockwise);
        }

        public override void PresentAsChosenBeatmap(MatchmakingPlaylistItemBeatmap item)
        {
            ShowChosenBorder();

            this.MoveTo(Vector2.Zero, 1000, Easing.OutExpo)
                .ScaleTo(1.5f, 1000, Easing.OutExpo);

            randomPanelContent?.Expire();
            dice.FadeOut().Expire();

            var flashLayer = new Box { RelativeSizeAxes = Axes.Both };

            AddRange(new Drawable[]
            {
                beatmapPanelContent = new BeatmapCardMatchmakingBeatmapContent(item.Beatmap, item.Mods),
                flashLayer,
            });

            flashLayer.FadeOutFromOne(1000, Easing.In).Expire();
        }

        protected override float AvatarOverlayOffset => base.AvatarOverlayOffset + (beatmapPanelContent?.AvatarOffset ?? 0);

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
