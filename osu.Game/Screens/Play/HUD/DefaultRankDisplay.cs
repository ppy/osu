// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultRankDisplay : Container, ISerialisableDrawable
    {
        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        public bool UsesFixedAnchor { get; set; }

        private readonly UpdateableRank rank;

        public DefaultRankDisplay()
        {
            Size = new Vector2(70, 35);

            InternalChildren = new Drawable[]
            {
                rank = new UpdateableRank(Scoring.ScoreRank.X)
                {
                    RelativeSizeAxes = Axes.Both
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rank.Rank = scoreProcessor.Rank.Value;

            scoreProcessor.Rank.BindValueChanged(v => rank.Rank = v.NewValue);
        }
    }
}