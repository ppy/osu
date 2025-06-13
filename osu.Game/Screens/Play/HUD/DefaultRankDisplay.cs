// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultRankDisplay : CompositeDrawable, ISerialisableDrawable
    {
        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        public bool UsesFixedAnchor { get; set; }

        private UpdateableRank rankDisplay = null!;

        private SkinnableSound rankDownSample = null!;
        private SkinnableSound rankUpSample = null!;

        private IBindable<ScoreRank> rank = null!;

        public DefaultRankDisplay()
        {
            Size = new Vector2(70, 35);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                rankDownSample = new SkinnableSound(new SampleInfo("Gameplay/rank-down")),
                rankUpSample = new SkinnableSound(new SampleInfo("Gameplay/rank-up")),
                rankDisplay = new UpdateableRank(ScoreRank.X)
                {
                    RelativeSizeAxes = Axes.Both
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rank = scoreProcessor.Rank.GetBoundCopy();
            rank.BindValueChanged(r =>
            {
                // Don't play rank-down sfx on quit/retry
                if (r.NewValue != r.OldValue && r.NewValue > ScoreRank.F)
                {
                    if (r.NewValue > rankDisplay.Rank)
                        rankUpSample.Play();
                    else
                        rankDownSample.Play();
                }

                rankDisplay.Rank = r.NewValue;
            }, true);
        }
    }
}
