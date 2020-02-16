// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;
using osu.Framework.Graphics.Effects;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchTextBox : SearchTextBox
    {
        private const int fade_duration = 300;

        public BeatmapSearchTextBox()
        {
            Height = 47;
            TextContainer.Height = 0.42f;
            CornerRadius = 0;
            PlaceholderText = @"type in keywords...";
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = Color4.White,
                Radius = 10,
                Roundness = 5,
            };

            FadeEdgeEffectTo(0);
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            BackgroundUnfocused = BackgroundFocused = Color4.White;
            IconColour = colourProvider.Light1;
            Placeholder.Colour = colourProvider.Foreground1;
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);
            FadeEdgeEffectTo(0.5f, fade_duration, Easing.OutQuint);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            base.OnFocusLost(e);
            FadeEdgeEffectTo(0, fade_duration, Easing.OutQuint);
        }

        protected override Caret CreateCaret() => base.CreateCaret().With(c => ((OsuCaret)c).CaretColour = Color4.Black);

        protected override Color4 SelectionColour => Color4.Gray;

        protected override SpriteText CreatePlaceholder() => new OsuSpriteText
        {
            Font = OsuFont.GetFont(weight: FontWeight.Light, italics: true),
            Shadow = false
        };

        protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: CalculatedTextSize, weight: FontWeight.Light, italics: true),
            Colour = Color4.Black,
            Shadow = false,
            Text = c.ToString()
        };
    }
}
