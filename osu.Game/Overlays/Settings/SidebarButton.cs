// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using System;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SidebarButton : TabItem<SettingsSection>
    {
        private readonly Box selectionIndicator;
        private readonly Box background;
        private readonly Container text;

        public Action<SidebarButton> OnHoverAction;
        public Action<SidebarButton> OnHoverLostAction;

        public SidebarButton(SettingsSection section)
            : base(section)
        {
            Height = Sidebar.DEFAULT_WIDTH;
            RelativeSizeAxes = Axes.X;
            Masking = true;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
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
                        new OsuSpriteText
                        {
                            Position = new Vector2(Sidebar.DEFAULT_WIDTH + 10, 0),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = section.Header,
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(20),
                            Icon = section.Icon,
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
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            selectionIndicator.Colour = colours.Yellow;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeTo(0.4f, 200);
            OnHoverAction.Invoke(this);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeTo(0, 200);
            OnHoverLostAction.Invoke(this);
            base.OnHoverLost(e);
        }

        protected override void OnActivated()
        {
            selectionIndicator.FadeIn(50);
            text.FadeColour(Color4.White, 50);
        }

        protected override void OnDeactivated()
        {
            selectionIndicator.FadeOut(50);
            text.FadeColour(OsuColour.Gray(0.6f), 50);
        }
    }
}
