using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Plugins.Internal.FallbackFunctionBar
{
    public class SimpleBarButton : CompositeDrawable, IHasTooltip
    {
        public LocalisableString TooltipText { get; }

        public IFunctionProvider Provider { get; set; }

        private Action action { get; set; }

        public SimpleBarButton(IFunctionProvider provider)
        {
            TooltipText = provider.Description;
            Size = provider.Size;
            action = provider.Action;

            Height = 1;
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.DarkGreen
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Icon = provider.Icon,
                            Size = new Vector2(13),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Color4.Black
                        },
                        new OsuSpriteText
                        {
                            Text = provider.Title,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Color4.Black
                        }
                    }
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            action?.Invoke();
            return base.OnClick(e);
        }
    }
}
