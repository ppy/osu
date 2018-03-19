using osu.Game.Rulesets.Shape.Wiki.Sections;
using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Shape.Wiki
{
    public class ShapeWikiOverlay : WikiOverlay
    {
        protected override WikiHeader Header => new ShapeWikiHeader();

        protected override WikiSection[] Sections => new WikiSection[]
            {
                new Gameplay()
            };
    }
}
