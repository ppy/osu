// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Overlays.Direct;

namespace osu.Game.Overlays
{
    public class DirectOverlay : WaveOverlayContainer
    {
        public static readonly int WIDTH_PADDING = 80;
        private readonly float panel_padding = 10f;

        private readonly Box background;
        private readonly FilterControl filter;
        private readonly FillFlowContainer<DirectPanel> panels;

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                var p = new List<DirectPanel>();

                foreach (BeatmapSetInfo b in value)
                    p.Add(new DirectListPanel(b));

                panels.Children = p;
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

            Children = new Drawable[]
            {
                background = new Box
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
                        new FillFlowContainer
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
                new Header
                {
                    RelativeSizeAxes = Axes.X,
                },
            };

            filter.Search.Exit = Hide;
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
