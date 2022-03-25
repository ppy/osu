// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
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

        private class BackButton : SidebarButton
        {
            private Container content;

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(SettingsSidebar.DEFAULT_WIDTH);

                AddRange(new Drawable[]
                {
                    content = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
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
                });
            }

            protected override void UpdateState()
            {
                content.FadeColour(IsHovered ? ColourProvider.Light1 : ColourProvider.Light3, FADE_DURATION, Easing.OutQuint);
            }
        }
    }
}
