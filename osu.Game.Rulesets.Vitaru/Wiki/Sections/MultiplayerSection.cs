using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Vitaru.Wiki.Sections
{
    public class MultiplayerSection : WikiSection
    {
        public override string Title => "Multiplayer";

        public MultiplayerSection()
        {
            Content.Add(new WikiParagraph("Vitaru comes equiped with both online and offline multiplayer (with bots, split screen shall appear in the future)."));
            Content.Add(new WikiSubSectionHeader("Offline"));
            Content.Add(new WikiParagraph("Currently Offline is in very early stages of development and the ai doesn't know how to use all the spells. " +
                "I would like to fix this for release but no promises."));
            Content.Add(new WikiSubSectionHeader("Online"));
            Content.Add(new WikiParagraph("Vitaru has wicked buggy online multiplayer (which requires the osu.Game Symcol mods, see discord to get these). " +
                "The lobby doesnt work right, packet sharing is still buggy in game, and finishing a map breaks the connection to host. " +
                "There is plenty that needs work, don't expect it to work perfectly quite yet, though a quick mention in the vitaru dev channel if you find something would be nice. " +
                "And as a final \"this is buggy af\" disclaimer, there is no packet loss compensation in place, so missing packets may or may not break eveything. " +
                "Plans to deal with this effectivly are in the works, especially trying to keep them in order.\n\n" +
                "Currently both peers and host are required to portforward the port being used, and in addition peers must input their local ip on top of the host's ip where as the host only has to plug in their local ip. " +
                "Spells are also destined to break the game, especially the time shifters, however this will be fixed (soon?).\n\n" +
                "This is a pain in the ass, only the host should have to port forward and in the future I would like to resolve this."));
        }
    }
}
