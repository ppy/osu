// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Connections;
using osu.Game.Rulesets.UI;
using System.Linq;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        private readonly Container approachCircles;
        private readonly Container judgementLayer;
        private readonly ConnectionRenderer<OsuHitObject> connectionLayer;

        // Todo: This should not be a thing, but is currently required for the editor
        // https://github.com/ppy/osu-framework/issues/1283
        protected virtual bool ProxyApproachCircles => true;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        public override Vector2 Size
        {
            get
            {
                if (Parent == null)
                    return Vector2.Zero;

                var parentSize = Parent.DrawSize;
                var aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 4f / 3f, parentSize.Y);

                return new Vector2(aspectSize.X / parentSize.X, aspectSize.Y / parentSize.Y) * base.Size;
            }
        }

        public OsuPlayfield() : base(BASE_SIZE.X)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddRange(new Drawable[]
            {
                connectionLayer = new FollowPointRenderer
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 2,
                },
                judgementLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                approachCircles = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1,
                },
            });
        }

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;

            h.OnJudgement += onJudgement;

            var c = h as IDrawableHitObjectWithProxiedApproach;
            if (c != null && ProxyApproachCircles)
                approachCircles.Add(c.ProxiedLayer.CreateProxy());

            base.Add(h);
        }

        public override void PostProcess()
        {
            connectionLayer.HitObjects = HitObjects.Objects
                .Select(d => d.HitObject)
                .OrderBy(h => h.StartTime).OfType<OsuHitObject>();
        }

        private void onJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            var osuJudgement = (OsuJudgement)judgement;
            var osuObject = (OsuHitObject)judgedObject.HitObject;

            if (!judgedObject.DisplayJudgement)
                return;

            DrawableOsuJudgement explosion = new DrawableOsuJudgement(osuJudgement)
            {
                Origin = Anchor.Centre,
                Position = osuObject.StackedEndPosition + osuJudgement.PositionOffset
            };

            judgementLayer.Add(explosion);
        }
    }
}
