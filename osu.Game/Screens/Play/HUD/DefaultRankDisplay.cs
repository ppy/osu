// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
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

        private SkinnableSound rankDownSample = null!;
        private SkinnableSound rankUpSample = null!;

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

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal([
                rankDownSample = new SkinnableSound(new SampleInfo("Gameplay/rank-down")),
                rankUpSample = new SkinnableSound(new SampleInfo("Gameplay/rank-up"))
            ]);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rank.Rank = scoreProcessor.Rank.Value;

            scoreProcessor.Rank.BindValueChanged(v =>
            {
                // Don't play rank-down sfx on quit/retry
                if (v.NewValue > Scoring.ScoreRank.F)
                {
                    if (v.NewValue > rank.Rank)
                        rankUpSample.Play();
                    else
                        rankDownSample.Play();
                }

                rank.Rank = v.NewValue;
            });
        }
    }
}
