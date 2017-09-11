// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Settings
{
    public class SidebarButton : Container
    {
        private readonly SpriteIcon drawableIcon;
        private readonly SpriteText headerText;
        private readonly Box backgroundBox;
        private readonly Box selectionIndicator;
        private readonly Container text;
        public Action<SettingsSection> Action;

        private SettingsSection section;
        public SettingsSection Section
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
                {
                    selectionIndicator.FadeIn(50);
                    text.FadeColour(Color4.White, 50);
                }
                else
                {
                    selectionIndicator.FadeOut(50);
                    text.FadeColour(OsuColour.Gray(0.6f), 50);
                }
            }
        }

        public SidebarButton()
        {
            Height = Sidebar.DEFAULT_WIDTH;
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                backgroundBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingMode.Additive,
                    Colour = OsuColour.Gray(60),
                    Alpha = 0,
                },
                text = new Container
                {
                    Width = Sidebar.DEFAULT_WIDTH,
                    RelativeSizeAxes = Axes.Y,
                    Colour = OsuColour.Gray(0.6f),
                    Children = new Drawable[]
                    {
                        headerText = new OsuSpriteText
                        {
                            Position = new Vector2(Sidebar.DEFAULT_WIDTH + 10, 0),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        drawableIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(20),
                        },
                    }
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
            selectionIndicator.Colour = colours.Yellow;
        }

        protected override bool OnClick(InputState state)
        {
            Action?.Invoke(section);
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