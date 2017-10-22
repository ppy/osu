// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;
using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : ScrollingPlayfield
    {
        public static readonly float BASE_WIDTH = 512;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        private readonly Container catcherContainer;
        private readonly Catcher catcher;

        public CatchPlayfield()
            : base(Axes.Y)
        {
            Container explodingFruitContainer;

            Reversed.Value = true;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            InternalChildren = new Drawable[]
            {
                content = new Container<Drawable>
                {
                    RelativeSizeAxes = Axes.Both,
                },
                explodingFruitContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                catcherContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    Height = 180,
                    Child = catcher = new Catcher
                    {
                        ExplodingFruitTarget = explodingFruitContainer,
                        RelativePositionAxes = Axes.Both,
                        Origin = Anchor.TopCentre,
                        X = 0.5f,
                    }
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            catcher.Size = new Vector2(catcherContainer.DrawSize.Y);
        }

        public bool CheckIfWeCanCatch(CatchBaseHit obj) => Math.Abs(catcher.Position.X - obj.X) < catcher.DrawSize.X / DrawSize.X / 2;

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;

            base.Add(h);

            var fruit = (DrawableCatchHitObject)h;
            fruit.CheckPosition = CheckIfWeCanCatch;
        }

        public override void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            if (judgement.IsHit)
            {
                Vector2 screenPosition = judgedObject.ScreenSpaceDrawQuad.Centre;

                // todo: don't do this
                (judgedObject.Parent as Container<DrawableHitObject>)?.Remove(judgedObject);
                (judgedObject.Parent as Container)?.Remove(judgedObject);

                catcher.Add(judgedObject, screenPosition);
            }
        }
    }
}
