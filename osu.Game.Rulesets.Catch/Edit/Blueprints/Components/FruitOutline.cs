// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public class FruitOutline : CompositeDrawable
    {
        public FruitOutline()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.Centre;
            Size = new Vector2(2 * CatchHitObject.OBJECT_RADIUS);
            InternalChild = new BorderPiece();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            Colour = osuColour.Yellow;
        }

        public void UpdateFrom(ScrollingHitObjectContainer hitObjectContainer, CatchHitObject hitObject)
        {
            X = hitObject.EffectiveX;
            Y = hitObjectContainer.PositionAtTime(hitObject.StartTime);
            Scale = new Vector2(hitObject.Scale);
        }
    }
}
