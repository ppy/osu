using osu.Game.Skinning;

namespace osu.Game.Database
{
    public class StableSkinImporter : StableImporter<SkinInfo>
    {
        protected override string ImportFromStablePath => "Skins";

        public StableSkinImporter(IModelImporter<SkinInfo> importer)
            : base(importer)
        {
        }
    }
}