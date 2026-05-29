// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Input;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormPasswordTextBox : FormTextBox
    {
        internal override InnerTextBox CreateTextBox() => new InnerPasswordBox();
        internal partial class InnerPasswordBox : InnerTextBox
        {
            private readonly OsuPasswordTextBox.CapsWarning warning;

            protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
            {
                AutoSizeAxes = Axes.Both,
                Child = new OsuPasswordTextBox.PasswordMaskChar(FontSize),
            };

            protected override bool AllowUniqueCharacterSamples => false;

            [Resolved]
            private GameHost host { get; set; } = null!;

            public InnerPasswordBox()
            {
                InputProperties = new TextInputProperties(TextInputType.Password, false);

                Add(warning = new OsuPasswordTextBox.CapsWarning
                {
                    Size = new Vector2(16),
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 10 },
                    Alpha = 0,
                });
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (e.Key == Key.CapsLock)
                    updateCapsWarning(host.CapsLockEnabled);
                return base.OnKeyDown(e);
            }

            protected override void OnFocus(FocusEvent e)
            {
                updateCapsWarning(host.CapsLockEnabled);
                base.OnFocus(e);
            }

            protected override void OnFocusLost(FocusLostEvent e)
            {
                updateCapsWarning(false);
                base.OnFocusLost(e);
            }

            private void updateCapsWarning(bool visible) => warning.FadeTo(visible ? 1 : 0, 250, Easing.OutQuint);
        }
    }
}

