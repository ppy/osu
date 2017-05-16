// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.General;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    internal class LoginOverlay : FocusedOverlayContainer
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
                new Box {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f,
                },
                new Container
                {
                    Width = 360,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    AutoSizeDuration = transition_time,
                    AutoSizeEasing = EasingTypes.OutQuint,
                    Children = new Drawable[]
                    {
                        settingsSection = new LoginSettings
                        {
                            Padding = new MarginPadding(10),
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
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            settingsSection.Bounding = true;
            FadeIn(transition_time, EasingTypes.OutQuint);

            settingsSection.TriggerFocus();
        }

        protected override void PopOut()
        {
            base.PopOut();

            settingsSection.Bounding = false;
            FadeOut(transition_time);
        }
    }
}
