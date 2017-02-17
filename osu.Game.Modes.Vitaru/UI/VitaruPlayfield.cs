//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Vitaru.Objects;
using osu.Game.Modes.Vitaru.Objects.Drawables;
using osu.Game.Modes.UI;
using OpenTK;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;
using System;

namespace osu.Game.Modes.Vitaru.UI
{
    public class VitaruPlayfield
    {/*
        private Container PlayArea;
        private Container EnemyArea;
        private Container JudgementLayer;

        public override Vector2 Size
        {
            get
            {
                var parentSize = Parent.DrawSize;
                var aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 4f / 3f, parentSize.Y);

                return new Vector2(aspectSize.X / parentSize.X, aspectSize.Y / parentSize.Y) * base.Size;
            }
        }

        public VitaruPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.75f);

            Add(new Drawable[]
            {
                PlayArea = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                EnemyArea = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                JudgementLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1
                }
            });
        }

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;
            DrawableEnemy c = h as DrawableEnemy;


            h.OnJudgement += judgement;

            base.Add(h);
        }

        private void judgement(DrawableHitObject arg1, JudgementInfo arg2)
        {
            throw new NotImplementedException();
        }*/

        /*private void judgement(DrawableHitObject h, JudgementInfo j)
        {
            DeathSprite explosion = new DeathSprite((VitaruJudgementInfo)j, (VitaruHitObject)h.HitObject);
        }*/
    }
}