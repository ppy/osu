// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Select
{
    public partial class PlayBeatmapDetailArea : BeatmapDetailArea
    {
        private BeatmapLeaderboardScoresProvider leaderboardScoresProvider;

        private BeatmapLeaderboard leaderboard;

        public override WorkingBeatmap Beatmap
        {
            get => base.Beatmap;
            set
            {
                base.Beatmap = value;

                leaderboardScoresProvider.BeatmapInfo = value is DummyWorkingBeatmap ? null : value?.BeatmapInfo;
            }
        }

        private Bindable<TabType> selectedTab = null!;

        private Bindable<bool> selectedModsFilter = null!;

        public PlayBeatmapDetailArea(BeatmapLeaderboardScoresProvider leaderboardScoresProvider, Action<ScoreInfo>? onScoreSelected = null)
        {
            this.leaderboardScoresProvider = leaderboardScoresProvider;

            Add(leaderboard = new BeatmapLeaderboard(leaderboardScoresProvider)
            {
                ScoreSelected = onScoreSelected,
                RelativeSizeAxes = Axes.Both
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            selectedTab = config.GetBindable<TabType>(OsuSetting.BeatmapDetailTab);
            selectedModsFilter = config.GetBindable<bool>(OsuSetting.BeatmapDetailModsFilter);

            selectedTab.BindValueChanged(tab => CurrentTab.Value = getTabItemFromTabType(tab.NewValue), true);
            CurrentTab.BindValueChanged(tab => selectedTab.Value = getTabTypeFromTabItem(tab.NewValue));

            selectedModsFilter.BindValueChanged(checkbox => CurrentModsFilter.Value = checkbox.NewValue, true);
            CurrentModsFilter.BindValueChanged(checkbox => selectedModsFilter.Value = checkbox.NewValue);
        }

        public override void Refresh()
        {
            base.Refresh();

            leaderboardScoresProvider.RefetchScores();
        }

        protected override void OnTabChanged(BeatmapDetailAreaTabItem tab, bool selectedMods)
        {
            base.OnTabChanged(tab, selectedMods);

            leaderboardScoresProvider.FilterMods = selectedMods;

            switch (tab)
            {
                case BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope> leaderboard:
                    leaderboardScoresProvider.Scope = leaderboard.Scope;
                    this.leaderboard.Show();
                    break;

                default:
                    leaderboard.Hide();
                    break;
            }
        }

        protected override BeatmapDetailAreaTabItem[] CreateTabItems() => base.CreateTabItems().Concat(new BeatmapDetailAreaTabItem[]
        {
            new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Local),
            new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Global),
            new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Country),
            new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Friend),
        }).ToArray();

        private BeatmapDetailAreaTabItem getTabItemFromTabType(TabType type)
        {
            switch (type)
            {
                case TabType.Details:
                    return new BeatmapDetailAreaDetailTabItem();

                case TabType.Local:
                    return new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Local);

                case TabType.Global:
                    return new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Global);

                case TabType.Country:
                    return new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Country);

                case TabType.Friends:
                    return new BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Friend);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private TabType getTabTypeFromTabItem(BeatmapDetailAreaTabItem item)
        {
            switch (item)
            {
                case BeatmapDetailAreaDetailTabItem:
                    return TabType.Details;

                case BeatmapDetailAreaLeaderboardTabItem<BeatmapLeaderboardScope> leaderboardTab:
                    switch (leaderboardTab.Scope)
                    {
                        case BeatmapLeaderboardScope.Local:
                            return TabType.Local;

                        case BeatmapLeaderboardScope.Country:
                            return TabType.Country;

                        case BeatmapLeaderboardScope.Global:
                            return TabType.Global;

                        case BeatmapLeaderboardScope.Friend:
                            return TabType.Friends;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(item));
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(item));
            }
        }

        public enum TabType
        {
            Details,
            Local,
            Country,
            Global,
            Friends
        }
    }
}
