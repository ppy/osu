using osu.Game.Rulesets.Vitaru.Wiki.Sections;
using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Vitaru.Wiki
{
    public class VitaruWikiOverlay : WikiOverlay
    {
        protected override WikiHeader Header => new VitaruWikiHeader();

        protected override WikiSection[] Sections => new WikiSection[]
            {
                new GameplaySection(),
                new EditorSection(),
                new RankingSection(),
                new MultiplayerSection(),
                new CodeSection(),
                new CreditsSection(),
            };
    }
}
