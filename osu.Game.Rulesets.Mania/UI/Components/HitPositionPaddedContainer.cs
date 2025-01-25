// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public partial class HitPositionPaddedContainer : Container
    {
        protected readonly IBindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            Direction.BindTo(scrollingInfo.Direction);
            Direction.BindValueChanged(onDirectionChanged);

            skin.SourceChanged += onSkinChanged;

            UpdateHitPosition();
        }

        private void onSkinChanged() => UpdateHitPosition();
        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction) => UpdateHitPosition();

        protected virtual void UpdateHitPosition()
        {
            float hitPosition = skin.GetConfig<ManiaSkinConfigurationLookup, float>(
                                    new ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups.HitPosition))?.Value
                                ?? Stage.HIT_TARGET_POSITION;

            Padding = Direction.Value == ScrollingDirection.Up
                ? new MarginPadding { Top = hitPosition }
                : new MarginPadding { Bottom = hitPosition };
        }
    }
}
