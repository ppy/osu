// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.IEnumerableExtensions;
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
            osuHitObject.CheckHittable = checkHittable;

            followPoints.AddFollowPoints(osuHitObject);
        }

        public override bool Remove(DrawableHitObject h)
        {
            bool result = base.Remove(h);

            if (result)
            {
                DrawableOsuHitObject osuHitObject = (DrawableOsuHitObject)h;
                osuHitObject.CheckHittable = null;

                followPoints.RemoveFollowPoints(osuHitObject);
            }

            return result;
        }

        private bool checkHittable(DrawableOsuHitObject osuHitObject)
        {
            var lastObject = HitObjectContainer.AliveObjects.GetPrevious(osuHitObject);

            // If there is no previous object alive, allow the hit.
            if (lastObject == null)
                return true;

            // Ensure that either the last object has received a judgement or the hit time occurs at or after the last object's start time.
            // Simultaneous hitobjects are allowed to be hit at the same time value to account for edge-cases such as Centipede.
            if (lastObject.Judged || Time.Current >= lastObject.HitObject.StartTime)
                return true;

            return false;
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            missAllEarlier(result);

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

        /// <summary>
        /// Misses all <see cref="OsuHitObject"/>s occurring earlier than the start time of a judged <see cref="OsuHitObject"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> of the judged <see cref="OsuHitObject"/>.</param>
        private void missAllEarlier(JudgementResult result)
        {
            // Hitobjects that count as bonus should not cause other hitobjects to get missed.
            // E.g. For the sequence slider-head -> circle -> slider-tick, hitting the tick before the circle should not cause the circle to be missed.
            // E.g. For the sequence spinner -> circle -> spinner-bonus, hitting the bonus before the circle should not cause the circle to be missed.
            if (result.Judgement.IsBonus)
                return;

            // The minimum start time required for hitobjects so that they aren't missed.
            double minimumTime = result.HitObject.StartTime;

            foreach (var obj in HitObjectContainer.AliveObjects)
            {
                if (obj.HitObject.StartTime >= minimumTime)
                    break;

                attemptMiss(obj);

                foreach (var n in obj.NestedHitObjects)
                {
                    if (n.HitObject.StartTime >= minimumTime)
                        break;

                    attemptMiss(n);
                }
            }

            static void attemptMiss(DrawableHitObject obj)
            {
                if (!(obj is DrawableOsuHitObject osuObject))
                    throw new InvalidOperationException($"{obj.GetType()} is not a {nameof(DrawableOsuHitObject)}.");

                // Hitobjects that have already been judged cannot be missed.
                if (osuObject.Judged)
                    return;

                // Hitobjects that count as bonus should not be missed.
                // For the sequence slider-head -> slider-tick -> circle, hitting the circle before the tick should not cause the tick to be missed.
                if (osuObject.Result.Judgement.IsBonus)
                    return;

                osuObject.MissForcefully();
            }
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => HitObjectContainer.ReceivePositionalInputAt(screenSpacePos);

        private class ApproachCircleProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable approachCircleProxy) => AddInternal(approachCircleProxy);
        }
    }
}
