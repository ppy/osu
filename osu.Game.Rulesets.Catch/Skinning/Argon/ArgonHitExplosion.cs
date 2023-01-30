// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    public partial class ArgonHitExplosion : CompositeDrawable, IHitExplosion
    {
        public override bool RemoveWhenNotAlive => true;

        private Container tallExplosion = null!;
        private Container largeFaint = null!;

        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();

        public ArgonHitExplosion()
        {
            Size = new Vector2(20);
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                tallExplosion = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Width = 0.1f,
                    Child = new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                largeFaint = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                    },
                },
            };

            accentColour.BindValueChanged(colour =>
            {
                tallExplosion.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = colour.NewValue,
                    Hollow = false,
                    Roundness = 15,
                    Radius = 15,
                };

                largeFaint.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Interpolation.ValueAt(0.2f, colour.NewValue, Color4.White, 0, 1),
                    Hollow = false,
                    Radius = 50,
                };
            }, true);
        }

        public void Animate(HitExplosionEntry entry)
        {
            X = entry.Position;
            Scale = new Vector2(entry.HitObject.Scale);
            accentColour.Value = entry.ObjectColour;

            using (BeginAbsoluteSequence(entry.LifetimeStart))
            {
                this.FadeOutFromOne(400);

                if (!(entry.HitObject is Droplet))
                {
                    float scale = Math.Clamp(entry.JudgementResult.ComboAtJudgement / 200f, 0.35f, 1.125f);

                    tallExplosion
                        .ScaleTo(new Vector2(1.1f, 20 * scale), 200, Easing.OutQuint)
                        .Then()
                        .ScaleTo(new Vector2(1.1f, 1), 600, Easing.In);
                }
            }
        }
    }
}
