// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Scoring;

namespace osu.Game.Online.Leaderboards
{
    public partial class UpdateableRank : ModelBackedDrawable<Grade?>
    {
        public Grade? Rank
        {
            get => Model;
            set => Model = value;
        }

        public UpdateableRank(Grade? rank = null)
        {
            Rank = rank;
        }

        protected override Drawable CreateDrawable(Grade? rank)
        {
            if (rank.HasValue)
            {
                return new DrawableGrade(rank.Value)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }

            return null;
        }
    }
}
