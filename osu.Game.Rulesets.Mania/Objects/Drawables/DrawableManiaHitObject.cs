// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public abstract class DrawableManiaHitObject<TObject> : DrawableHitObject<ManiaHitObject, ManiaJudgement>
        where TObject : ManiaHitObject
    {
        public new TObject HitObject;

        private readonly Container glowContainer;

        protected DrawableManiaHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;

            RelativePositionAxes = Axes.Y;
            Y = (float)HitObject.StartTime;

            Add(glowContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    }
                }
            });
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                if (base.AccentColour == value)
                    return;
                base.AccentColour = value;

                glowContainer.EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = value
                };
            }
        }

        protected override ManiaJudgement CreateJudgement() => new ManiaJudgement();
    }
}
