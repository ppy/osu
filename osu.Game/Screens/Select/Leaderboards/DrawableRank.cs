// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Select.Leaderboards
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

        private void updateTexture() => rankSprite.Texture = textures.Get($@"Grades/{Rank.GetDescription()}");

        public void UpdateRank(ScoreRank newRank)
        {
            Rank = newRank;

            if (LoadState >= LoadState.Ready)
                updateTexture();
        }
    }
}
