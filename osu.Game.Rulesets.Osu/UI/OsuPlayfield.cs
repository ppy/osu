// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Objects;
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
                followPoints.RemoveFollowPoints((DrawableOsuHitObject)h);

            return result;
        }

        private bool checkHittable(DrawableOsuHitObject osuHitObject)
        {
            DrawableHitObject lastObject = osuHitObject;

            // Get the last hitobject that can block future hits
            while ((lastObject = HitObjectContainer.AliveObjects.GetPrevious(lastObject)) != null)
            {
                if (canBlockFutureHits(lastObject.HitObject))
                    break;
            }

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
            // Hitobjects that block future hits should miss previous hitobjects if they're hit out-of-order.
            if (canBlockFutureHits(result.HitObject))
                missAllEarlierObjects(result.HitObject);

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
        /// <param name="hitObject">The marker <see cref="HitObject"/>, which all <see cref="HitObject"/>s earlier than will get missed.</param>
        private void missAllEarlierObjects(HitObject hitObject)
        {
            double minimumTime = hitObject.StartTime;

            foreach (var obj in HitObjectContainer.AliveObjects)
            {
                if (obj.HitObject.StartTime >= minimumTime)
                    break;

                switch (obj)
                {
                    case DrawableHitCircle circle:
                        miss(circle);
                        break;

                    case DrawableSlider slider:
                        miss(slider.HeadCircle);
                        break;
                }
            }

            static void miss(DrawableOsuHitObject obj)
            {
                // Hitobjects that have already been judged cannot be missed.
                if (obj.Judged)
                    return;

                obj.MissForcefully();
            }
        }

        /// <summary>
        /// Whether a <see cref="HitObject"/> can block hits on future <see cref="HitObject"/>s until its start time is reached.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to test.</param>
        /// <returns>Whether <paramref name="hitObject"/> can block hits on future <see cref="HitObject"/>s.</returns>
        private bool canBlockFutureHits(HitObject hitObject)
            => hitObject is HitCircle || hitObject is Slider;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => HitObjectContainer.ReceivePositionalInputAt(screenSpacePos);

        private class ApproachCircleProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable approachCircleProxy) => AddInternal(approachCircleProxy);
        }
    }
}
