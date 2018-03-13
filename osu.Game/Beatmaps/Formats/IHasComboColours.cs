using System.Collections.Generic;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public interface IHasComboColours
    {
        List<Color4> ComboColours { get; set; }
    }
}
