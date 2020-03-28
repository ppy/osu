// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmapRank : Container
    {
        private const int rank_size = 20;
        private readonly BeatmapInfo beatmap;

        private TopLocalRank rank;

        public CarouselBeatmapRank(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;

            Height = rank_size;
        }

        [BackgroundDependencyLoader]
        private void load(ScoreManager scores, IBindable<RulesetInfo> ruleset)
        {
            scores.ItemAdded += scoreChanged;
            scores.ItemRemoved += scoreChanged;
            ruleset.ValueChanged += _ => rulesetChanged();

            rank = new TopLocalRank(beatmap)
            {
                ScoreLoaded = scaleDisplay
            };

            InternalChild = new DelayedLoadWrapper(rank)
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        private void rulesetChanged()
        {
            rank.FetchAndLoadTopScore();
        }

        private void scoreChanged(ScoreInfo score)
        {
            if (score.BeatmapInfoID == beatmap.ID)
            {
                rank.FetchAndLoadTopScore();
            }
        }

        private void scaleDisplay(ScoreInfo score)
        {
            if (score != null)
                Width = rank_size * 2;
            else
                Width = 0;
        }
    }
}
