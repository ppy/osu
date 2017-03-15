﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Connections;
using osu.Game.Modes.UI;
using System.Linq;
using osu.Game.Graphics.Cursor;
using osu.Game.Modes.Osu.Judgements;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.UI
{
    public class OsuPlayfield : Playfield<OsuHitObject, OsuJudgementInfo>
    {
        private Container approachCircles;
        private Container judgementLayer;
        private ConnectionRenderer<OsuHitObject> connectionLayer;

        public override Vector2 Size
        {
            get
            {
                var parentSize = Parent.DrawSize;
                var aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 4f / 3f, parentSize.Y);

                return new Vector2(aspectSize.X / parentSize.X, aspectSize.Y / parentSize.Y) * base.Size;
            }
        }

        public OsuPlayfield() : base(512)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.75f);

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
            AddInternal(new OsuCursorContainer { Colour = Color4.LightYellow });
        }

        public override void Add(DrawableHitObject<OsuHitObject, OsuJudgementInfo> h)
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

        public override void OnJudgement(DrawableHitObject<OsuHitObject, OsuJudgementInfo> judgedObject)
        {
            HitExplosion explosion = new HitExplosion(judgedObject.Judgement, judgedObject.HitObject);

            judgementLayer.Add(explosion);
        }
    }
}