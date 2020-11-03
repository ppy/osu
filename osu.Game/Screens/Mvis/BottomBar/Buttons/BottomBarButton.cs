// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Graphics.Containers;
using osuTK.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Modules;
using osu.Game.Configuration;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class BottomBarButton : OsuClickableContainer
    {
        [Resolved]
        private CustomColourProvider colourProvider { get; set; }
        protected CustomColourProvider ColourProvider;
        protected FillFlowContainer contentFillFlow;
        protected Box bgBox;
        private Box flashBox;
        private Container content;
        public IconUsage ButtonIcon
        {
            get => spriteIcon.Icon;
            set => spriteIcon.Icon = value;
        }
        public Drawable ExtraDrawable;
        public string Text
        {
            get => spriteText.Text;
            set => spriteText.Text = value;
        }
        private OsuSpriteText spriteText  = new OsuSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };
        protected SpriteIcon spriteIcon = new SpriteIcon
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(13),
        };

        public bool NoIcon = false;

        public BottomBarButton()
        {
            Size = new Vector2(30, 30);
        }

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            ColourProvider = colourProvider;

            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    CornerRadius = 5,
                    Children = new Drawable[]
                    {
                        bgBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourProvider.Background3
                        },
                        contentFillFlow = new FillFlowContainer
                        {
                            Margin = new MarginPadding{ Left = 15, Right = 15 },
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(5)
                        },
                        flashBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Alpha = 0,
                        }
                    }
                }
            };

            if ( !NoIcon )
                contentFillFlow.Add(spriteIcon);
            
            if ( Text != null )
                contentFillFlow.Add(spriteText);

            // From OsuAnimatedButton
            if (AutoSizeAxes != Axes.None)
            {
                content.RelativeSizeAxes = (Axes.Both & ~AutoSizeAxes);
                content.AutoSizeAxes = AutoSizeAxes;
            }

            content.BorderColour = ColourProvider.Content2;
            content.BorderThickness = 1.5f;
        }

        protected override bool OnMouseDown(Framework.Input.Events.MouseDownEvent e)
        {
            content.ScaleTo(0.8f, 2000, Easing.OutQuint);
            flashBox.FadeTo(0.8f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(Framework.Input.Events.MouseUpEvent e)
        {
            content.ScaleTo(1f, 1000, Easing.OutElastic);
            flashBox.FadeOut(1000, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        protected override bool OnClick(Framework.Input.Events.ClickEvent e)
        {
            OnClickAnimation();
            return base.OnClick(e);
        }

        protected virtual void OnClickAnimation()
        {
            flashBox.FadeTo(1).Then().FadeOut(300);
        }
    }
}
