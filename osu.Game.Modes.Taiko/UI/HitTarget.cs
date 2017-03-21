// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Game.Modes.Taiko.Objects;

namespace osu.Game.Modes.Taiko.UI
{
    internal class HitTarget : Container
    {
        private Sprite outer;
        private Sprite inner;

        private Container innerFlash;
        private Container outerFlash;

        public HitTarget()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Size = new Vector2(5, TaikoPlayfield.PlayfieldHeight),

                    Colour = Color4.Black
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2 * 1.5f),
                    Scale = new Vector2(TaikoPlayfield.PLAYFIELD_SCALE),

                    Children = new Drawable[]
                    {
                        outer = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,
                        },
                        inner = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1 / 1.5f)
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

                            Children = new Drawable[]
                            {
                                outerFlash = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2 * 1.5f, 680),

                                    Masking = true,
                                    CornerRadius = TaikoHitObject.CIRCLE_RADIUS * 2 * 1.5f,

                                    Alpha = 0,

                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,

                                            Alpha = 0,
                                            AlwaysPresent = true
                                        }
                                    }
                                },
                                innerFlash = new CircularContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    RelativeSizeAxes = Axes.Both,

                                    Masking = true,

                                    Alpha = 0,

                                    Children = new[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,

                                            Colour = Color4.White.Opacity(0.85f),
                                        }
                                    },
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            outer.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer");
            inner.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner");
        }

        public void Flash(Color4 colour)
        {
            innerFlash.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = colour,
                Radius = 20
            };

            outerFlash.EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = colour,
                Radius = 250
            };

            outerFlash.FadeTo(0.3f, 125, EasingTypes.OutQuint);
            outerFlash.Delay(125).FadeOut(125);

            innerFlash.FadeIn();
            innerFlash.FadeOut(250, EasingTypes.OutQuint);
        }
    }
}
