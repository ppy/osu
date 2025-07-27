// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary
{
    public partial class TestGameplayButton : OsuButton
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = OsuFont.TorusAlternate.With(weight: FontWeight.Light, size: 24),
            Shadow = false
        };

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            BackgroundColour = colours.Orange1;
            SpriteText.Colour = colourProvider.Background6;

            Content.CornerRadius = 0;

            Text = EditorStrings.TestBeatmap;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Background.FadeColour(colours.Orange0, 500, Easing.OutQuint);
            // don't call base in order to block scale animation
            return false;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Background.FadeColour(colours.Orange1, 300, Easing.OutQuint);
            // don't call base in order to block scale animation
        }
    }
}
