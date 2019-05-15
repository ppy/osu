// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    public class TaikoPiece : BeatSyncedContainer, IHasAccentColour
    {
        private Color4 accentColour;

        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public virtual Color4 AccentColour
        {
            get => accentColour;
            set => accentColour = value;
        }

        private bool kiaiMode;

        /// <summary>
        /// Whether Kiai mode effects are enabled for this circle piece.
        /// </summary>
        public virtual bool KiaiMode
        {
            get => kiaiMode;
            set => kiaiMode = value;
        }

        public TaikoPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }
}
