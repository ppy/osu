// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
