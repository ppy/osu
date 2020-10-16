// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacyHealthDisplay : CompositeDrawable, IHealthDisplay
    {
        private readonly Skin skin;
        private Sprite fill;
        private Marker marker;

        private float maxFillWidth;

        public Bindable<double> Current { get; } = new BindableDouble { MinValue = 0, MaxValue = 1 };

        public LegacyHealthDisplay(Skin skin)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    Texture = skin.GetTexture("scorebar-bg")
                },
                fill = new Sprite
                {
                    Texture = skin.GetTexture("scorebar-colour"),
                    Position = new Vector2(7.5f, 7.8f) * 1.6f
                },
                marker = new Marker(skin)
                {
                    Current = { BindTarget = Current },
                }
            };

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

        public void Flash(JudgementResult result)
        {
            marker.ScaleTo(1.4f).Then().ScaleTo(1, 200, Easing.Out);
        }

        private class Marker : CompositeDrawable
        {
            public Bindable<double> Current { get; } = new Bindable<double>();

            public Marker(Skin skin)
            {
                Origin = Anchor.Centre;

                if (skin.GetTexture("scorebar-ki") != null)
                {
                    // TODO: old style (marker changes as health decreases)
                }
                else
                {
                    InternalChildren = new Drawable[]
                    {
                        new Sprite
                        {
                            Texture = skin.GetTexture("scorebar-marker"),
                            Origin = Anchor.Centre,
                        }
                    };
                }
            }
        }
    }
}
