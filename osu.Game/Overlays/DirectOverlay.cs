// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Overlays.SearchableList;
using osu.Game.Rulesets;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    public class DirectOverlay : SearchableListOverlay<DirectTab, DirectSortCriteria, BeatmapSearchCategory>
    {
        private const float panel_padding = 10f;

        private APIAccess api;
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
            get { return beatmapSets; }
            set
            {
                if (beatmapSets?.Equals(value) ?? false) return;

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
            get { return resultAmounts; }
            set
            {
                if (value == ResultAmounts) return;
                resultAmounts = value;

                updateResultCounts();
            }
        }

        public DirectOverlay()
        {
            RelativeSizeAxes = Axes.Both;

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
                            TextSize = 15,
                        },
                        resultCountsText = new OsuSpriteText
                        {
                            TextSize = 15,
                            Font = @"Exo2.0-Bold",
                        },
                    }
                },
            };

            Filter.Search.Current.ValueChanged += text =>
            {
                if (text != string.Empty)
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
            ((FilterControl)Filter).Ruleset.ValueChanged += ruleset => Scheduler.AddOnce(updateSearch);
            Filter.DisplayStyleControl.DisplayStyle.ValueChanged += recreatePanels;
            Filter.DisplayStyleControl.Dropdown.Current.ValueChanged += rankStatus => Scheduler.AddOnce(updateSearch);

            Header.Tabs.Current.ValueChanged += tab =>
            {
                if (tab != DirectTab.Search)
                {
                    currentQuery.Value = string.Empty;
                    Filter.Tabs.Current.Value = (DirectSortCriteria)Header.Tabs.Current.Value;
                    Scheduler.AddOnce(updateSearch);
                }
            };

            currentQuery.ValueChanged += v =>
            {
                queryChangedDebounce?.Cancel();

                if (string.IsNullOrEmpty(v))
                    Scheduler.AddOnce(updateSearch);
                else
                {
                    BeatmapSets = null;
                    ResultAmounts = null;

                    queryChangedDebounce = Scheduler.AddDelayed(updateSearch, 500);
                }
            };

            currentQuery.BindTo(Filter.Search.Current);

            Filter.Tabs.Current.ValueChanged += sortCriteria =>
            {
                if (Header.Tabs.Current.Value != DirectTab.Search && sortCriteria != (DirectSortCriteria)Header.Tabs.Current.Value)
                    Header.Tabs.Current.Value = DirectTab.Search;

                Scheduler.AddOnce(updateSearch);
            };

            updateResultCounts();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, APIAccess api, RulesetStore rulesets, PreviewTrackManager previewTrackManager)
        {
            this.api = api;
            this.rulesets = rulesets;
            this.previewTrackManager = previewTrackManager;

            resultCountsContainer.Colour = colours.Yellow;
        }

        private void updateResultCounts()
        {
            resultCountsContainer.FadeTo(ResultAmounts == null ? 0f : 1f, 200, Easing.OutQuint);
            if (ResultAmounts == null) return;

            resultCountsText.Text = pluralize("Artist", ResultAmounts.Artists) + ", " +
                                    pluralize("Song", ResultAmounts.Songs) + ", " +
                                    pluralize("Tag", ResultAmounts.Tags);
        }

        private string pluralize(string prefix, int value)
        {
            return $@"{value} {prefix}" + (value == 1 ? string.Empty : @"s");
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
                Scheduler.AddOnce(updateSearch);
        }

        private SearchBeatmapSetsRequest getSetsRequest;

        private readonly Bindable<string> currentQuery = new Bindable<string>();

        private ScheduledDelegate queryChangedDebounce;
        private PreviewTrackManager previewTrackManager;

        private void updateSearch()
        {
            queryChangedDebounce?.Cancel();

            if (!IsLoaded)
                return;

            if (State == Visibility.Hidden)
                return;

            BeatmapSets = null;
            ResultAmounts = null;

            getSetsRequest?.Cancel();

            if (api == null)
                return;

            if (Header.Tabs.Current.Value == DirectTab.Search && (Filter.Search.Text == string.Empty || currentQuery == string.Empty))
                return;

            previewTrackManager.StopAnyPlaying(this);

            getSetsRequest = new SearchBeatmapSetsRequest(currentQuery.Value ?? string.Empty,
                ((FilterControl)Filter).Ruleset.Value,
                Filter.DisplayStyleControl.Dropdown.Current.Value,
                Filter.Tabs.Current.Value); //todo: sort direction (?)

            getSetsRequest.Success += response =>
            {
                Task.Run(() =>
                {
                    var sets = response.Select(r => r.ToBeatmapSet(rulesets)).ToList();

                    // may not need scheduling; loads async internally.
                    Schedule(() =>
                    {
                        BeatmapSets = sets;
                        recreatePanels(Filter.DisplayStyleControl.DisplayStyle.Value);
                    });
                });
            };

            api.Queue(getSetsRequest);
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
