using osu.Framework.Graphics.Textures;
using Symcol.osu.Core.Wiki;
using Symcol.osu.Core.Wiki.Sections;

namespace Symcol.osu.Mods.Caster.Wiki
{
    public class CasterWikiSet : WikiSet
    {
        public override string Name => "caster";

        public override string Description => "The caster aims to make the casters' lives easier by making it much easier to archive and share important information about tournements.";

        public override string IndexTooltip => "the caster mod wiki!";

        public override Texture Icon => CasterModSet.CasterTextures.Get("Casters icon 1080");

        public override Texture HeaderBackground => CasterModSet.CasterTextures.Get("Casters");

        public override WikiSection[] GetSections() => new WikiSection[]
        {
            new General(),
            new Teams(),
            new Maps(),
            new Results(),
            new Credits(), 
        };
    }
}
