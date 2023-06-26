// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public partial class HitObjectArea : SkinReloadableDrawable
    {
        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();
        public readonly HitObjectContainer HitObjectContainer;

        public HitObjectArea(HitObjectContainer hitObjectContainer)
        {
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = HitObjectContainer = hitObjectContainer
            };
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            Direction.BindTo(scrollingInfo.Direction);
            Direction.BindValueChanged(onDirectionChanged, true);
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);
            UpdateHitPosition();
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            UpdateHitPosition();
        }

        protected virtual void UpdateHitPosition()
        {
            float hitPosition = CurrentSkin.GetConfig<ManiaSkinConfigurationLookup, float>(
                                    new ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups.HitPosition))?.Value
                                ?? Stage.HIT_TARGET_POSITION;

            Padding = Direction.Value == ScrollingDirection.Up
                ? new MarginPadding { Top = hitPosition }
                : new MarginPadding { Bottom = hitPosition };
        }
    }
}
