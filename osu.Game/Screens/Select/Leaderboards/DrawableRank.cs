// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Modes;
using System;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class DrawableRank : Container
    {
        private Sprite sprite;
        private TextureStore textures;

        private ScoreRank rank;
        public ScoreRank Rank
        {
            get { return rank; }
            set
            {
                if (value == rank) return;
                rank = value;
                sprite.Texture = textures.Get($@"Badges/ScoreRanks/{Enum.GetName(typeof(ScoreRank), rank)}");
            }
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            textures = ts;
            sprite.Texture = textures.Get($@"Badges/ScoreRanks/{Enum.GetName(typeof(ScoreRank), rank)}");
        }

        public DrawableRank(ScoreRank rank)
        {
            this.rank = rank;

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
