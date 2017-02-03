﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics;
using osu.Game.Overlays.Options.General;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    class LoginOverlay : OverlayContainer
    {
        private LoginOptions optionsSection;

        const float transition_time = 400;

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
                    Alpha = 0.8f,
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
                        optionsSection = new LoginOptions
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
            optionsSection.Bounding = true;
            FadeIn(transition_time, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            optionsSection.Bounding = false;
            FadeOut(transition_time);
        }
    }
}
