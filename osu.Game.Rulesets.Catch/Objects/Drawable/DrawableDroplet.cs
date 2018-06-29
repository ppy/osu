// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawable.Pieces;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableDroplet : PalpableCatchHitObject<Droplet>
    {
        private Pulp pulp;

        public override bool StaysOnPlate => false;

        public DrawableDroplet(Droplet h)
            : base(h)
        {
            Origin = Anchor.Centre;
            Size = new Vector2((float)CatchHitObject.OBJECT_RADIUS) / 4;
            Masking = false;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (CheckPosition == null) return;

            if (timeOffset >= 0)
                AddJudgement(new CatchDropletJudgement { Result = CheckPosition.Invoke(HitObject) ? HitResult.Perfect : HitResult.Miss });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = pulp = new Pulp
            {
                Size = Size
            };
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;
                pulp.AccentColour = AccentColour;
            }
        }
    }
}
