// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Overlays.SearchableList;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class DirectOverlay : SearchableListOverlay<DirectTab, DirectSortCriteria, BeatmapSearchCategory>
    {
        private const float panel_padding = 10f;

        private RulesetStore rulesets;

        private readonly FillFlowContainer resultCountsContainer;
        private readonly OsuSpriteText resultCountsText;
        private FillFlowContainer<DirectPanel> panels;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"485e74");
        protected override Color4 TrianglesColourLight => OsuColour.FromHex(@"465b71");
        protected override Color4 TrianglesColourDark => OsuColour.FromHex(@"3f5265");

        protected override SearchableListHeader<DirectTab> CreateHeader() => new Header();
        protected override SearchableListFilterControl<DirectSortCriteria, BeatmapSearchCategory> CreateFilterControl() => new FilterControl();

        private IEnumerable<BeatmapSetInfo> beatmapSets;

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            get => beatmapSets;
            set
            {
                if (ReferenceEquals(beatmapSets, value)) return;

                beatmapSets = value?.ToList();

                if (beatmapSets == null) return;

                var artists = new List<string>();
                var songs = new List<string>();
                var tags = new List<string>();

                foreach (var s in beatmapSets)
                {
                    artists.Add(s.Metadata.Artist);
                    songs.Add(s.Metadata.Title);
                    tags.AddRange(s.Metadata.Tags.Split(' '));
                }

                ResultAmounts = new ResultCounts(distinctCount(artists), distinctCount(songs), distinctCount(tags));
            }
        }

        private ResultCounts resultAmounts;

        public ResultCounts ResultAmounts
        {
            get => resultAmounts;
            set
            {
                if (value == ResultAmounts) return;

                resultAmounts = value;

                updateResultCounts();
            }
        }

        public DirectOverlay()
        {
            // osu!direct colours are not part of the standard palette

            Waves.FirstWaveColour = OsuColour.FromHex(@"19b0e2");
            Waves.SecondWaveColour = OsuColour.FromHex(@"2280a2");
            Waves.ThirdWaveColour = OsuColour.FromHex(@"005774");
            Waves.FourthWaveColour = OsuColour.FromHex(@"003a4e");

            ScrollFlow.Children = new Drawable[]
            {
                resultCountsContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Top = 5 },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Found ",
                            Font = OsuFont.GetFont(size: 15)
                        },
                        resultCountsText = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold)
                        },
                    }
                },
            };

            Filter.Search.Current.ValueChanged += text =>
            {
                if (!string.IsNullOrEmpty(text.NewValue))
                {
                    Header.Tabs.Current.Value = DirectTab.Search;

                    if (Filter.Tabs.Current.Value == DirectSortCriteria.Ranked)
                        Filter.Tabs.Current.Value = DirectSortCriteria.Relevance;
                }
                else
                {
                    Header.Tabs.Current.Value = DirectTab.NewestMaps;

                    if (Filter.Tabs.Current.Value == DirectSortCriteria.Relevance)
                        Filter.Tabs.Current.Value = DirectSortCriteria.Ranked;
                }
            };
            ((FilterControl)Filter).Ruleset.ValueChanged += _ => queueUpdateSearch();
            Filter.DisplayStyleControl.DisplayStyle.ValueChanged += style => recreatePanels(style.NewValue);
            Filter.DisplayStyleControl.Dropdown.Current.ValueChanged += _ => queueUpdateSearch();

            Header.Tabs.Current.ValueChanged += tab =>
            {
                if (tab.NewValue != DirectTab.Search)
                {
                    currentQuery.Value = string.Empty;
                    Filter.Tabs.Current.Value = (DirectSortCriteria)Header.Tabs.Current.Value;
                    queueUpdateSearch();
                }
            };

            currentQuery.ValueChanged += text => queueUpdateSearch(!string.IsNullOrEmpty(text.NewValue));

            currentQuery.BindTo(Filter.Search.Current);

            Filter.Tabs.Current.ValueChanged += tab =>
            {
                if (Header.Tabs.Current.Value != DirectTab.Search && tab.NewValue != (DirectSortCriteria)Header.Tabs.Current.Value)
                    Header.Tabs.Current.Value = DirectTab.Search;

                queueUpdateSearch();
            };

            updateResultCounts();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, RulesetStore rulesets, PreviewTrackManager previewTrackManager)
        {
            this.rulesets = rulesets;
            this.previewTrackManager = previewTrackManager;

            beatmapSetPager = new BeatmapSetPager(rulesets);
            beatmapSetPager.PageFetch += onPageFetch;

            resultCountsContainer.Colour = colours.Yellow;
        }

        private void updateResultCounts()
        {
            resultCountsContainer.FadeTo(ResultAmounts == null ? 0f : 1f, 200, Easing.OutQuint);
            if (ResultAmounts == null) return;

            resultCountsText.Text = "Artist".ToQuantity(ResultAmounts.Artists) + ", " +
                                    "Song".ToQuantity(ResultAmounts.Songs) + ", " +
                                    "Tag".ToQuantity(ResultAmounts.Tags);
        }

        private IEnumerable<DirectPanel> createPanels(PanelDisplayStyle displayStyle, IEnumerable<BeatmapSetInfo> sets)
        {
            return sets.Select<BeatmapSetInfo, DirectPanel>(b =>
            {
                switch (displayStyle)
                {
                    case PanelDisplayStyle.Grid:
                        return new DirectGridPanel(b)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        };

                    default:
                        return new DirectListPanel(b);
                }
            });
        }

        private void addPanels(PanelDisplayStyle displayStyle, IEnumerable<BeatmapSetInfo> sets)
        {
            if (BeatmapSets == null)
                return;

            var newPanels = createPanels(displayStyle, sets);

            LoadComponentsAsync(newPanels, p =>
            {
                if (panels == null)
                    return;
                
                panels.AddRange(newPanels);
            });
        }
        
        private void recreatePanels(PanelDisplayStyle displayStyle)
        {
            if (panels != null)
            {
                panels.FadeOut(200);
                panels.Expire();
                panels = null;
            }

            if (BeatmapSets == null) return;

            var newPanels = new FillFlowContainer<DirectPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(panel_padding),
                Margin = new MarginPadding { Top = 10 },
                ChildrenEnumerable = createPanels(displayStyle, BeatmapSets)
            };

            LoadComponentAsync(newPanels, p =>
            {
                if (panels != null) ScrollFlow.Remove(panels);
                ScrollFlow.Add(panels = newPanels);
            });
        }

        protected override void PopIn()
        {
            base.PopIn();

            // Queries are allowed to be run only on the first pop-in
            if (!beatmapSetPager.IsFetching)
                queueUpdateSearch();
        }

        private readonly Bindable<string> currentQuery = new Bindable<string>(string.Empty);

        private ScheduledDelegate queryChangedDebounce;
        private ScheduledDelegate addPageDebounce;
        private PreviewTrackManager previewTrackManager;
        private BeatmapSetPager beatmapSetPager;

        private class BeatmapSetPager
        {
            public event PageFetchHandler PageFetch;

            private readonly RulesetStore rulesets;

            private SearchBeatmapSetsRequest getSetsRequest;

            private int currentPage = 1;

            public bool IsLastPageFetched { get; private set; } = false;
            public bool IsFetching => getSetsRequest != null;

            public BeatmapSetPager(RulesetStore rulesets)
            {
                this.rulesets = rulesets;
            }

            public SearchBeatmapSetsRequest FetchNextPage(string query, RulesetInfo ruleset, BeatmapSearchCategory searchCategory = BeatmapSearchCategory.Any, DirectSortCriteria sortCriteria = DirectSortCriteria.Ranked, SortDirection direction = SortDirection.Descending)
            {
                if (getSetsRequest != null)
                    return null;

                getSetsRequest = new SearchBeatmapSetsRequest(
                    query,
                    ruleset,
                    currentPage,
                    searchCategory,
                    sortCriteria,
                    direction);

                getSetsRequest.Success += response =>
                {
                    var sets = response.BeatmapSets.Select(r => r.ToBeatmapSet(rulesets)).ToList();

                    if (sets.Count <= 0) IsLastPageFetched = true;

                    PageFetch?.Invoke(currentPage, sets);
                    
                    getSetsRequest = null;
                    currentPage++;
                };

                return getSetsRequest;
            }

            public void Reset()
            {
                IsLastPageFetched = false;

                currentPage = 1;

                getSetsRequest?.Cancel();
                getSetsRequest = null;
            }

            public delegate void PageFetchHandler(int page, List<BeatmapSetInfo> sets);
        }

        private void onPageFetch(int page, List<BeatmapSetInfo> sets)
        {
            Schedule(() => {
                if (page > 1)
                {
                    BeatmapSets = BeatmapSets.Concat(sets);
                    addPanels(Filter.DisplayStyleControl.DisplayStyle.Value, sets);
                }
                else
                {
                    BeatmapSets = sets;
                    recreatePanels(Filter.DisplayStyleControl.DisplayStyle.Value);
                }

                addPageDebounce = Scheduler.AddDelayed(() => addPageDebounce = null, 500);
            });
        }

        private void queueUpdateSearch(bool queryTextChanged = false)
        {
            BeatmapSets = null;
            ResultAmounts = null;
            
            beatmapSetPager?.Reset();

            queryChangedDebounce?.Cancel();
            queryChangedDebounce = Scheduler.AddDelayed(updateSearch, queryTextChanged ? 500 : 100);
        }

        private void queueAddPage()
        {
            if (beatmapSetPager.IsFetching)
                return;

            if (addPageDebounce != null)
                return;
            
            if (beatmapSetPager.IsLastPageFetched)
                return;
            
            updateSearch();
        }

        private void updateSearch()
        {
            if (!IsLoaded)
                return;

            if (State.Value == Visibility.Hidden)
                return;

            if (API == null)
                return;

            previewTrackManager.StopAnyPlaying(this);

            var getSetsRequest = beatmapSetPager.FetchNextPage(
                currentQuery.Value,
                ((FilterControl)Filter).Ruleset.Value,
                Filter.DisplayStyleControl.Dropdown.Current.Value,
                Filter.Tabs.Current.Value); //todo: sort direction (?)
            
            if (getSetsRequest == null)
                return;

            API.Queue(getSetsRequest);
        }

        protected override void Update()
        {
            base.Update();

            if (panels == null)
                return;

            if (scroll.ScrollableExtent <= 0)
                return;

            if (!scroll.IsScrolledToEnd(500f))
                return;
                        
            queueAddPage();
        }

        private int distinctCount(List<string> list) => list.Distinct().ToArray().Length;

        public class ResultCounts
        {
            public readonly int Artists;
            public readonly int Songs;
            public readonly int Tags;

            public ResultCounts(int artists, int songs, int tags)
            {
                Artists = artists;
                Songs = songs;
                Tags = tags;
            }
        }
    }
}
