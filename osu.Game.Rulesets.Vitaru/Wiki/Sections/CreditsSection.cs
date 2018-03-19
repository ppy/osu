using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Vitaru.Wiki.Sections
{
    public class CreditsSection : WikiSection
    {
        public override string Title => "Credits";

        public CreditsSection()
        {
            Content.Add(new WikiParagraph("A place of thanks, because these people helped make vitaru in one way or another."));
            Content.Add(new WikiSubSectionLinkHeader("Jorolf", "https://osu.ppy.sh/users/7004641", "View profile in browser"));
            Content.Add(new WikiParagraph("Started the code base, without Jorolf vitaru would not exist today."));
            Content.Add(new WikiSubSectionLinkHeader("Arrcival", "https://osu.ppy.sh/users/3782165", "View profile in browser"));
            Content.Add(new WikiParagraph("Helped early on with design choices and pattern functions still used today!"));
            Content.Add(new WikiSubSectionLinkHeader("ColdVolcano", "https://osu.ppy.sh/users/7492333", "View profile in browser"));
            Content.Add(new WikiParagraph("Helped with random things early on, helped move things along."));
        }
    }
}
