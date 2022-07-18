// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.UI
{
    internal class DefaultHitExplosion : CircularContainer, IAnimatableHitExplosion
    {
        private readonly HitResult result;

        [CanBeNull]
        private Box body;

        [Resolved]
        private OsuColour colours { get; set; }

        public DefaultHitExplosion(HitResult result)
        {
            this.result = result;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            BorderColour = Color4.White;
            BorderThickness = 1;

            Blending = BlendingParameters.Additive;

            Alpha = 0.15f;
            Masking = true;

            if (!result.IsHit())
                return;

            InternalChildren = new[]
            {
                body = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            updateColour();
        }

        private void updateColour([CanBeNull] DrawableHitObject judgedObject = null)
        {
            if (body == null)
                return;

            bool isRim = (judgedObject?.HitObject as Hit)?.Type == HitType.Rim;
            body.Colour = isRim ? colours.BlueDarker : colours.PinkDarker;
        }

        public void Animate(DrawableHitObject drawableHitObject)
        {
            updateColour(drawableHitObject);

            this.ScaleTo(3f, 1000, Easing.OutQuint);
            this.FadeOut(500);
        }

        public void AnimateSecondHit()
        {
        }
    }
}
