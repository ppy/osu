// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;

namespace osu.Game.Overlays.Dashboard.Home.News
{
    public partial class ShowMoreNewsPanel : OsuHoverContainer
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
                    Text = CommonStrings.ButtonsSeeMore
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
