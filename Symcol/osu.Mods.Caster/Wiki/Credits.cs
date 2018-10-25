using Symcol.osu.Core.Wiki.Sections;
using Symcol.osu.Core.Wiki.Sections.SectionPieces;
using Symcol.osu.Core.Wiki.Sections.Subsection;

namespace Symcol.osu.Mods.Caster.Wiki
{
    public class Credits : WikiSection
    {
        public override string Title => "Credits";

        public Credits()
        {
            Content.Add(new WikiParagraph("Because no project is a one man show."));
            Content.Add(new WikiSubSectionLinkHeader("KoziLord", "https://osu.ppy.sh/users/6330292", "View profile in browser"));
            Content.Add(new WikiParagraph("Created templates for the art so it didn't look like garbage."));
        }
    }
}
