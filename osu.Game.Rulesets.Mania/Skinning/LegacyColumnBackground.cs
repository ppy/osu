// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyColumnBackground : LegacyManiaColumnElement, IKeyBindingHandler<ManiaAction>
    {
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly bool isLastColumn;

        [CanBeNull]
        private readonly LegacyStageBackground stageBackground;

        private Container hitTargetContainer;
        private Container lightContainer;
        private Sprite light;
        private Drawable hitTarget;

        private float hitPosition;

        public LegacyColumnBackground(bool isLastColumn, [CanBeNull] LegacyStageBackground stageBackground)
        {
            this.isLastColumn = isLastColumn;
            this.stageBackground = stageBackground;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo)
        {
            string lightImage = skin.GetManiaSkinConfig<string>(LegacyManiaSkinConfigurationLookups.LightImage)?.Value
                                ?? "mania-stage-light";

            float leftLineWidth = GetColumnSkinConfig<float>(skin, LegacyManiaSkinConfigurationLookups.LeftLineWidth)
                ?.Value ?? 1;
            float rightLineWidth = GetColumnSkinConfig<float>(skin, LegacyManiaSkinConfigurationLookups.RightLineWidth)
                ?.Value ?? 1;

            bool hasLeftLine = leftLineWidth > 0;
            bool hasRightLine = rightLineWidth > 0 && skin.GetConfig<LegacySkinConfiguration.LegacySetting, decimal>(LegacySkinConfiguration.LegacySetting.Version)?.Value >= 2.4m
                                || isLastColumn;
            bool hasHitTarget = Column.Index == 0 || stageBackground == null;

            hitPosition = GetColumnSkinConfig<float>(skin, LegacyManiaSkinConfigurationLookups.HitPosition)?.Value
                          ?? Stage.HIT_TARGET_POSITION;

            float lightPosition = GetColumnSkinConfig<float>(skin, LegacyManiaSkinConfigurationLookups.LightPosition)?.Value
                                  ?? 0;

            Color4 lineColour = GetColumnSkinConfig<Color4>(skin, LegacyManiaSkinConfigurationLookups.ColumnLineColour)?.Value
                                ?? Color4.White;

            Color4 backgroundColour = GetColumnSkinConfig<Color4>(skin, LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour)?.Value
                                      ?? Color4.Black;

            Color4 lightColour = GetColumnSkinConfig<Color4>(skin, LegacyManiaSkinConfigurationLookups.ColumnLightColour)?.Value
                                 ?? Color4.White;

            Drawable background;

            InternalChildren = new[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = backgroundColour
                },
                hitTargetContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        // In legacy skins, the hit target takes on the full stage size and is sandwiched between the column background and the column light.
                        // To simulate this effect in lazer's hierarchy, the hit target is added to the first column's background and manually extended to the full size of the stage.
                        // Adding to the first columns allows depth issues to be resolved - if it were added to the last column, the previous column lights would appear below it.
                        // This still means that the hit target will appear below the next column backgrounds, but that's a much easier problem to solve by proxying the backgrounds below.
                        hitTarget = new LegacyHitTarget
                        {
                            RelativeSizeAxes = Axes.Y,
                            Alpha = hasHitTarget ? 1 : 0
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = leftLineWidth,
                            Scale = new Vector2(0.740f, 1),
                            Colour = lineColour,
                            Alpha = hasLeftLine ? 1 : 0
                        },
                        new Box
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = rightLineWidth,
                            Scale = new Vector2(0.740f, 1),
                            Colour = lineColour,
                            Alpha = hasRightLine ? 1 : 0
                        }
                    }
                },
                lightContainer = new Container
                {
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = lightPosition },
                    Child = light = new Sprite
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Colour = lightColour,
                        Texture = skin.GetTexture(lightImage),
                        RelativeSizeAxes = Axes.X,
                        Width = 1,
                        Alpha = 0
                    }
                }
            };

            // Resolve depth issues with the hit target appearing under the next column backgrounds by proxying to the stage background (always below the columns).
            stageBackground?.AddColumnBackground(background.CreateProxy());

            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                lightContainer.Anchor = Anchor.TopCentre;
                lightContainer.Scale = new Vector2(1, -1);

                hitTargetContainer.Padding = new MarginPadding { Top = hitPosition };
            }
            else
            {
                lightContainer.Anchor = Anchor.BottomCentre;
                lightContainer.Scale = Vector2.One;

                hitTargetContainer.Padding = new MarginPadding { Bottom = hitPosition };
            }
        }

        protected override void Update()
        {
            base.Update();
            hitTarget.Width = stageBackground?.DrawWidth ?? DrawWidth;
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
