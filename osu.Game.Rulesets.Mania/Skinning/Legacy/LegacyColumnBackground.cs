// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public class LegacyColumnBackground : LegacyManiaColumnElement, IKeyBindingHandler<ManiaAction>
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container lightContainer;
        private Sprite light;

        public LegacyColumnBackground()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            string lightImage = skin.GetManiaSkinConfig<string>(LegacyManiaSkinConfigurationLookups.LightImage)?.Value
                                ?? "mania-stage-light";

            float lightPosition = GetColumnSkinConfig<float>(skin, LegacyManiaSkinConfigurationLookups.LightPosition)?.Value
                                  ?? 0;

            Color4 lightColour = GetColumnSkinConfig<Color4>(skin, LegacyManiaSkinConfigurationLookups.ColumnLightColour)?.Value
                                 ?? Color4.White;

            InternalChildren = new[]
            {
                lightContainer = new Container
                {
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = lightPosition },
                    Child = light = new Sprite
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Colour = LegacyColourCompatibility.DisallowZeroAlpha(lightColour),
                        Texture = skin.GetTexture(lightImage),
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
                lightContainer.Anchor = Anchor.TopCentre;
                lightContainer.Scale = new Vector2(1, -1);
            }
            else
            {
                lightContainer.Anchor = Anchor.BottomCentre;
                lightContainer.Scale = Vector2.One;
            }
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action == Column.Action.Value)
            {
                light.FadeIn();
                light.ScaleTo(Vector2.One);
            }

            return false;
        }

        public void OnReleased(ManiaAction action)
        {
            // Todo: Should be 400 * 100 / CurrentBPM
            const double animation_length = 250;

            if (action == Column.Action.Value)
            {
                light.FadeTo(0, animation_length);
                light.ScaleTo(new Vector2(1, 0), animation_length);
            }
        }
    }
}
