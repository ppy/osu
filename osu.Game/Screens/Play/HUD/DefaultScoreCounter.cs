// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultScoreCounter : ScoreCounter
    {
        private readonly Vector2 offset = new Vector2(20, 5);

        public DefaultScoreCounter()
            : base(6)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
        }

        [Resolved(canBeNull: true)]
        private HUDOverlay hud { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;

            // todo: check if default once health display is skinnable
            hud?.ShowHealthbar.BindValueChanged(healthBar =>
            {
                this.MoveToY(healthBar.NewValue ? 30 : 0, HUDOverlay.FADE_DURATION, HUDOverlay.FADE_EASING);
            }, true);
        }
    }
}
