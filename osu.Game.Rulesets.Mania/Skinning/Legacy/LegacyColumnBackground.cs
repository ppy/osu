// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyColumnBackground : LegacyManiaColumnElement, IKeyBindingHandler<ManiaAction>
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private Container lightContainer = null!;
        private Drawable light = null!;

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

            int lightFramePerSecond = skin.GetManiaSkinConfig<int>(LegacyManiaSkinConfigurationLookups.LightFramePerSecond)?.Value ?? 60;

            InternalChildren = new[]
            {
                lightContainer = new Container
                {
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = lightPosition },
                    Child = light = skin.GetAnimation(lightImage, true, true, frameLength: 1000d / lightFramePerSecond)?.With(l =>
                    {
                        l.Anchor = Anchor.BottomCentre;
                        l.Origin = Anchor.BottomCentre;
                        l.Colour = LegacyColourCompatibility.DisallowZeroAlpha(lightColour);
                        l.RelativeSizeAxes = Axes.X;
                        l.Width = 1;
                        l.Alpha = 0;
                    }) ?? Empty(),
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

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action == Column.Action.Value)
            {
                light.FadeIn();
                light.ScaleTo(Vector2.One);
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
            // Todo: Should be 400 * 100 / CurrentBPM
            const double animation_length = 250;

            if (e.Action == Column.Action.Value)
            {
                light.FadeTo(0, animation_length);
                light.ScaleTo(new Vector2(1, 0), animation_length);
            }
        }
    }
}
