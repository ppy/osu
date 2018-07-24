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

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        private readonly Container approachCircles;
        private readonly JudgementContainer<DrawableOsuJudgement> judgementLayer;
        private readonly ConnectionRenderer<OsuHitObject> connectionLayer;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        public OsuPlayfield()
            : base(BASE_SIZE.X)
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
                judgementLayer = new JudgementContainer<DrawableOsuJudgement>
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
            h.OnJudgement += onJudgement;

            var c = h as IDrawableHitObjectWithProxiedApproach;
            if (c != null)
                approachCircles.Add(c.ProxiedLayer.CreateProxy());

            base.Add(h);
        }

        public override void PostProcess()
        {
            connectionLayer.HitObjects = HitObjects.Objects.Select(d => d.HitObject).OfType<OsuHitObject>();
        }

        private void onJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            if (!judgedObject.DisplayJudgement || !DisplayJudgements)
                return;

            DrawableOsuJudgement explosion = new DrawableOsuJudgement(judgement, judgedObject)
            {
                Origin = Anchor.Centre,
                Position = ((OsuHitObject)judgedObject.HitObject).StackedEndPosition
            };

            judgementLayer.Add(explosion);
        }
    }
}
