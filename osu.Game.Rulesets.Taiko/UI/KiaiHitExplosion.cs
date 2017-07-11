// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class KiaiHitExplosion : CircularContainer
    {
        public readonly TaikoJudgement Judgement;

        private readonly bool isRim;

        public KiaiHitExplosion(TaikoJudgement judgement, bool isRim)
        {
            this.isRim = isRim;

            Judgement = judgement;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(TaikoHitObject.DEFAULT_CIRCLE_DIAMETER, 1);

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

            ScaleTo(new Vector2(1, 3f), 500, EasingTypes.OutQuint);
            FadeOut(250);

            Expire();
        }
    }
}