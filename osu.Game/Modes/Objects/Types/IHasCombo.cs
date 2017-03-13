using OpenTK.Graphics;

namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that is part of a combo.
    /// </summary>
    public interface IHasCombo
    {
        /// <summary>
        /// The colour of this HitObject in the combo.
        /// </summary>
        Color4 ComboColour { get; set; }

        /// <summary>
        /// Whether the HitObject starts a new combo.
        /// </summary>
        bool NewCombo { get; }

        /// <summary>
        /// The combo index.
        /// </summary>
        int ComboIndex { get; }
    }
}
