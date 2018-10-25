using System.Collections.Generic;
using osu.Core.OsuMods;
using osu.Core.Wiki.Included.Lazer;
using osu.Core.Wiki.Included.Symcol;

namespace osu.Core.Wiki
{
    public static class WikiSetStore
    {
        public static List<WikiSet> LoadedWikiSets = new List<WikiSet>();

        public static void ReloadWikiSets()
        {
            //We want to add a default one for "Home"
            LoadedWikiSets = new List<WikiSet>
            {
                new LazerWikiSet(),
                new SymcolWikiSet()
            };


            foreach (OsuModSet set in OsuModStore.LoadedModSets)
                if (set.GetWikiSet() != null)
                    LoadedWikiSets.Add(set.GetWikiSet());
        }
    }
}
