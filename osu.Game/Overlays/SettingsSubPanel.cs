// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Overlays
{
    public abstract class SettingsSubPanel : SettingsPanel
    {
        protected SettingsSubPanel()
            : base(true)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new BackButton
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Action = Hide
            });
        }

        protected override bool DimMainContent => false; // dimming is handled by main overlay

        private class BackButton : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
        {
            private AspectContainer aspect;

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(Sidebar.DEFAULT_WIDTH);
                Children = new Drawable[]
                {
                    aspect = new AspectContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(15),
                                Shadow = true,
                                Icon = FontAwesome.Solid.ChevronLeft
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Y = 15,
                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                                Text = @"back",
                            },
                        }
                    }
                };
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                aspect.ScaleTo(0.75f, 2000, Easing.OutQuint);
                return base.OnMouseDown(e);
            }

            protected override bool OnMouseUp(MouseUpEvent e)
            {
                aspect.ScaleTo(1, 1000, Easing.OutElastic);
                return base.OnMouseUp(e);
            }

            public bool OnPressed(GlobalAction action)
            {
                switch (action)
                {
                    case GlobalAction.Back:
                        Click();
                        return true;
                }

                return false;
            }

            public bool OnReleased(GlobalAction action) => false;
        }
    }
}
