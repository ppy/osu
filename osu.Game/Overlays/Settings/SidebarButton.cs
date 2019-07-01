// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SidebarButton : Button
    {
        private readonly SpriteIcon drawableIcon;
        private readonly SpriteText headerText;
        private readonly Box selectionIndicator;
        private readonly Container text;
        public new Action<SettingsSection> Action;

        private SettingsSection section;

        public SettingsSection Section
        {
            get => section;
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
            get => selected;
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
            BackgroundColour = OsuColour.Gray(60);
            Background.Alpha = 0;

            Height = Sidebar.DEFAULT_WIDTH;
            RelativeSizeAxes = Axes.X;

            AddRange(new Drawable[]
            {
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
                },
                new HoverClickSounds(HoverSampleSet.Loud),
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            selectionIndicator.Colour = colours.Yellow;
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke(section);
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            Background.FadeTo(0.4f, 200);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Background.FadeTo(0, 200);
            base.OnHoverLost(e);
        }
    }
}
