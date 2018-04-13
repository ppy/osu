// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Platform;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuPasswordTextBox : OsuTextBox
    {
        protected override Drawable GetDrawableCharacter(char c) => new PasswordMaskChar(CalculatedTextSize);

        public override bool AllowClipboardExport => false;

        private readonly CapsWarning warning;

        private GameHost host;

        public OsuPasswordTextBox()
        {
            Add(warning = new CapsWarning
            {
                Size = new Vector2(20),
                Origin = Anchor.CentreRight,
                Anchor = Anchor.CentreRight,
                Margin = new MarginPadding { Right = 10 },
                Alpha = 0,
            });
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            this.host = host;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.CapsLock)
                updateCapsWarning(host.CapsLockEnabled);
            return base.OnKeyDown(state, args);
        }

        protected override void OnFocus(InputState state)
        {
            updateCapsWarning(host.CapsLockEnabled);
            base.OnFocus(state);
        }

        protected override void OnFocusLost(InputState state)
        {
            updateCapsWarning(false);
            base.OnFocusLost(state);
        }

        private void updateCapsWarning(bool visible) => warning.FadeTo(visible ? 1 : 0, 250, Easing.OutQuint);

        public class PasswordMaskChar : Container
        {
            private readonly CircularContainer circle;

            public PasswordMaskChar(float size)
            {
                Size = new Vector2(size / 2, size);
                Children = new[]
                {
                    circle = new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.8f, 0),
                        Children = new[]
                        {
                            new Box
                            {
                                Colour = Color4.White,
                                RelativeSizeAxes = Axes.Both,
                            }
                        },
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                circle.FadeIn(500, Easing.OutQuint);
                circle.ResizeTo(new Vector2(0.8f), 500, Easing.OutQuint);
            }
        }

        private class CapsWarning : SpriteIcon, IHasTooltip
        {
            public string TooltipText => @"Caps lock is active";

            public CapsWarning()
            {
                Icon = FontAwesome.fa_warning;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                Colour = colour.YellowLight;
            }
        }
    }
}
