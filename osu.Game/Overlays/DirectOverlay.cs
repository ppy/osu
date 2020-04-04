// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
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

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private readonly FillFlowContainer resultCountsContainer;
        private readonly OsuSpriteText resultCountsText;
        private FillFlowContainer<DirectPanel> panels;

        protected override Color4 BackgroundColour => Color4Extensions.FromHex(@"485e74");
        protected override Color4 TrianglesColourLight => Color4Extensions.FromHex(@"465b71");
        protected override Color4 TrianglesColourDark => Color4Extensions.FromHex(@"3f5265");

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
            : base(OverlayColourScheme.Blue)
        {
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
        private void load(OsuColour colours)
        {
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
                ChildrenEnumerable = BeatmapSets.Select<BeatmapSetInfo, DirectPanel>(b =>
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
                })
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
            if (getSetsRequest == null)
                queueUpdateSearch();
        }

        private SearchBeatmapSetsRequest getSetsRequest;

        private readonly Bindable<string> currentQuery = new Bindable<string>(string.Empty);

        private ScheduledDelegate queryChangedDebounce;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        private void queueUpdateSearch(bool queryTextChanged = false)
        {
            BeatmapSets = null;
            ResultAmounts = null;

            getSetsRequest?.Cancel();

            queryChangedDebounce?.Cancel();
            queryChangedDebounce = Scheduler.AddDelayed(updateSearch, queryTextChanged ? 500 : 100);
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

            getSetsRequest = new SearchBeatmapSetsRequest(
                currentQuery.Value,
                ((FilterControl)Filter).Ruleset.Value,
                Filter.DisplayStyleControl.Dropdown.Current.Value,
                Filter.Tabs.Current.Value); //todo: sort direction (?)

            getSetsRequest.Success += response =>
            {
                Task.Run(() =>
                {
                    var sets = response.BeatmapSets.Select(r => r.ToBeatmapSet(rulesets)).ToList();

                    // may not need scheduling; loads async internally.
                    Schedule(() =>
                    {
                        BeatmapSets = sets;
                        recreatePanels(Filter.DisplayStyleControl.DisplayStyle.Value);
                    });
                });
            };

            API.Queue(getSetsRequest);
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
