// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.General;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;

namespace osu.Game.Overlays
{
    public class LoginOverlay : OsuFocusedOverlayContainer
    {
        private LoginSettings settingsSection;

        private const float transition_time = 400;

        public LoginOverlay()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new OsuContextMenuContainer
                {
                    Width = 360,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.6f,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            AutoSizeDuration = transition_time,
                            AutoSizeEasing = Easing.OutQuint,
                            Children = new Drawable[]
                            {
                                settingsSection = new LoginSettings
                                {
                                    Padding = new MarginPadding(10),
                                    RequestHide = Hide,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Height = 3,
                                    Colour = colours.Yellow,
                                    Alpha = 1,
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            settingsSection.Bounding = true;
            this.FadeIn(transition_time, Easing.OutQuint);

            GetContainingInputManager().ChangeFocus(settingsSection);
        }

        protected override void PopOut()
        {
            base.PopOut();

            settingsSection.Bounding = false;
            this.FadeOut(transition_time);
        }
    }
}
