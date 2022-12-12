// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Catch.Configuration;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI.Cursor
{
    public partial class CatchCursor : SkinReloadableDrawable
    {
        private const float size = 28;

        private CatchCursorSprite cursorSprite;

        private readonly Bindable<bool> configShowCursorDuringPlay = new Bindable<bool>();

        public CatchCursor()
        {
            Origin = Anchor.Centre;

            Size = new Vector2(size);
        }

        private void OnCursorVisibilityChanged(bool visibility)
        {
            if (visibility)
                this.FadeTo(1, 250, Easing.InQuint);
            else
                this.FadeTo(0, 250, Easing.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(CatchRulesetConfigManager rulesetConfig)
        {
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Child = cursorSprite = new DefaultCursor(), //not currently skinnable
            };
            rulesetConfig?.BindWith(CatchRulesetSetting.ShowCursorDuringPlay, configShowCursorDuringPlay);
            configShowCursorDuringPlay.BindValueChanged(show => OnCursorVisibilityChanged(show.NewValue), true);
        }

        private partial class DefaultCursor : CatchCursorSprite
        {
            //placeholder until it is decided what the default catch cursor should be
            public DefaultCursor()
            {
                RelativeSizeAxes = Axes.Both;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChildren = new[]
                {
                    ExpandTarget = new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderThickness = size / 6,
                        BorderColour = Color4.White,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Pink.Opacity(0.5f),
                            Radius = 5,
                        },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            },
                            new CircularContainer
                            {
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                BorderThickness = size / 3,
                                BorderColour = Color4.White.Opacity(0.5f),
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                        AlwaysPresent = true,
                                    },
                                },
                            },
                        },
                    },
                    new Circle
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(0.14f),
                        Colour = new Color4(34, 93, 204, 255),
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Radius = 8,
                            Colour = Color4.White,
                        },
                    },
                };
            }
        }
    }
}
