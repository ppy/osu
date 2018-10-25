using osu.Core.Wiki.Included.Lazer.Sections.SubSections;
using osu.Core.Wiki.Sections;
using osu.Core.Wiki.Sections.Subsection;

namespace osu.Core.Wiki.Included.Lazer.Sections
{
    public class WhatsChanged : WikiSection
    {
        public override string Title => "Whats Changed?";

        public override string Overview => "A lot has changed between osu!lazer (here on refered to as just \"lazer\") and the old client (here on refered to as \"osu!stable\" or just \"stable\"). "
                                           + "First off, a lot of things are still being implemented or worked on. "
                                           + "A few examples are multiplayer, the editor, and some gamemodes (sometimes refered to as \"rulesets\") are still being worked on and are rather incomplete. "
                                           + "As such there is no need to report these things as \"broken\" or \"incomplete\". The devs know and are working everyday to implement them and fix bug.\n\n"
                                           + "With that out of the way, lets talk about changes to old features that new lazer players might not quite understand or know about and think there is an issue because of it.\n\n"
                                           + "1. You can no longer \"Hit F5 to refresh\" in song select, but this isn't a missing feature. The song select updates automatically and instantly when new maps are added or removed.";

        public override WikiSubSection[] GetSubSections() => new WikiSubSection[]
        {
            new Osu(),
            new Taiko(),
            new Catch(),
            new Mania(),
        };
    }
}
