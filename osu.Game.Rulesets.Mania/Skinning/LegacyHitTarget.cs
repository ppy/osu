// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyHitTarget : CompositeDrawable
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        [Resolved(CanBeNull = true)]
        private ManiaStage stage { get; set; }

        private Container directionContainer;

        public LegacyHitTarget()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            string targetImage = skin.GetConfig<LegacyManiaSkinConfigurationLookup, string>(
                                     new LegacyManiaSkinConfigurationLookup(stage?.Columns.Count ?? 4, LegacyManiaSkinConfigurationLookups.HitTargetImage))?.Value
                                 ?? "mania-stage-hint";

            InternalChild = directionContainer = new Container
            {
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = new Sprite
                {
                    Texture = skin.GetTexture(targetImage),
                    Scale = new Vector2(1, 0.9f * 1.6025f),
                    RelativeSizeAxes = Axes.X,
                    Width = 1
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                directionContainer.Anchor = Anchor.TopLeft;
                directionContainer.Scale = new Vector2(1, -1);
            }
            else
            {
                directionContainer.Anchor = Anchor.BottomLeft;
                directionContainer.Scale = Vector2.One;
            }
        }
    }
}
