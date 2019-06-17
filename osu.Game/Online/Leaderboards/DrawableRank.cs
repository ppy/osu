// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Scoring;
using System;

namespace osu.Game.Online.Leaderboards
{
    public class DrawableRank : ModelBackedDrawable<ScoreRank>
    {
        private TextureStore textures;

        public ScoreRank Rank
        {
            get => Model;
            set => Model = value;
        }

        private ScoreRank rank;

        public DrawableRank(ScoreRank rank)
        {
            this.rank = rank;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            textures = ts ?? throw new ArgumentNullException(nameof(ts));
            Rank = rank;
        }

        protected override Drawable CreateDrawable(ScoreRank rank)
        {
            return new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit,
                Texture = textures.Get($"Grades/{getTextureName()}"),
            };
        }

        private string getTextureName()
        {
            switch (Rank)
            {
                default:
                    return Rank.GetDescription();

                case ScoreRank.SH:
                    return "SPlus";

                case ScoreRank.XH:
                    return "SSPlus";
            }
        }
    }
}
