using System.Collections.Generic;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public interface IHasCustomColours
    {
        Dictionary<string, Color4> CustomColours { get; set; }
    }
}
