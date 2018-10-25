using System.Collections.Generic;
using OpenTK.Graphics;

namespace Symcol.osu.Mods.Caster.CasterScreens.TeamsPieces
{
    /// <summary>
    /// Takes a team line from a file and converts it to usable info for the caster
    /// </summary>
    public class TeamSet
    {
        public string Name;

        public List<string> Players;

        public Color4 Colour;

        public TeamSet(string team)
        {

        }
    }
}
