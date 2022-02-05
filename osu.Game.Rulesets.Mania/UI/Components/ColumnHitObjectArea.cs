// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnHitObjectArea : HitObjectArea
    {
        public readonly Container Explosions;

        public readonly Container UnderlayElements;

        private readonly Drawable hitTarget;

        public ColumnHitObjectArea(HitObjectContainer hitObjectContainer)
            : base(hitObjectContainer)
        {
            AddRangeInternal(new[]
            {
                UnderlayElements = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 2,
                },
                hitTarget = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitTarget), _ => new DefaultHitTarget())
                {
                    RelativeSizeAxes = Axes.X,
                    Depth = 1
                },
                Explosions = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1,
                }
            });
        }

        protected override void UpdateHitPosition()
        {
            base.UpdateHitPosition();

            if (Direction.Value == ScrollingDirection.Up)
                hitTarget.Anchor = hitTarget.Origin = Anchor.TopLeft;
            else
                hitTarget.Anchor = hitTarget.Origin = Anchor.BottomLeft;
        }
    }
}
