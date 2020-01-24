// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A circle explodes from the hit target to indicate a hitobject has been hit.
    /// </summary>
    internal class HitExplosion : CircularContainer
    {
        public override bool RemoveWhenNotAlive => true;

        public readonly DrawableHitObject JudgedObject;

        private readonly Box innerFill;

        private readonly bool isRim;

        public HitExplosion(DrawableHitObject judgedObject, bool isRim)
        {
            this.isRim = isRim;

            JudgedObject = judgedObject;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(TaikoHitObject.DEFAULT_SIZE);

            RelativePositionAxes = Axes.Both;

            BorderColour = Color4.White;
            BorderThickness = 1;

            Alpha = 0.15f;
            Masking = true;

            Children = new[]
            {
                innerFill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            innerFill.Colour = isRim ? colours.BlueDarker : colours.PinkDarker;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(3f, 1000, Easing.OutQuint);
            this.FadeOut(500);

            Expire(true);
        }

        /// <summary>
        /// Transforms this hit explosion to visualise a secondary hit.
        /// </summary>
        public void VisualiseSecondHit()
        {
            this.ResizeTo(new Vector2(TaikoHitObject.DEFAULT_STRONG_SIZE), 50);
        }
    }
}
