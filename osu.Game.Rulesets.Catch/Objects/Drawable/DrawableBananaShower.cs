﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableBananaShower : DrawableCatchHitObject<BananaShower>
    {
        private readonly Container bananaContainer;

        public DrawableBananaShower(BananaShower s, Func<CatchHitObject, DrawableHitObject<CatchHitObject>> getVisualRepresentation = null)
            : base(s)
        {
            RelativeSizeAxes = Axes.X;
            Origin = Anchor.BottomLeft;
            X = 0;

            InternalChild = bananaContainer = new Container { RelativeSizeAxes = Axes.Both };

            foreach (var b in s.NestedHitObjects.Cast<BananaShower.Banana>())
                AddNested(getVisualRepresentation?.Invoke(b));
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
                AddJudgement(new Judgement { Result = NestedHitObjects.Cast<DrawableCatchHitObject>().Any(n => n.Judgements.Any(j => j.IsHit)) ? HitResult.Perfect : HitResult.Miss });
        }

        protected override void AddNested(DrawableHitObject h)
        {
            ((DrawableCatchHitObject)h).CheckPosition = o => CheckPosition?.Invoke(o) ?? false;
            bananaContainer.Add(h);
            base.AddNested(h);
        }
    }
}
