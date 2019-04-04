// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Scoring;

namespace osu.Game.Online.Leaderboards
{
    public class DrawableRank : Container
    {
        private readonly Sprite rankSprite;
        private TextureStore textures;

        public ScoreRank Rank { get; private set; }

        public DrawableRank(ScoreRank rank)
        {
            Rank = rank;

            Children = new Drawable[]
            {
                rankSprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            this.textures = textures;
            updateTexture();
        }

        private void updateTexture()
        {
            rankSprite.Texture = textures.Get($@"Grades/{Rank.GetDescription()}");
        }

        public void UpdateRank(ScoreRank newRank)
        {
            Rank = newRank;

            if (LoadState >= LoadState.Ready)
                updateTexture();
        }
    }
}
