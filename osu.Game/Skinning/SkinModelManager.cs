// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.IO.Archives;

namespace osu.Game.Skinning
{
    public class SkinModelManager : ArchiveModelManager<SkinInfo, SkinFileInfo>
    {
        private readonly IStorageResourceProvider skinResources;

        public SkinModelManager(Storage storage, DatabaseContextFactory contextFactory, SkinStore skinStore, GameHost host, IStorageResourceProvider skinResources)
            : base(storage, contextFactory, skinStore, host)
        {
            this.skinResources = skinResources;

            // can be removed 20220420.
            populateMissingHashes();
        }

        public override IEnumerable<string> HandledExtensions => new[] { ".osk" };

        protected override string[] HashableFileTypes => new[] { ".ini", ".json" };

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == @".osk";

        protected override SkinInfo CreateModel(ArchiveReader archive) => new SkinInfo { Name = archive.Name ?? @"No name" };

        private const string unknown_creator_string = @"Unknown";

        protected override bool HasCustomHashFunction => true;

        protected override string ComputeHash(SkinInfo item)
        {
            var instance = createInstance(item);

            // This function can be run on fresh import or save. The logic here ensures a skin.ini file is in a good state for both operations.

            // `Skin` will parse the skin.ini and populate `Skin.Configuration` during construction above.
            string skinIniSourcedName = instance.Configuration.SkinInfo.Name;
            string skinIniSourcedCreator = instance.Configuration.SkinInfo.Creator;
            string archiveName = item.Name.Replace(@".osk", string.Empty, StringComparison.OrdinalIgnoreCase);

            bool isImport = item.ID == 0;

            if (isImport)
            {
                item.Name = !string.IsNullOrEmpty(skinIniSourcedName) ? skinIniSourcedName : archiveName;
                item.Creator = !string.IsNullOrEmpty(skinIniSourcedCreator) ? skinIniSourcedCreator : unknown_creator_string;

                // For imports, we want to use the archive or folder name as part of the metadata, in addition to any existing skin.ini metadata.
                // In an ideal world, skin.ini would be the only source of metadata, but a lot of skin creators and users don't update it when making modifications.
                // In both of these cases, the expectation from the user is that the filename or folder name is displayed somewhere to identify the skin.
                if (archiveName != item.Name)
                    item.Name = @$"{item.Name} [{archiveName}]";
            }

            // By this point, the metadata in SkinInfo will be correct.
            // Regardless of whether this is an import or not, let's write the skin.ini if non-existing or non-matching.
            // This is (weirdly) done inside ComputeHash to avoid adding a new method to handle this case. After switching to realm it can be moved into another place.
            if (skinIniSourcedName != item.Name)
                updateSkinIniMetadata(item);

            return base.ComputeHash(item);
        }

        private void updateSkinIniMetadata(SkinInfo item)
        {
            string nameLine = @$"Name: {item.Name}";
            string authorLine = @$"Author: {item.Creator}";

            string[] newLines =
            {
                @"// The following content was automatically added by osu! during import, based on filename / folder metadata.",
                @"[General]",
                nameLine,
                authorLine,
            };

            var existingFile = item.Files.SingleOrDefault(f => f.Filename.Equals(@"skin.ini", StringComparison.OrdinalIgnoreCase));

            if (existingFile == null)
            {
                // In the case a skin doesn't have a skin.ini yet, let's create one.
                writeNewSkinIni();
                return;
            }

            using (Stream stream = new MemoryStream())
            {
                using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    using (var existingStream = Files.Storage.GetStream(existingFile.FileInfo.GetStoragePath()))
                    using (var sr = new StreamReader(existingStream))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                            sw.WriteLine(line);
                    }

                    sw.WriteLine();

                    foreach (string line in newLines)
                        sw.WriteLine(line);
                }

                ReplaceFile(item, existingFile, stream);

                // can be removed 20220502.
                if (!ensureIniWasUpdated(item))
                {
                    Logger.Log($"Skin {item}'s skin.ini had issues and has been removed. Please report this and provide the problematic skin.", LoggingTarget.Database, LogLevel.Important);

                    DeleteFile(item, item.Files.SingleOrDefault(f => f.Filename.Equals(@"skin.ini", StringComparison.OrdinalIgnoreCase)));
                    writeNewSkinIni();
                }
            }

            void writeNewSkinIni()
            {
                using (Stream stream = new MemoryStream())
                {
                    using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    {
                        foreach (string line in newLines)
                            sw.WriteLine(line);
                    }

                    AddFile(item, stream, @"skin.ini");
                }
            }
        }

        private bool ensureIniWasUpdated(SkinInfo item)
        {
            // This is a final consistency check to ensure that hash computation doesn't enter an infinite loop.
            // With other changes to the surrounding code this should never be hit, but until we are 101% sure that there
            // are no other cases let's avoid a hard startup crash by bailing and alerting.

            var instance = createInstance(item);

            return instance.Configuration.SkinInfo.Name == item.Name;
        }

        protected override Task Populate(SkinInfo model, ArchiveReader archive, CancellationToken cancellationToken = default)
        {
            var instance = createInstance(model);

            model.InstantiationInfo ??= instance.GetType().GetInvariantInstantiationInfo();

            model.Name = instance.Configuration.SkinInfo.Name;
            model.Creator = instance.Configuration.SkinInfo.Creator;

            return Task.CompletedTask;
        }

        private void populateMissingHashes()
        {
            var skinsWithoutHashes = ModelStore.ConsumableItems.Where(i => i.Hash == null).ToArray();

            foreach (SkinInfo skin in skinsWithoutHashes)
            {
                try
                {
                    Update(skin);
                }
                catch (Exception e)
                {
                    Delete(skin);
                    Logger.Error(e, $"Existing skin {skin} has been deleted during hash recomputation due to being invalid");
                }
            }
        }

        private Skin createInstance(SkinInfo item) => item.CreateInstance(skinResources);
    }
}
