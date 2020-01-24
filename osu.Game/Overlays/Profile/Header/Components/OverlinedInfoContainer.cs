// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class OverlinedInfoContainer : CompositeDrawable
    {
        private readonly Circle line;
        private readonly OsuSpriteText title;
        private readonly OsuSpriteText content;

        public string Title
        {
            set => title.Text = value;
        }

        public string Content
        {
            set => content.Text = value;
        }

        public Color4 LineColour
        {
            set => line.Colour = value;
        }

        public OverlinedInfoContainer(bool big = false, int minimumWidth = 60)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    line = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 4,
                    },
                    title = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: big ? 14 : 12, weight: FontWeight.Bold)
                    },
                    content = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: big ? 40 : 18, weight: FontWeight.Light)
                    },
                    new Container //Add a minimum size to the FillFlowContainer
                    {
                        Width = minimumWidth,
                    }
                }
            };
        }
    }
}
