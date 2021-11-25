using osu.Game.Skinning;

namespace osu.Game.Database
{
    public class LegacySkinImporter : LegacyModelImporter<SkinInfo>
    {
        protected override string ImportFromStablePath => "Skins";

        public LegacySkinImporter(IModelImporter<SkinInfo> importer)
            : base(importer)
        {
        }
    }
}