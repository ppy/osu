// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.IO.Archives;
using Realms;

namespace osu.Game.Skinning
{
    public class SkinImporter : RealmArchiveModelImporter<SkinInfo>
    {
        private const string skin_info_file = "skininfo.json";

        private readonly IStorageResourceProvider skinResources;

        private readonly ModelManager<SkinInfo> modelManager;

        public SkinImporter(Storage storage, RealmAccess realm, IStorageResourceProvider skinResources)
            : base(storage, realm)
        {
            this.skinResources = skinResources;

            modelManager = new ModelManager<SkinInfo>(storage, realm);
        }

        public override IEnumerable<string> HandledExtensions => new[] { ".osk" };

        protected override string[] HashableFileTypes => new[] { ".ini", ".json" };

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path).ToLowerInvariant() == @".osk";

        protected override SkinInfo CreateModel(ArchiveReader archive, ImportParameters parameters) => new SkinInfo { Name = archive.Name ?? @"No name" };

        private const string unknown_creator_string = @"Unknown";

        protected override void Populate(SkinInfo model, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default)
        {
            var skinInfoFile = model.GetFile(skin_info_file);

            if (skinInfoFile != null)
            {
                try
                {
                    using (var existingStream = Files.Storage.GetStream(skinInfoFile.File.GetStoragePath()))
                    using (var reader = new StreamReader(existingStream))
                    {
                        var deserialisedSkinInfo = JsonConvert.DeserializeObject<SkinInfo>(reader.ReadToEnd());

                        if (deserialisedSkinInfo != null)
                        {
                            // for now we only care about the instantiation info.
                            // eventually we probably want to transfer everything across.
                            model.InstantiationInfo = deserialisedSkinInfo.InstantiationInfo;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogForModel(model, $"Error during {skin_info_file} parsing, falling back to default", e);

                    // Not sure if we should still run the import in the case of failure here, but let's do so for now.
                    model.InstantiationInfo = string.Empty;
                }
            }

            // Always rewrite instantiation info (even after parsing in from the skin json) for sanity.
            model.InstantiationInfo = createInstance(model).GetType().GetInvariantInstantiationInfo();

            checkSkinIniMetadata(model, realm);
        }

        private void checkSkinIniMetadata(SkinInfo item, Realm realm)
        {
            var instance = createInstance(item);

            // This function can be run on fresh import or save. The logic here ensures a skin.ini file is in a good state for both operations.
            // `Skin` will parse the skin.ini and populate `Skin.Configuration` during construction above.
            string skinIniSourcedName = instance.Configuration.SkinInfo.Name;
            string skinIniSourcedCreator = instance.Configuration.SkinInfo.Creator;
            string archiveName = item.Name.Replace(@".osk", string.Empty, StringComparison.OrdinalIgnoreCase);

            bool isImport = !item.IsManaged;

            if (isImport)
            {
                item.Name = !string.IsNullOrEmpty(skinIniSourcedName) ? skinIniSourcedName : archiveName;
                item.Creator = !string.IsNullOrEmpty(skinIniSourcedCreator) ? skinIniSourcedCreator : unknown_creator_string;

                // For imports, we want to use the archive or folder name as part of the metadata, in addition to any existing skin.ini metadata.
                // In an ideal world, skin.ini would be the only source of metadata, but a lot of skin creators and users don't update it when making modifications.
                // In both of these cases, the expectation from the user is that the filename or folder name is displayed somewhere to identify the skin.
                if (archiveName != item.Name
                    // lazer exports use this format
                    // GetValidFilename accounts for skins with non-ASCII characters in the name that have been exported by lazer.
                    && archiveName != item.GetDisplayString().GetValidFilename())
                    item.Name = @$"{item.Name} [{archiveName}]";
            }

            // By this point, the metadata in SkinInfo will be correct.
            // Regardless of whether this is an import or not, let's write the skin.ini if non-existing or non-matching.
            // This is (weirdly) done inside ComputeHash to avoid adding a new method to handle this case. After switching to realm it can be moved into another place.
            if (skinIniSourcedName != item.Name)
                updateSkinIniMetadata(item, realm);
        }

        private void updateSkinIniMetadata(SkinInfo item, Realm realm)
        {
            string nameLine = @$"Name: {item.Name}";
            string authorLine = @$"Author: {item.Creator}";

            List<string> newLines = new List<string>
            {
                @"// The following content was automatically added by osu! during import, based on filename / folder metadata.",
                @"[General]",
                nameLine,
                authorLine,
            };

            var existingFile = item.GetFile(@"skin.ini");

            if (existingFile == null)
            {
                // skins without a skin.ini are supposed to import using the "latest version" spec.
                // see https://github.com/peppy/osu-stable-reference/blob/1531237b63392e82c003c712faa028406073aa8f/osu!/Graphics/Skinning/SkinManager.cs#L297-L298
                newLines.Add($"Version: {SkinConfiguration.LATEST_VERSION}");

                // In the case a skin doesn't have a skin.ini yet, let's create one.
                writeNewSkinIni();
            }
            else
            {
                using (Stream stream = new MemoryStream())
                {
                    using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    {
                        using (var existingStream = Files.Storage.GetStream(existingFile.File.GetStoragePath()))
                        using (var sr = new StreamReader(existingStream))
                        {
                            string? line;
                            while ((line = sr.ReadLine()) != null)
                                sw.WriteLine(line);
                        }

                        sw.WriteLine();

                        foreach (string line in newLines)
                            sw.WriteLine(line);
                    }

                    modelManager.ReplaceFile(existingFile, stream, realm);
                }
            }

            // The hash is already populated at this point in import.
            // As we have changed files, it needs to be recomputed.
            item.Hash = ComputeHash(item);

            void writeNewSkinIni()
            {
                using (Stream stream = new MemoryStream())
                {
                    using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    {
                        foreach (string line in newLines)
                            sw.WriteLine(line);
                    }

                    modelManager.AddFile(item, stream, @"skin.ini", realm);
                }

                item.Hash = ComputeHash(item);
            }
        }

        private Skin createInstance(SkinInfo item) => item.CreateInstance(skinResources);

        /// <summary>
        /// Save a skin, serialising any changes to skin layouts to relevant JSON structures.
        /// </summary>
        /// <returns>Whether any change actually occurred.</returns>
        public bool Save(Skin skin)
        {
            bool hadChanges = false;

            skin.SkinInfo.PerformWrite(s =>
            {
                // Update for safety
                s.InstantiationInfo = skin.GetType().GetInvariantInstantiationInfo();

                // Serialise out the SkinInfo itself.
                string skinInfoJson = JsonConvert.SerializeObject(s, new JsonSerializerSettings { Formatting = Formatting.Indented });

                using (var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(skinInfoJson)))
                {
                    modelManager.AddFile(s, streamContent, skin_info_file, s.Realm!);
                }

                // Then serialise each of the drawable component groups into respective files.
                foreach (var drawableInfo in skin.LayoutInfos)
                {
                    string json = JsonConvert.SerializeObject(drawableInfo.Value, new JsonSerializerSettings { Formatting = Formatting.Indented });

                    using (var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        var oldFile = s.GetFile(drawableInfo.Key.Filename);

                        if (oldFile != null)
                            modelManager.ReplaceFile(oldFile, streamContent, s.Realm!);
                        else
                            modelManager.AddFile(s, streamContent, drawableInfo.Key.Filename, s.Realm!);
                    }
                }

                string newHash = ComputeHash(s);

                hadChanges = newHash != s.Hash;

                s.Hash = newHash;
            });

            return hadChanges;
        }
    }
}
