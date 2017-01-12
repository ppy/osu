//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public abstract class OptionsSection : Container
    {
        protected FlowContainer content;
        protected override Container<Drawable> Content => content;

        public abstract FontAwesome Icon { get; }
        public abstract string Header { get; }

        public OptionsSection()
        {
            Margin = new MarginPadding { Top = 20 };

            const int headerSize = 26, headerMargin = 25;
            const int borderSize = 2;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            AddInternal(new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(0, 0, 0, 255),
                    RelativeSizeAxes = Axes.X,
                    Height = borderSize,
                },
                new Container
                {
                    Padding = new MarginPadding
                    {
                        Top = 20 + borderSize,
                        Left = OptionsOverlay.CONTENT_MARGINS,
                        Right = OptionsOverlay.CONTENT_MARGINS,
                        Bottom = 10,
                    },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        new SpriteText
                        {
                            TextSize = headerSize,
                            Colour = OsuColour.Pink,
                            Text = Header,
                        },
                        content = new FlowContainer
                        {
                            Margin = new MarginPadding { Top = headerSize + headerMargin },
                            Direction = FlowDirection.VerticalOnly,
                            Spacing = new Vector2(0, 50),
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        },
                    }
                },
            });
        }
    }
}