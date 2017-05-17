// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Overlays.Direct;

namespace osu.Game.Overlays
{
    public class DirectOverlay : WaveOverlayContainer
    {
        public static readonly int WIDTH_PADDING = 80;

        private readonly Box background;
        private readonly FilterControl filter;

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
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new Header
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                        filter = new FilterControl
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
                new DirectGridPanel
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Width = 300,
                },
                new DirectListPanel
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Width = 0.8f,
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
