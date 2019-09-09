// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherSprite : CompositeDrawable
    {
        public CatcherSprite()
        {
            Size = new Vector2(CatcherArea.CATCHER_SIZE);

            // Sets the origin roughly to the centre of the catcher's plate to allow for correct scaling.
            OriginPosition = new Vector2(-0.02f, 0.06f) * CatcherArea.CATCHER_SIZE;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new SkinnableSprite("Gameplay/catch/fruit-catcher-idle")
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            };
        }
    }
}
