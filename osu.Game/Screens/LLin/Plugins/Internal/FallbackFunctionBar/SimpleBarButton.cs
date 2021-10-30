using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar
{
    public class SimpleBarButton : CompositeDrawable, IHasTooltip
    {
        private readonly SpriteIcon spriteIcon = new SpriteIcon
        {
            Size = new Vector2(13),
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Colour = Color4.White
        };

        private readonly OsuSpriteText spriteText = new OsuSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Colour = Color4.White
        };

        private readonly Box flashBox;

        public LocalisableString TooltipText { get; }

        public readonly IFunctionProvider Provider;

        protected LocalisableString Title
        {
            get => spriteText.Text;
            set => spriteText.Text = value;
        }

        private IconUsage emptyIcon => new IconUsage();

        protected IconUsage Icon
        {
            get => spriteIcon.Icon;
            set
            {
                if (!value.Equals(emptyIcon))
                    spriteIcon.Icon = value;
                else
                    spriteIcon.FadeOut();
            }
        }

        public SimpleBarButton(IFunctionProvider provider)
        {
            TooltipText = provider.Description;
            Width = Math.Max(provider.Size.X, 40);
            Provider = provider;

            Title = provider.Title;
            Icon = provider.Icon;

            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#365960")
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        spriteIcon,
                        spriteText,
                    }
                },
                flashBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0,
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            flashBox.FadeTo(0.1f, 300);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            flashBox.FadeTo(0f, 300);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Provider.Active();
            return base.OnClick(e);
        }
    }
}
