// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public partial class ColumnHitObjectArea : HitPositionPaddedContainer
    {
        public readonly Container Explosions;

        public readonly Container UnderlayElements;

        private readonly Drawable hitTarget;

        protected override Container<Drawable> Content => content;

        private readonly Container content;

        public ColumnHitObjectArea()
        {
            AddRangeInternal(new[]
            {
                UnderlayElements = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                hitTarget = new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.HitTarget), _ => new DefaultHitTarget())
                {
                    RelativeSizeAxes = Axes.X,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                Explosions = new Container
                {
                    RelativeSizeAxes = Axes.Both,
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
