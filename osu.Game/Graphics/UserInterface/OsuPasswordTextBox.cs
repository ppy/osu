// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using System;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuPasswordTextBox : OsuTextBox
    {
        protected override Drawable GetDrawableCharacter(char c) => new PasswordMaskChar(CalculatedTextSize);

        public override bool AllowClipboardExport => false;

        public OsuPasswordTextBox()
        {
            Add(new CapsWarning
            {
                Size = new Vector2(20),
            });
        }

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
            public string TooltipText => Console.CapsLock ? @"Caps lock is active" : string.Empty;

            public override bool HandleInput => true;

            public CapsWarning()
            {
                Icon = FontAwesome.fa_warning;
                Origin = Anchor.CentreRight;
                Anchor = Anchor.CentreRight;
                Margin = new MarginPadding { Right = 10 };
                AlwaysPresent = true;
                Alpha = 0;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                Colour = colour.YellowLight;
            }

            protected override void Update()
            {
                base.Update();
                updateVisibility();
            }

            private void updateVisibility() => this.FadeTo(Console.CapsLock ? 1 : 0, 250, Easing.OutQuint);
        }
    }
}
