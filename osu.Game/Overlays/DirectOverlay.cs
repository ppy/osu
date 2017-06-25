// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Direct;
using osu.Game.Overlays.SearchableList;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    public class DirectOverlay : SearchableListOverlay<DirectTab, DirectSortCritera, RankStatus>
    {
        private const float panel_padding = 10f;

        private readonly FillFlowContainer resultCountsContainer;
        private readonly OsuSpriteText resultCountsText;
        private readonly FillFlowContainer<DirectPanel> panels;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"485e74");
        protected override Color4 TrianglesColourLight => OsuColour.FromHex(@"465b71");
        protected override Color4 TrianglesColourDark => OsuColour.FromHex(@"3f5265");

        protected override SearchableListHeader<DirectTab> CreateHeader() => new Header();
        protected override SearchableListFilterControl<DirectSortCritera, RankStatus> CreateFilterControl() => new FilterControl();

        private IEnumerable<BeatmapSetInfo> beatmapSets;
        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            get { return beatmapSets; }
            set
            {
                if (beatmapSets?.Equals(value) ?? false) return;
                beatmapSets = value;

                recreatePanels(Filter.DisplayStyleControl.DisplayStyle.Value);
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

            FirstWaveColour = OsuColour.FromHex(@"19b0e2");
            SecondWaveColour = OsuColour.FromHex(@"2280a2");
            ThirdWaveColour = OsuColour.FromHex(@"005774");
            FourthWaveColour = OsuColour.FromHex(@"003a4e");

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
                panels = new FillFlowContainer<DirectPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(panel_padding),
                    Margin = new MarginPadding { Top = 10 },
                },
            };

            Header.Tabs.Current.ValueChanged += tab => { if (tab != DirectTab.Search) Filter.Search.Text = string.Empty; };
            Filter.Search.Current.ValueChanged += text => { if (text != string.Empty) Header.Tabs.Current.Value = DirectTab.Search; };
            Filter.DisplayStyleControl.DisplayStyle.ValueChanged += recreatePanels;

            updateResultCounts();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            resultCountsContainer.Colour = colours.Yellow;
        }

        private void updateResultCounts()
        {
            resultCountsContainer.FadeTo(ResultAmounts == null ? 0f : 1f, 200, EasingTypes.Out);
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
            if (BeatmapSets == null) return;
            panels.ChildrenEnumerable = BeatmapSets.Select(b => displayStyle == PanelDisplayStyle.Grid ? (DirectPanel)new DirectGridPanel(b) { Width = 400 } : new DirectListPanel(b));
        }

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
