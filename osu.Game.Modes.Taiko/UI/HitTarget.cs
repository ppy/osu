// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Modes.Taiko.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.UI
{
    class HitTarget : Container
    {
        private Sprite outer;
        private Sprite inner;

        private Container flashContainer;
        private Container innerFlash;
        private Container outerFlash;

        public HitTarget()
        {
            Children = new Drawable[]
            {
                new Box()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Size = new Vector2(5, TaikoPlayfield.PLAYFIELD_HEIGHT),

                    Colour = Color4.Black
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2 * 1.5f),

                    Children = new Drawable[]
                    {
                        outer = new Sprite()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,
                        },
                        inner = new Sprite()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1 / 1.5f)
                        },
                        flashContainer = new Container()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

                            Alpha = 0,

                            Children = new Drawable[]
                            {
                                outerFlash = new Container()
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    Size = new Vector2(74.2f, 530),

                                    Masking = true,
                                    CornerRadius = 74.2f,

                                    Children = new Drawable[]
                                    {
                                        outerFlash = new Container()
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,

                                            RelativeSizeAxes = Axes.Both,

                                            Masking = true,

                                            Children = new[]
                                            {
                                                new Box()
                                                {
                                                    Alpha = 0,
                                                    AlwaysPresent = true
                                                }
                                            }
                                        }
                                    }
                                },
                                innerFlash = new CircularContainer()
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    RelativeSizeAxes = Axes.Both,

                                    Masking = true,

                                    Children = new[]
                                    {
                                        new Box()
                                        {
                                            RelativeSizeAxes = Axes.Both,

                                            Colour = Color4.White,
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

        public void Flash(Color4 colour, bool showOuter = true)
        {
            outerFlash.Alpha = showOuter ? 0.3f : 0;

            innerFlash.EdgeEffect = new EdgeEffect()
            {
                Type = EdgeEffectType.Glow,
                Colour = colour,
                Radius = 20
            };

            outerFlash.EdgeEffect = new EdgeEffect()
            {
                Type = EdgeEffectType.Glow,
                Colour = colour,
                Radius = 250
            };

            flashContainer.FadeIn(50);
            flashContainer.Delay(50).FadeOut(100);
        }
    }
}
