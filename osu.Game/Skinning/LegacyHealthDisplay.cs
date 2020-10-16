// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacyHealthDisplay : CompositeDrawable, IHealthDisplay
    {
        private readonly Skin skin;
        private Drawable fill;
        private LegacyMarker marker;

        private float maxFillWidth;

        private bool isNewStyle;

        public Bindable<double> Current { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1
        };

        public LegacyHealthDisplay(Skin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            isNewStyle = getTexture(skin, "marker") != null;

            // background implementation is the same for both versions.
            AddInternal(new Sprite { Texture = getTexture(skin, "bg") });

            if (isNewStyle)
            {
                AddRangeInternal(new[]
                {
                    fill = new LegacyNewStyleFill(skin),
                    marker = new LegacyNewStyleMarker(skin),
                });
            }
            else
            {
                AddRangeInternal(new[]
                {
                    fill = new LegacyOldStyleFill(skin),
                    marker = new LegacyOldStyleMarker(skin),
                });
            }

            marker.Current.BindTo(Current);
            maxFillWidth = fill.Width;
        }

        protected override void Update()
        {
            base.Update();

            fill.Width = Interpolation.ValueAt(
                Math.Clamp(Clock.ElapsedFrameTime, 0, 200),
                fill.Width, (float)Current.Value * maxFillWidth, 0, 200, Easing.OutQuint);

            marker.Position = fill.Position + new Vector2(fill.DrawWidth, fill.DrawHeight / 2);
        }

        public void Flash(JudgementResult result) => marker.Flash(result);

        private static Texture getTexture(Skin skin, string name) => skin.GetTexture($"scorebar-{name}");

        public class LegacyOldStyleMarker : LegacyMarker
        {
            public LegacyOldStyleMarker(Skin skin)
            {
                InternalChildren = new Drawable[]
                {
                    new Sprite
                    {
                        Texture = getTexture(skin, "ki"),
                        Origin = Anchor.Centre,
                    }
                };
            }
        }

        public class LegacyNewStyleMarker : LegacyMarker
        {
            public LegacyNewStyleMarker(Skin skin)
            {
                InternalChildren = new Drawable[]
                {
                    new Sprite
                    {
                        Texture = getTexture(skin, "marker"),
                        Origin = Anchor.Centre,
                    }
                };
            }
        }

        public class LegacyMarker : CompositeDrawable, IHealthDisplay
        {
            public Bindable<double> Current { get; } = new Bindable<double>();

            public LegacyMarker()
            {
                Origin = Anchor.Centre;
            }

            public void Flash(JudgementResult result)
            {
                this.ScaleTo(1.4f).Then().ScaleTo(1, 200, Easing.Out);
            }
        }

        internal class LegacyOldStyleFill : CompositeDrawable
        {
            public LegacyOldStyleFill(Skin skin)
            {
                // required for sizing correctly..
                var firstFrame = getTexture(skin, "colour-0");

                if (firstFrame == null)
                {
                    InternalChild = new Sprite { Texture = getTexture(skin, "colour") };
                    Size = InternalChild.Size;
                }
                else
                {
                    InternalChild = skin.GetAnimation("scorebar-colour", true, true, startAtCurrentTime: false, applyConfigFrameRate: true) ?? Drawable.Empty();
                    Size = new Vector2(firstFrame.DisplayWidth, firstFrame.DisplayHeight);
                }

                Position = new Vector2(3, 10) * 1.6f;
                Masking = true;
            }
        }

        internal class LegacyNewStyleFill : Sprite
        {
            public LegacyNewStyleFill(Skin skin)
            {
                Texture = getTexture(skin, "colour");
                Position = new Vector2(7.5f, 7.8f) * 1.6f;
            }
        }
    }
}
