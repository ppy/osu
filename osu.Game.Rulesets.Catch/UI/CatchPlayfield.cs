// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : ScrollingPlayfield<CatchBaseHit, CatchJudgement>
    {
        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;
        private readonly CatcherArea catcherArea;

        public CatchPlayfield()
            : base(Axes.Y)
        {
            Reversed.Value = true;

            Size = new Vector2(1);

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            InternalChildren = new Drawable[]
            {
                content = new Container<Drawable>
                {
                    RelativeSizeAxes = Axes.Both,
                },
                catcherArea = new CatcherArea
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    Height = 0.3f
                }
            };
        }

        public override void Add(DrawableHitObject<CatchBaseHit, CatchJudgement> h)
        {
            base.Add(h);

            var fruit = (DrawableFruit)h;
            fruit.CheckPosition = catcherArea.CheckIfWeCanCatch;
            fruit.OnJudgement += Fruit_OnJudgement;
        }

        private void Fruit_OnJudgement(DrawableHitObject<CatchBaseHit, CatchJudgement> obj)
        {
            if (obj.Judgement.Result == HitResult.Hit)
            {
                Vector2 screenPosition = obj.ScreenSpaceDrawQuad.Centre;
                Remove(obj);
                catcherArea.Add(obj, screenPosition);
            }
        }
    }
}
