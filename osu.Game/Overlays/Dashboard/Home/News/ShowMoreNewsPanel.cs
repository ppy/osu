// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Overlays.Dashboard.Home.News
{
    public class ShowMoreNewsPanel : OsuHoverContainer
    {
        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        [Resolved(canBeNull: true)]
        private NewsOverlay overlay { get; set; }

        private OsuSpriteText text;

        public ShowMoreNewsPanel()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Child = new HomePanel
            {
                Child = text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Vertical = 20 },
                    Text = "see more"
                }
            };

            IdleColour = colourProvider.Light1;
            HoverColour = Color4.White;

            Action = () =>
            {
                overlay?.ShowFrontPage();
            };
        }
    }
}
