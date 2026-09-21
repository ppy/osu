// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class ProfilePopoverAction : OsuClickableContainer
    {
        private readonly IconUsage icon;
        private readonly LocalisableString caption;

        private Box background = null!;
        private CircularContainer indicator = null!;

        public ProfilePopoverAction(IconUsage icon, LocalisableString caption)
        {
            this.icon = icon;
            this.caption = caption;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Content.RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Content.AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 4;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                    Alpha = 0,
                },
                indicator = new Circle
                {
                    Width = 4,
                    Height = 14,
                    X = 10,
                    Colour = colourProvider.Highlight1,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Horizontal = 25, Vertical = 5 },
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5, 0),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Icon = icon,
                            Size = new Vector2(11),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new OsuSpriteText
                        {
                            Text = caption,
                            Font = OsuFont.Style.Body,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            UseFullGlyphHeight = false,
                        }
                    }
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            background.Alpha = indicator.Alpha = IsHovered ? 1 : 0;
        }
    }
}
