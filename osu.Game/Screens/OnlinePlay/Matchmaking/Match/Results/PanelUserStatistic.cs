// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.Results
{
    public partial class PanelUserStatistic : CompositeDrawable
    {
        private readonly Color4 backgroundColour = Color4.SaddleBrown;

        private readonly string text;

        public PanelUserStatistic(string text)
        {
            this.text = text;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new CircularContainer
            {
                AutoSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding(10),
                        Text = text
                    }
                }
            };
        }
    }
}
