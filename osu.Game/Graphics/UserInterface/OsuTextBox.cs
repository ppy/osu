﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTextBox : TextBox, IKeyBindingHandler<GlobalAction>
    {
        protected override Color4 BackgroundUnfocused => Color4.Black.Opacity(0.5f);
        protected override Color4 BackgroundFocused => OsuColour.Gray(0.3f).Opacity(0.8f);
        protected override Color4 BackgroundCommit => BorderColour;

        protected override float LeftRightPadding => 10;

        protected override SpriteText CreatePlaceholder() => new OsuSpriteText
        {
            Font = @"Exo2.0-MediumItalic",
            Colour = new Color4(180, 180, 180, 255),
            Margin = new MarginPadding { Left = 2 },
        };

        public OsuTextBox()
        {
            Height = 40;
            TextContainer.Height = 0.5f;
            CornerRadius = 5;

            Current.DisabledChanged += disabled => { Alpha = disabled ? 0.3f : 1; };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            BorderColour = colour.Yellow;
        }

        protected override void OnFocus(FocusEvent e)
        {
            BorderThickness = 3;
            base.OnFocus(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            BorderThickness = 0;

            base.OnFocusLost(e);
        }

        protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText { Text = c.ToString(), TextSize = CalculatedTextSize };

        public virtual bool OnPressed(GlobalAction action)
        {
            if (action == GlobalAction.Back)
            {
                KillFocus();
                return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;
    }
}
