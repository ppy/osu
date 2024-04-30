// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.Select
{
    public partial class ObjectCountContent : FillFlowContainer<BarStatisticRow>
    {
        private IRulesetInfo? lastRulesetWhenUpdated;

        [Resolved]
        private IBindable<IBeatmapInfo?> beatmapInfo { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        public ObjectCountContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(0, 5);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(r => updateObjectCounts());

            beatmapInfo.BindValueChanged(_ => updateObjectCounts(), true);
        }

        private void updateObjectCounts()
        {
            // needed for cases where ruleset and beatmapInfo change at the same time.
            Scheduler.AddOnce(() =>
            {
                switch (beatmapInfo.Value)
                {
                    case APIBeatmap beatmap:
                        if (Children.Count == 0)
                        {
                            // TODO: figure out how to get a (playable) beatmap with an `APIBeatmap` or move `GetStatistics()` to `Ruleset`
                            Children = new[]
                            {
                                new BarStatisticRow(1000)
                                {
                                    Title = BeatmapsetsStrings.ShowStatsCountCircles,
                                },
                                new BarStatisticRow(1000)
                                {
                                    Title = BeatmapsetsStrings.ShowStatsCountSliders,
                                },
                                new BarStatisticRow(1000)
                                {
                                    Title = "Spinner Count",
                                },
                            };
                        }

                        int[] apiObjectCount = { beatmap.CircleCount, beatmap.SliderCount, beatmap.SpinnerCount };

                        for (int i = 0; i < Children.Count; i++)
                            Children[i].Value = (apiObjectCount[i], null);

                        break;

                    case BeatmapInfo:
                        if (ruleset.Value == null) return;

                        var playableBeatmap = workingBeatmap.Value.GetPlayableBeatmap(ruleset.Value);
                        var statistics = playableBeatmap.GetStatistics().ToArray();

                        // only reconstruct children when the ruleset changes
                        // so that the existing `BarStatisticRow`s can animate to the new value.
                        if (lastRulesetWhenUpdated?.OnlineID != ruleset.Value.OnlineID)
                        {
                            // TODO: find way of getting the object counts from the hardest difficulty, set 1000 as max value for now
                            Children = statistics.Select(s =>
                                                     new BarStatisticRow(1000)
                                                     {
                                                         Title = s.Name,
                                                         Value = (int.Parse(s.Content), null),
                                                     })
                                                 .ToArray();
                        }
                        else
                        {
                            for (int i = 0; i < Children.Count; i++)
                                Children[i].Value = (int.Parse(statistics[i].Content), null);
                        }

                        lastRulesetWhenUpdated = ruleset.Value;
                        break;
                }
            });
        }
    }
}
