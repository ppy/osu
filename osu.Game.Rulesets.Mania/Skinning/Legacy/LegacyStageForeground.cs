// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyStageForeground : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Drawable? sprite;

        public LegacyStageForeground()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            string bottomImage = skin.GetManiaSkinConfig<string>(LegacyManiaSkinConfigurationLookups.BottomStageImage)?.Value
                                 ?? "mania-stage-bottom";

            sprite = skin.GetAnimation(bottomImage, true, true)?.With(d =>
            {
                if (d == null)
                    return;

                d.Scale = new Vector2(1.6f);
            });

            if (sprite != null)
                InternalChild = sprite;

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (sprite == null)
                return;

            if (direction.NewValue == ScrollingDirection.Up)
                sprite.Anchor = sprite.Origin = Anchor.TopCentre;
            else
                sprite.Anchor = sprite.Origin = Anchor.BottomCentre;
        }
    }
}
