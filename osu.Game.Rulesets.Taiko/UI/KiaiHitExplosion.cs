// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class KiaiHitExplosion : CircularContainer
    {
        public override bool RemoveWhenNotAlive => true;

        public readonly DrawableHitObject JudgedObject;

        private readonly bool isRim;

        public KiaiHitExplosion(DrawableHitObject judgedObject, bool isRim)
        {
            this.isRim = isRim;

            JudgedObject = judgedObject;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(TaikoHitObject.DEFAULT_SIZE, 1);

            Masking = true;
            Alpha = 0.25f;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = isRim ? colours.BlueDarker : colours.PinkDarker,
                Radius = 60,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(new Vector2(1, 3f), 500, Easing.OutQuint);
            this.FadeOut(250);

            Expire(true);
        }
    }
}
