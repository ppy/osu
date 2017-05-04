// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public class DrawableHoldNote : DrawableManiaHitObject<HoldNote>
    {
        /// <summary>
        /// Length of this hold note, relative to its parent.
        /// </summary>
        public float Length;

        private NotePiece headPiece;
        private BodyPiece bodyPiece;
        private NotePiece tailPiece;

        public DrawableHoldNote(HoldNote hitObject)
            : base(hitObject)
        {
            Children = new Drawable[]
            {
                headPiece = new NotePiece
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre
                },
                bodyPiece = new BodyPiece
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
                tailPiece = new NotePiece
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre
                }
            };
        }

        public override Color4 AccentColour
        {
            get { return AccentColour; }
            set
            {
                if (base.AccentColour == value)
                    return;
                base.AccentColour = value;

                headPiece.AccentColour = value;
                bodyPiece.AccentColour = value;
                tailPiece.AccentColour = value;
            }
        }


        protected override void Update()
        {
            bodyPiece.Height = Parent.DrawSize.Y * Length;
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
