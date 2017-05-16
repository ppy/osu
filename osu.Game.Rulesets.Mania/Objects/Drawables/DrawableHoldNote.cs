// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public class DrawableHoldNote : DrawableManiaHitObject<HoldNote>
    {
        private readonly NotePiece headPiece;
        private readonly BodyPiece bodyPiece;
        private readonly NotePiece tailPiece;

        public DrawableHoldNote(HoldNote hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.Both;
            Height = (float)HitObject.Duration;

            Add(new Drawable[]
            {
                // For now the body piece covers the entire height of the container
                // whereas possibly in the future we don't want to extend under the head/tail.
                // This will be fixed when new designs are given or the current design is finalized.
                bodyPiece = new BodyPiece
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                headPiece = new NotePiece
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                tailPiece = new NotePiece
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre
                }
            });
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
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
