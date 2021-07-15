// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
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
            InternalChild = new BorderPiece();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            Colour = osuColour.Yellow;
        }

        public void UpdateFrom(ScrollingHitObjectContainer hitObjectContainer, CatchHitObject hitObject, [CanBeNull] CatchHitObject parent = null)
        {
            X = hitObject.EffectiveX - (parent?.OriginalX ?? 0);
            Y = hitObjectContainer.PositionAtTime(hitObject.StartTime, parent?.StartTime ?? hitObjectContainer.Time.Current);
            Scale = new Vector2(hitObject.Scale);
        }
    }
}
