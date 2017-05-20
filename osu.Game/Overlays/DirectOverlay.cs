// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Direct;

namespace osu.Game.Overlays
{
    public class DirectOverlay : WaveOverlayContainer
    {
        public static readonly int WIDTH_PADDING = 80;
        private const float panel_padding = 10f;

        private readonly FilterControl filter;
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

        public ResultCounts ResultCounts
        {
            get { return filter.ResultCounts; }
            set { filter.ResultCounts = value; }
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
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollDraggerVisible = false,
                    Padding = new MarginPadding { Top = Header.HEIGHT },
                    Children = new Drawable[]
                    {
                        new ReverseDepthFillFlowContainer<Drawable>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                filter = new FilterControl
                                {
                                    RelativeSizeAxes = Axes.X,
                                },
                                panels = new FillFlowContainer<DirectPanel>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Top = FilterControl.LOWER_HEIGHT + panel_padding, Bottom = panel_padding, Left = WIDTH_PADDING, Right = WIDTH_PADDING },
                                    Spacing = new Vector2(panel_padding),
                                },
                            },
                        },
                    },
                },
                header = new Header
                {
                    RelativeSizeAxes = Axes.X,
                },
            };

            header.Tabs.Current.ValueChanged += tab => { if (tab != DirectTab.Search) filter.Search.Current.Value = @""; };

            filter.Search.Exit = Hide;
            filter.Search.Current.ValueChanged += text => { if (text != @"") header.Tabs.Current.Value = DirectTab.Search; };
            filter.DisplayStyle.ValueChanged += recreatePanels;
        }

        private void recreatePanels(PanelDisplayStyle displayStyle)
        {
            panels.Children = BeatmapSets.Select(b => displayStyle == PanelDisplayStyle.Grid ? (DirectPanel)new DirectGridPanel(b) { Width = 400 } : new DirectListPanel(b));
        }

        protected override void PopIn()
        {
            base.PopIn();

            filter.Search.HoldFocus = true;
            Schedule(() => filter.Search.TriggerFocus());
        }

        protected override void PopOut()
        {
            base.PopOut();

            filter.Search.HoldFocus = false;
        }
    }
}
