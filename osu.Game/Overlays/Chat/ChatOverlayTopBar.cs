// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public partial class ChatOverlayTopBar : Container
    {
        private Box background = null!;

        private Color4 backgroundColour;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, TextureStore textures)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = backgroundColour = colourProvider.Background3,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 50),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Icon = HexaconsIcons.Social,
                                Size = new Vector2(18),
                            },
                            // Placeholder text
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = ChatStrings.TitleCompact,
                                Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                                Margin = new MarginPadding { Bottom = 2f },
                            },
                        },
                    },
                },
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(backgroundColour.Lighten(0.1f), 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(backgroundColour, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
