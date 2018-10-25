using Symcol.osu.Core.Wiki.Sections;
using Symcol.osu.Core.Wiki.Sections.SectionPieces;
using Symcol.osu.Core.Wiki.Sections.Subsection;

namespace Symcol.osu.Mods.Caster.Wiki
{
    public class General : WikiSection
    {
        public override string Title => "General";

        public override string Overview => "Upon first opening the Caster you may notice a panel to the right and a small bar at the top of the screen in adition to the two team panels near the center of the screen. "
                                           + "The left panel is called the caster control panel and is used to select which cup you want to view information on. "
                                           + "It also allows you to create new ones and switch to edit mode which I discussed later on in the \"Editables\" sub section down below. "
                                           + "";

        public override WikiSubSection[] GetSubSections() => new WikiSubSection[]
        {
            new Editables(), 
        };

        private class Editables : WikiSubSection
        {
            public override string Title => "Editables";

            public Editables()
            {
                Content.Add(new WikiParagraph("Editables are one of the main reasons the caster was re-writen from scratch, forcing people to go digging through the files to do anything was not acceptable at all. "
                                              + "So now we have editables, you can use them by toggling \"Edit mode\" from the top of the control panel. "
                                              + "This will make nearly everything on screen tweakable so you can add new players to a team, get rid of players or change what mods are on a specific map in a map pool."));
            }
        }
    }
}
