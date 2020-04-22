// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Connections;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        private readonly ApproachCircleProxyContainer approachCircles;
        private readonly JudgementContainer<DrawableOsuJudgement> judgementLayer;
        private readonly FollowPointRenderer followPoints;
        private readonly OrderedHitPolicy hitPolicy;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        protected override GameplayCursorContainer CreateCursor() => new OsuCursorContainer();

        public OsuPlayfield()
        {
            InternalChildren = new Drawable[]
            {
                followPoints = new FollowPointRenderer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 2,
                },
                judgementLayer = new JudgementContainer<DrawableOsuJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                // Todo: This should not exist, but currently helps to reduce LOH allocations due to unbinding skin source events on judgement disposal
                // Todo: Remove when hitobjects are properly pooled
                new SkinProvidingContainer(null)
                {
                    Child = HitObjectContainer,
                },
                approachCircles = new ApproachCircleProxyContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1,
                },
            };

            hitPolicy = new OrderedHitPolicy(HitObjectContainer);
        }

        public override void Add(DrawableHitObject h)
        {
            h.OnNewResult += onNewResult;
            h.OnLoadComplete += d =>
            {
                if (d is IDrawableHitObjectWithProxiedApproach c)
                    approachCircles.Add(c.ProxiedLayer.CreateProxy());
            };

            base.Add(h);

            DrawableOsuHitObject osuHitObject = (DrawableOsuHitObject)h;
            osuHitObject.CheckHittable = hitPolicy.IsHittable;

            followPoints.AddFollowPoints(osuHitObject);
        }

        public override bool Remove(DrawableHitObject h)
        {
            bool result = base.Remove(h);

            if (result)
                followPoints.RemoveFollowPoints((DrawableOsuHitObject)h);

            return result;
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            // Hitobjects that block future hits should miss previous hitobjects if they're hit out-of-order.
            hitPolicy.HandleHit(judgedObject);

            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            DrawableOsuJudgement explosion = new DrawableOsuJudgement(result, judgedObject)
            {
                Origin = Anchor.Centre,
                Position = ((OsuHitObject)judgedObject.HitObject).StackedEndPosition,
                Scale = new Vector2(((OsuHitObject)judgedObject.HitObject).Scale)
            };

            judgementLayer.Add(explosion);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => HitObjectContainer.ReceivePositionalInputAt(screenSpacePos);

        private class ApproachCircleProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable approachCircleProxy) => AddInternal(approachCircleProxy);
        }
    }
}
