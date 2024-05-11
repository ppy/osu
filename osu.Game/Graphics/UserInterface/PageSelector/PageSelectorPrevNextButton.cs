// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public partial class PageSelectorPrevNextButton : PageSelectorButton
    {
        private readonly bool rightAligned;
        private readonly LocalisableString text;

        private SpriteIcon icon;
        private OsuSpriteText name;

        public PageSelectorPrevNextButton(bool rightAligned, LocalisableString text)
        {
            this.rightAligned = rightAligned;
            this.text = text;
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(3, 0),
                    Children = new Drawable[]
                    {
                        name = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12),
                            Anchor = rightAligned ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = rightAligned ? Anchor.CentreLeft : Anchor.CentreRight,
                            Text = text.ToUpper(),
                        },
                        icon = new SpriteIcon
                        {
                            Icon = rightAligned ? FontAwesome.Solid.ChevronRight : FontAwesome.Solid.ChevronLeft,
                            Size = new Vector2(8),
                            Anchor = rightAligned ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = rightAligned ? Anchor.CentreLeft : Anchor.CentreRight,
                        },
                    }
                },
            }
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Colour = ColourProvider.Dark4;
            name.Colour = icon.Colour = ColourProvider.Light1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.BindValueChanged(enabled => Background.FadeTo(enabled.NewValue ? 1 : 0.5f, DURATION), true);
        }

        protected override void UpdateHoverState() =>
            Background.FadeColour(IsHovered ? ColourProvider.Dark3 : ColourProvider.Dark4, DURATION, Easing.OutQuint);
    }
}
