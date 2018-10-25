using osu.Core.Containers.Shawdooow;
using osu.Core.Wiki;
using osu.Game;
using osu.Game.Screens;

namespace osu.Core.OsuMods
{
    public abstract class OsuModSet
    {
        public virtual SymcolButton GetMenuButton() => null;

        public virtual OsuScreen GetScreen() => null;

        public virtual WikiSet GetWikiSet() => null;

        public virtual void LoadComplete(OsuGame game) { }
    }
}
