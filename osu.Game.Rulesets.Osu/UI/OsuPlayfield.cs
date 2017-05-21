// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Graphics.Cursor;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfield : Playfield<OsuHitObject, OsuJudgement>
    {
        private readonly Container approachCircles;
        private readonly Container judgementLayer;
        private readonly ConnectionRenderer<OsuHitObject> connectionLayer;

        public override bool ProvidingUserCursor => true;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        public override Vector2 Size
        {
            get
            {
                var parentSize = Parent.DrawSize;
                var aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 4f / 3f, parentSize.Y);

                return new Vector2(aspectSize.X / parentSize.X, aspectSize.Y / parentSize.Y) * base.Size;
            }
        }

        public OsuPlayfield() : base(BASE_SIZE.X)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Add(new Drawable[]
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AddInternal(new GameplayCursor());
        }

        public override void Add(DrawableHitObject<OsuHitObject, OsuJudgement> h)
        {
            h.Depth = (float)h.HitObject.StartTime;

            IDrawableHitObjectWithProxiedApproach c = h as IDrawableHitObjectWithProxiedApproach;
            if (c != null)
                approachCircles.Add(c.ProxiedLayer.CreateProxy());

            base.Add(h);
        }

        public override void PostProcess()
        {
            connectionLayer.HitObjects = HitObjects.Children
                .Select(d => d.HitObject)
                .OrderBy(h => h.StartTime);
        }

        public override void OnJudgement(DrawableHitObject<OsuHitObject, OsuJudgement> judgedObject)
        {
            DrawableOsuJudgement explosion = new DrawableOsuJudgement(judgedObject.Judgement)
            {
                Origin = Anchor.Centre,
                Position = judgedObject.HitObject.StackedEndPosition + judgedObject.Judgement.PositionOffset
            };

            judgementLayer.Add(explosion);
        }
    }
}
