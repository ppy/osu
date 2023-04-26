// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Scoring;

namespace osu.Game.Online.Leaderboards
{
    public partial class UpdateableRank : ModelBackedDrawable<ScoreRank?>
    {
        public ScoreRank? Rank
        {
            get => Model;
            set => Model = value;
        }

        public UpdateableRank(ScoreRank? rank = null)
        {
            Rank = rank;
        }

        protected override Drawable CreateDrawable(ScoreRank? rank)
        {
            if (rank.HasValue)
            {
                return new DrawableRank(rank.Value)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }

            return null;
        }
    }
}
