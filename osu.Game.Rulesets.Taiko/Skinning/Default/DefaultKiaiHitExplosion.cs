// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Taiko.Objects;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    public partial class DefaultKiaiHitExplosion : CircularContainer
    {
        public override bool RemoveWhenNotAlive => true;

        private readonly HitType type;

        public DefaultKiaiHitExplosion(HitType type)
        {
            this.type = type;

            RelativeSizeAxes = Axes.Both;

            Blending = BlendingParameters.Additive;

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
                Colour = type == HitType.Rim ? colours.BlueDarker : colours.PinkDarker,
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
