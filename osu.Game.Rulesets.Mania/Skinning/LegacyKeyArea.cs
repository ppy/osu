// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyKeyArea : CompositeDrawable, IKeyBindingHandler<ManiaAction>
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container directionContainer;
        private Sprite upSprite;
        private Sprite downSprite;

        [Resolved(CanBeNull = true)]
        private ManiaStage stage { get; set; }

        [Resolved]
        private Column column { get; set; }

        public LegacyKeyArea()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            int fallbackColumn = column.Index % 2 + 1;

            string upImage = skin.GetConfig<LegacyManiaSkinConfigurationLookup, string>(
                                 new LegacyManiaSkinConfigurationLookup(stage?.Columns.Count ?? 4, LegacyManiaSkinConfigurationLookups.KeyImage, column.Index))?.Value
                             ?? $"mania-key{fallbackColumn}";

            string downImage = skin.GetConfig<LegacyManiaSkinConfigurationLookup, string>(
                                   new LegacyManiaSkinConfigurationLookup(stage?.Columns.Count ?? 4, LegacyManiaSkinConfigurationLookups.KeyImageDown, column.Index))?.Value
                               ?? $"mania-key{fallbackColumn}D";

            InternalChild = directionContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    upSprite = new Sprite
                    {
                        Origin = Anchor.BottomCentre,
                        Texture = skin.GetTexture(upImage),
                        RelativeSizeAxes = Axes.X,
                        Width = 1
                    },
                    downSprite = new Sprite
                    {
                        Origin = Anchor.BottomCentre,
                        Texture = skin.GetTexture(downImage),
                        RelativeSizeAxes = Axes.X,
                        Width = 1,
                        Alpha = 0
                    }
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                directionContainer.Anchor = directionContainer.Origin = Anchor.TopCentre;
                upSprite.Anchor = downSprite.Anchor = Anchor.TopCentre;
                upSprite.Scale = downSprite.Scale = new Vector2(1, -1);
            }
            else
            {
                directionContainer.Anchor = directionContainer.Origin = Anchor.BottomCentre;
                upSprite.Anchor = downSprite.Anchor = Anchor.BottomCentre;
                upSprite.Scale = downSprite.Scale = Vector2.One;
            }
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action == column.Action.Value)
            {
                upSprite.FadeTo(0);
                downSprite.FadeTo(1);
            }

            return false;
        }

        public void OnReleased(ManiaAction action)
        {
            if (action == column.Action.Value)
            {
                upSprite.FadeTo(1);
                downSprite.FadeTo(0);
            }
        }
    }
}
