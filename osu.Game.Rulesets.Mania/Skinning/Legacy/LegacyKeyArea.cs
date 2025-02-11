// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyKeyArea : LegacyManiaColumnElement, IKeyBindingHandler<ManiaAction>
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container directionContainer = null!;
        private Sprite upSprite = null!;
        private Sprite downSprite = null!;

        [Resolved]
        private Column column { get; set; } = null!;

        public LegacyKeyArea()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            string upImage = GetColumnSkinConfig<string>(skin, LegacyManiaSkinConfigurationLookups.KeyImage)?.Value
                             ?? $"mania-key{FallbackColumnIndex}";

            string downImage = GetColumnSkinConfig<string>(skin, LegacyManiaSkinConfigurationLookups.KeyImageDown)?.Value
                               ?? $"mania-key{FallbackColumnIndex}D";

            InternalChild = directionContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    // Key images are placed side-to-side on the playfield, therefore ClampToEdge must be used to prevent any gaps between each key.
                    upSprite = new Sprite
                    {
                        Origin = Anchor.BottomCentre,
                        Texture = skin.GetTexture(upImage, WrapMode.ClampToEdge, default),
                        RelativeSizeAxes = Axes.X,
                        Width = 1
                    },
                    downSprite = new Sprite
                    {
                        Origin = Anchor.BottomCentre,
                        Texture = skin.GetTexture(downImage, WrapMode.ClampToEdge, default),
                        RelativeSizeAxes = Axes.X,
                        Width = 1,
                        Alpha = 0
                    }
                }
            };

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            if (GetColumnSkinConfig<bool>(skin, LegacyManiaSkinConfigurationLookups.KeysUnderNotes)?.Value ?? false)
                Column.UnderlayElements.Add(CreateProxy());
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

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action == column.Action.Value)
            {
                upSprite.FadeTo(0);
                downSprite.FadeTo(1);
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
            if (e.Action == column.Action.Value)
            {
                upSprite.Delay(LegacyHitExplosion.FADE_IN_DURATION).FadeTo(1);
                downSprite.Delay(LegacyHitExplosion.FADE_IN_DURATION).FadeTo(0);
            }
        }
    }
}
