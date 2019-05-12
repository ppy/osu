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
using System.Linq;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.UI.Cursor;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        private readonly ApproachCircleProxyContainer approachCircles;
        private readonly JudgementContainer<DrawableOsuJudgement> judgementLayer;
        private readonly ConnectionRenderer<OsuHitObject> connectionLayer;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        protected override GameplayCursorContainer CreateCursor() => new OsuCursorContainer();

        public OsuPlayfield()
        {
            InternalChildren = new Drawable[]
            {
                connectionLayer = new FollowPointRenderer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 2,
                },
                judgementLayer = new JudgementContainer<DrawableOsuJudgement>
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                HitObjectContainer,
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

            if (h is IDrawableHitObjectWithProxiedApproach c)
            {
                var original = c.ProxiedLayer;

                // Hitobjects only have lifetimes set on LoadComplete. For nested hitobjects (e.g. SliderHeads), this only happens when the parenting slider becomes visible.
                // This delegation is required to make sure that the approach circles for those not-yet-loaded objects aren't added prematurely.
                original.OnLoadComplete += addApproachCircleProxy;
            }

            base.Add(h);
        }

        private void addApproachCircleProxy(Drawable d)
        {
            var proxy = d.CreateProxy();
            proxy.LifetimeStart = d.LifetimeStart;
            proxy.LifetimeEnd = d.LifetimeEnd;
            approachCircles.Add(proxy);
        }

        public override void PostProcess()
        {
            connectionLayer.HitObjects = HitObjectContainer.Objects.Select(d => d.HitObject).OfType<OsuHitObject>();
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value)
                return;

            DrawableOsuJudgement explosion = new DrawableOsuJudgement(result, judgedObject)
            {
                Origin = Anchor.Centre,
                Position = ((OsuHitObject)judgedObject.HitObject).StackedEndPosition,
                Scale = new Vector2(((OsuHitObject)judgedObject.HitObject).Scale * 1.65f)
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
