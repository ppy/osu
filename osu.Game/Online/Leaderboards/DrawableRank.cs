// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Scoring;
using System;

namespace osu.Game.Online.Leaderboards
{
    public class DrawableRank : Sprite
    {
        private readonly ScoreRank rank;

        public DrawableRank(ScoreRank rank)
        {
            this.rank = rank;
        }

        [BackgroundDependencyLoader(true)]
        private void load(TextureStore ts)
        {
            if (ts == null)
                throw new ArgumentNullException(nameof(ts));

            Texture = ts.Get($@"Grades/{getTextureName()}");
        }

        private string getTextureName()
        {
            switch (rank)
            {
                default:
                    return rank.GetDescription();

                case ScoreRank.SH:
                    return "SPlus";

                case ScoreRank.XH:
                    return "SSPlus";
            }
        }
    }
}
