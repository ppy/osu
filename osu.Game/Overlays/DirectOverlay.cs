// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Direct;

using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Overlays
{
    public class DirectOverlay : WaveOverlayContainer
    {
        public static readonly int WIDTH_PADDING = 80;
        private const float panel_padding = 10f;

        private readonly FilterControl filter;
        private readonly FillFlowContainer resultCountsContainer;
        private readonly OsuSpriteText resultCountsText;
        private readonly FillFlowContainer<DirectPanel> panels;

        private IEnumerable<BeatmapSetInfo> beatmapSets;
        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            get { return beatmapSets; }
            set
            {
                if (beatmapSets?.Equals(value) ?? false) return;
                beatmapSets = value;

                recreatePanels(filter.DisplayStyle.Value);
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

            Header header;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"485e74"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new[]
                    {
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            TriangleScale = 5,
                            ColourLight = OsuColour.FromHex(@"465b71"),
                            ColourDark = OsuColour.FromHex(@"3f5265"),
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Header.HEIGHT + FilterControl.HEIGHT },
                    Children = new[]
                    {
                        new ScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScrollDraggerVisible = false,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        resultCountsContainer = new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Margin = new MarginPadding { Left = WIDTH_PADDING, Top = 6 },
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
                                            Padding = new MarginPadding { Top = panel_padding, Bottom = panel_padding, Left = WIDTH_PADDING, Right = WIDTH_PADDING },
                                            Spacing = new Vector2(panel_padding),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                filter = new FilterControl
                {
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding { Top = Header.HEIGHT },
                },
                header = new Header
                {
                    RelativeSizeAxes = Axes.X,
                },
            };

            header.Tabs.Current.ValueChanged += tab => { if (tab != DirectTab.Search) filter.Search.Current.Value = string.Empty; };

            filter.Search.Exit = Hide;
            filter.Search.Current.ValueChanged += text => { if (text != string.Empty) header.Tabs.Current.Value = DirectTab.Search; };
            filter.DisplayStyle.ValueChanged += recreatePanels;

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
            panels.Children = BeatmapSets.Select(b => displayStyle == PanelDisplayStyle.Grid ? (DirectPanel)new DirectGridPanel(b) { Width = 400 } : new DirectListPanel(b));
        }

        protected override bool OnFocus(InputState state)
        {
            filter.Search.TriggerFocus();
            return false;
        }

        protected override void PopIn()
        {
            base.PopIn();

            filter.Search.HoldFocus = true;
        }

        protected override void PopOut()
        {
            base.PopOut();

            filter.Search.HoldFocus = false;
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

        public enum PanelDisplayStyle
        {
            Grid,
            List,
        }
    }
}
