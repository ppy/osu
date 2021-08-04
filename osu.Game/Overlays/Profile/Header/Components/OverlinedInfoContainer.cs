// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class OverlinedInfoContainer : CompositeDrawable
    {
        private readonly OsuSpriteText title;
        private readonly OsuSpriteText content;

        public LocalisableString Title
        {
            set => title.Text = value;
        }

        public LocalisableString Content
        {
            set => content.Text = value;
        }

        public OverlinedInfoContainer(bool big = false, int minimumWidth = 60, FillDirection direction = FillDirection.Vertical)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = direction,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    title = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: big ? 22 : 20, weight: FontWeight.Bold)
                    },
                    content = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: big ? 44 : 18, weight: FontWeight.Light)
                    },
                    new Container // Add a minimum size to the FillFlowContainer
                    {
                        Width = (direction == FillDirection.Vertical) ? minimumWidth : 0,
                    }
                }
            };
        }
    }
}
