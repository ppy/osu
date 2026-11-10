// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osuTK;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormPasswordTextBox : FormTextBox
    {
        private CapsWarning capsWarning = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            CaptionContainer.Add(capsWarning = new CapsWarning
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Alpha = 0,
            });
        }

        internal override InnerTextBox CreateTextBox() => new InnerPasswordBox
        {
            UpdateCapsWarning = visible => capsWarning.FadeTo(visible ? 1 : 0, 250, Easing.OutQuint),
        };

        internal partial class InnerPasswordBox : InnerTextBox
        {
            public Action<bool>? UpdateCapsWarning { get; init; }

            [Resolved]
            private GameHost host { get; set; } = null!;

            protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
            {
                AutoSizeAxes = Axes.Both,
                Child = new OsuPasswordTextBox.PasswordMaskChar(FontSize),
            };

            protected override bool AllowUniqueCharacterSamples => false;

            public InnerPasswordBox()
            {
                InputProperties = new TextInputProperties(TextInputType.Password, false);
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (e.Key == Key.CapsLock)
                    UpdateCapsWarning?.Invoke(host.CapsLockEnabled);

                return base.OnKeyDown(e);
            }

            protected override void OnFocus(FocusEvent e)
            {
                UpdateCapsWarning?.Invoke(host.CapsLockEnabled);
                base.OnFocus(e);
            }

            protected override void OnFocusLost(FocusLostEvent e)
            {
                UpdateCapsWarning?.Invoke(false);
                base.OnFocusLost(e);
            }
        }

        private partial class CapsWarning : OsuTextFlowContainer
        {
            public CapsWarning()
                : base(t => t.Font = OsuFont.Style.Caption1)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AutoSizeAxes = Axes.Both;
                Colour = colours.YellowLight;

                AddArbitraryDrawable(new SpriteIcon
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Size = new Vector2(10),
                    Icon = FontAwesome.Solid.ExclamationTriangle,
                    Margin = new MarginPadding { Right = 2 },
                    Y = 1f,
                });
                AddText(CommonStrings.CapsLock);
            }
        }
    }
}
