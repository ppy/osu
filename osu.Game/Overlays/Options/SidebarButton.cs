//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class SidebarButton : Container
    {
        private TextAwesome drawableIcon;
        private SpriteText headerText;
        private Box backgroundBox;
        private Box selectionIndicator;
        public Action Action;

        private OptionsSection section;
        public OptionsSection Section
        {
            get
            {
                return section;
            }
            set
            {
                section = value;
                headerText.Text = value.Header;
                drawableIcon.Icon = value.Icon;
            }
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (selected)
                    selectionIndicator.FadeIn(50);
                else
                    selectionIndicator.FadeOut(50);
            }
        }

        public SidebarButton()
        {
            Height = OptionsSidebar.default_width;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                backgroundBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    BlendingMode = BlendingMode.Additive,
                    Colour = OsuColour.Gray(60),
                    Alpha = 0,
                },
                new Container
                {
                    Width = OptionsSidebar.default_width,
                    RelativeSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        drawableIcon = new TextAwesome
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
                headerText = new SpriteText
                {
                    Position = new Vector2(OptionsSidebar.default_width + 10, 0),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                selectionIndicator = new Box
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Y,
                    Width = 5,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            selectionIndicator.Colour = colours.Pink;
        }

        protected override bool OnClick(InputState state)
        {
            Action?.Invoke();
            backgroundBox.FlashColour(Color4.White, 400);
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            backgroundBox.FadeTo(0.4f, 200);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            backgroundBox.FadeTo(0, 200);
            base.OnHoverLost(state);
        }
    }
}