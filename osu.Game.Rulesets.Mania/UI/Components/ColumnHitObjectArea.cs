// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public class ColumnHitObjectArea : SkinReloadableDrawable
    {
        public readonly Container<HitExplosion> Explosions;

        [Resolved(CanBeNull = true)]
        private ManiaStage stage { get; set; }

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly Drawable hitTarget;

        public ColumnHitObjectArea(HitObjectContainer hitObjectContainer)
        {
            InternalChildren = new[]
            {
                hitTarget = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HitTarget), _ => new DefaultHitTarget())
                {
                    RelativeSizeAxes = Axes.X,
                },
                hitObjectContainer,
                Explosions = new Container<HitExplosion> { RelativeSizeAxes = Axes.Both }
            };
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);
            updateHitPosition();
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            updateHitPosition();
        }

        private void updateHitPosition()
        {
            float hitPosition = CurrentSkin.GetConfig<LegacyManiaSkinConfigurationLookup, float>(
                                    new LegacyManiaSkinConfigurationLookup(stage?.Columns.Count ?? 4, LegacyManiaSkinConfigurationLookups.HitPosition))?.Value
                                ?? ManiaStage.HIT_TARGET_POSITION;

            if (direction.Value == ScrollingDirection.Up)
            {
                hitTarget.Anchor = hitTarget.Origin = Anchor.TopLeft;

                Padding = new MarginPadding { Top = hitPosition };
                Explosions.Padding = new MarginPadding { Top = DefaultNotePiece.NOTE_HEIGHT / 2 };
            }
            else
            {
                hitTarget.Anchor = hitTarget.Origin = Anchor.BottomLeft;

                Padding = new MarginPadding { Bottom = hitPosition };
                Explosions.Padding = new MarginPadding { Bottom = DefaultNotePiece.NOTE_HEIGHT / 2 };
            }
        }
    }
}
