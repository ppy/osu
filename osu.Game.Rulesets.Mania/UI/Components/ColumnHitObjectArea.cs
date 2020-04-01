// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnHitObjectArea : HitObjectArea
    {
        public readonly Container<HitExplosion> Explosions;
        private readonly Drawable hitTarget;

        public ColumnHitObjectArea(HitObjectContainer hitObjectContainer)
            : base(hitObjectContainer)
        {
            AddRangeInternal(new[]
            {
                hitTarget = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitTarget), _ => new DefaultHitTarget())
                {
                    RelativeSizeAxes = Axes.X,
                    Depth = 1
                },
                Explosions = new Container<HitExplosion>
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
            {
                hitTarget.Anchor = hitTarget.Origin = Anchor.TopLeft;
                Explosions.Padding = new MarginPadding { Top = DefaultNotePiece.NOTE_HEIGHT / 2 };
            }
            else
            {
                hitTarget.Anchor = hitTarget.Origin = Anchor.BottomLeft;
                Explosions.Padding = new MarginPadding { Bottom = DefaultNotePiece.NOTE_HEIGHT / 2 };
            }
        }
    }
}
