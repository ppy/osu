// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public class DrawableNote : DrawableManiaHitObject<Note>
    {
        private readonly NotePiece headPiece;

        public DrawableNote(Note hitObject, Bindable<Key> key = null)
            : base(hitObject, key)
        {
            RelativeSizeAxes = Axes.Both;
            Height = 100;

            Add(headPiece = new NotePiece
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre
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
            }
        }

        protected override void Update()
        {
            if (Time.Current > HitObject.StartTime)
                Colour = Color4.Green;
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
