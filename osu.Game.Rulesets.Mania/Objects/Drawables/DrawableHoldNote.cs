// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;

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
            RelativeSizeAxes = Axes.Both;
            Height = (float)HitObject.Duration;

            Add(new Drawable[]
            {
                bodyPiece = new BodyPiece
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
                headPiece = new NotePiece
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre
                },
                tailPiece = new NotePiece
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                }
            });

            // The "length" of the hold note stops at the "base" of the tail piece
            // but we want to contain the tail piece within our bounds
            Height += (float)HitObject.Duration / headPiece.Height;
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

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
