using osu.Framework.Graphics;
using Symcol.Core.Graphics.Containers;
using Symcol.osu.Mods.Caster.Pieces;

namespace Symcol.osu.Mods.Caster.CasterScreens
{
    public class CasterSubScreen : SymcolContainer
    {
        protected readonly CasterControlPanel CasterControlPanel;

        public CasterSubScreen(CasterControlPanel controlPanel)
        {
            CasterControlPanel = controlPanel;

            RelativeSizeAxes = Axes.Both;
        }
    }
}
