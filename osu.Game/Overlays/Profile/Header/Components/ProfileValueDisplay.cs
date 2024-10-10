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
        private readonly ContentText content;

        public LocalisableString Title
        {
            set => title.Text = value;
        }

        public LocalisableString Content
        {
            set => content.Text = value;
        }

        public LocalisableString ContentTooltipText
        {
            set => content.TooltipText = value;
        }

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
                    content = new ContentText
                    {
                        Font = OsuFont.GetFont(size: big ? 30 : 20, weight: FontWeight.Light),
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
            content.Colour = colourProvider.Content2;
        }

        private partial class ContentText : OsuSpriteText, IHasTooltip
        {
            public LocalisableString TooltipText { get; set; }
        }
    }
}
