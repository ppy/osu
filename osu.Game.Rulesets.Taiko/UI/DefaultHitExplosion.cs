// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    internal class DefaultHitExplosion : CircularContainer
    {
        [Resolved]
        private DrawableHitObject judgedObject { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.Both;

            BorderColour = Color4.White;
            BorderThickness = 1;

            Blending = BlendingParameters.Additive;

            Alpha = 0.15f;
            Masking = true;

            if (judgedObject.Result.Type == HitResult.Miss)
                return;

            bool isRim = (judgedObject.HitObject as Hit)?.Type == HitType.Rim;

            InternalChildren = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = isRim ? colours.BlueDarker : colours.PinkDarker,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(3f, 1000, Easing.OutQuint);
            this.FadeOut(500);

            Expire(true);
        }
    }
}
