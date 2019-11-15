// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public class OsuCursor : SkinReloadableDrawable
    {
        private const float size = 28;

        private bool cursorExpand;

        private Container expandTarget;

        public OsuCursor()
        {
            Origin = Anchor.Centre;

            Size = new Vector2(size);
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            cursorExpand = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.CursorExpand)?.Value ?? true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = expandTarget = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Child = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.Cursor), _ => new DefaultCursor(), confineMode: ConfineMode.NoScaling)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            };
        }

        private const float pressed_scale = 1.2f;
        private const float released_scale = 1f;

        public void Expand()
        {
            if (!cursorExpand) return;

            expandTarget.ScaleTo(released_scale).ScaleTo(pressed_scale, 100, Easing.OutQuad);
        }

        public void Contract() => expandTarget.ScaleTo(released_scale, 100, Easing.OutQuad);

        private class DefaultCursor : CompositeDrawable
        {
            public DefaultCursor()
            {
                RelativeSizeAxes = Axes.Both;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    new CircularContainer
                    {
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
                            new CircularContainer
                            {
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Scale = new Vector2(0.1f),
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.White,
                                    },
                                },
                            },
                        }
                    }
                };
            }
        }
    }
}
