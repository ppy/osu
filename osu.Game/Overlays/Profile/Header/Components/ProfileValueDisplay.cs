// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class ProfileValueDisplay : CompositeDrawable
    {
        private readonly OsuSpriteText title;

        public LocalisableString Title
        {
            set => title.Text = value;
        }

        public ContentText Content { get; }

        public ProfileValueDisplay(bool big = false, int minimumWidth = 60)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    title = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 12)
                    },
                    Content = new ContentText
                    {
                        Font = OsuFont.GetFont(size: big ? 30 : 20, weight: big ? FontWeight.Regular : FontWeight.Light),
                    },
                    new Container // Add a minimum size to the FillFlowContainer
                    {
                        Width = minimumWidth,
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            title.Colour = colourProvider.Content1;
            Content.Colour = colourProvider.Content2;
        }

        public partial class ContentText : OsuSpriteText, IHasTooltip
        {
            public LocalisableString TooltipText { get; set; }
        }
    }
}
