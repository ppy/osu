// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Extensions;
using osu.Game.Modes.Scoring;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class DrawableRank : Container
    {
        private readonly Sprite sprite;

        public ScoreRank Rank { get; private set; }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            sprite.Texture = textures.Get($@"Badges/ScoreRanks/{Rank.GetDescription()}");
        }

        public DrawableRank(ScoreRank rank)
        {
            Rank = rank;

            Children = new Drawable[]
            {
                sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }
    }
}
