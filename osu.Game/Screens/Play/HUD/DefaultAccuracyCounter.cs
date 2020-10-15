// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultAccuracyCounter : PercentageCounter, IAccuracyCounter
    {
        private readonly Vector2 offset = new Vector2(-20, 5);

        public DefaultAccuracyCounter()
        {
            Origin = Anchor.TopRight;
            Anchor = Anchor.TopRight;
        }

        [Resolved(canBeNull: true)]
        private HUDOverlay hud { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
        }

        protected override void Update()
        {
            base.Update();

            if (hud?.ScoreCounter.Drawable is DefaultScoreCounter score)
            {
                // for now align with the score counter. eventually this will be user customisable.
                Anchor = Anchor.TopLeft;
                Position = Parent.ToLocalSpace(score.ScreenSpaceDrawQuad.TopLeft) + offset;
            }
        }
    }
}
