using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    public class OptionsSection : Container
    {
        private SpriteText header;
        private FlowContainer content;
        protected override Container Content => content;


        public string Header
        {
            get { return header.Text; }
            set { header.Text = value; }
        }


        public OptionsSection()
        {
            const int headerSize = 30, headerMargin = 25;
            const int borderSize = 2;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            AddInternal(new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(3, 3, 3, 255),
                    RelativeSizeAxes = Axes.X,
                    Height = borderSize,
                },
                new Container
                {
                    Padding = new MarginPadding
                    {
                        Top = 10 + borderSize,
                        Left = OptionsOverlay.SideMargins,
                        Right = OptionsOverlay.SideMargins,
                        Bottom = 10,
                    },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        header = new SpriteText
                        {
                            TextSize = headerSize,
                            Colour = new Color4(247, 198, 35, 255),
                        },
                        content = new FlowContainer
                        {
                            Margin = new MarginPadding { Top = headerSize + headerMargin },
                            Direction = FlowDirection.VerticalOnly,
                            Spacing = new Vector2(0, 25),
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        },
                    }
                },
            });
        }
    }
}