// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    public class TaikoPiece : Container, IHasAccentColour
    {
        private Color4 accentColour;
        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public virtual Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;
            }
        }

        private bool kiaiMode;
        /// <summary>
        /// Whether Kiai mode effects are enabled for this circle piece.
        /// </summary>
        public virtual bool KiaiMode
        {
            get { return kiaiMode; }
            set
            {
                kiaiMode = value;
            }
        }

        public TaikoPiece()
        {
            //just a default
            Size = new Vector2(TaikoHitObject.DEFAULT_CIRCLE_DIAMETER);
        }
    }
}
