// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
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

            Expire();
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
