using osu.Core.Wiki.Sections;

namespace osu.Core.Wiki.Included.Home
{
    public class WhatIsTheWiki : WikiSection
    {
        public override string Title => "What is this?";

        public override string Overview => "This is the un-official ingame wiki, " +
            "the wiki's purpose is to be just like any other wiki: house information about osu!lazer, rulesets and even mods loaded from the modloader. " +
            "You can get started by selecting a wiki from the index near the top left of this panel and return to this page at anytime by clicking \"Home\". ";
    }
}
