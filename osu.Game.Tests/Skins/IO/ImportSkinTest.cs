// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Skinning;
using SharpCompress.Archives.Zip;

namespace osu.Game.Tests.Skins.IO
{
    public class ImportSkinTest : ImportTest
    {
        #region Testing filename metadata inclusion

        [Test]
        public Task TestSingleImportDifferentFilename() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin.osk"));

            // When the import filename doesn't match, it should be appended (and update the skin.ini).
            assertCorrectMetadata(import1, "test skin [skin]", "skinner", osu);
        });

        [Test]
        public Task TestSingleImportWeirdIniFileCase() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner", iniFilename: "Skin.InI"), "skin.osk"));

            // When the import filename doesn't match, it should be appended (and update the skin.ini).
            assertCorrectMetadata(import1, "test skin [skin]", "skinner", osu);
        });

        [Test]
        public Task TestSingleImportMissingSectionHeader() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner", includeSectionHeader: false), "skin.osk"));

            // When the import filename doesn't match, it should be appended (and update the skin.ini).
            assertCorrectMetadata(import1, "test skin [skin]", "skinner", osu);
        });

        [Test]
        public Task TestSingleImportMatchingFilename() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "test skin.osk"));

            // When the import filename matches it shouldn't be appended.
            assertCorrectMetadata(import1, "test skin", "skinner", osu);
        });

        [Test]
        public Task TestSingleImportNoIniFile() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithNonIniFile(), "test skin.osk"));

            // When the import filename matches it shouldn't be appended.
            assertCorrectMetadata(import1, "test skin", "Unknown", osu);
        });

        [Test]
        public Task TestEmptyImportImportsWithFilename() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createEmptyOsk(), "test skin.osk"));

            // When the import filename matches it shouldn't be appended.
            assertCorrectMetadata(import1, "test skin", "Unknown", osu);
        });

        #endregion

        #region Cases where imports should match existing

        [Test]
        public Task TestImportTwiceWithSameMetadataAndFilename() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin.osk"));
            var import2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin.osk"));

            assertImportedOnce(import1, import2);
        });

        [Test]
        public Task TestImportTwiceWithNoMetadataSameDownloadFilename() => runSkinTest(async osu =>
        {
            // if a user downloads two skins that do have skin.ini files but don't have any creator metadata in the skin.ini, they should both import separately just for safety.
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni(string.Empty, string.Empty), "download.osk"));
            var import2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni(string.Empty, string.Empty), "download.osk"));

            assertImportedOnce(import1, import2);
        });

        [Test]
        public Task TestImportUpperCasedOskArchive() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "name 1.OsK"));
            assertCorrectMetadata(import1, "name 1", "author 1", osu);

            var import2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "name 1.oSK"));

            assertImportedOnce(import1, import2);
        });

        [Test]
        public Task TestSameMetadataNameSameFolderName() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "my custom skin 1"));
            var import2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "my custom skin 1"));

            assertImportedOnce(import1, import2);
            assertCorrectMetadata(import1, "name 1 [my custom skin 1]", "author 1", osu);
        });

        #endregion

        #region Cases where imports should be uniquely imported

        [Test]
        public Task TestImportTwiceWithSameMetadataButDifferentFilename() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin.osk"));
            var import2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin2.osk"));

            assertImportedBoth(import1, import2);
        });

        [Test]
        public Task TestImportTwiceWithNoMetadataDifferentDownloadFilename() => runSkinTest(async osu =>
        {
            // if a user downloads two skins that do have skin.ini files but don't have any creator metadata in the skin.ini, they should both import separately just for safety.
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni(string.Empty, string.Empty), "download.osk"));
            var import2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni(string.Empty, string.Empty), "download2.osk"));

            assertImportedBoth(import1, import2);
        });

        [Test]
        public Task TestImportTwiceWithSameFilenameDifferentMetadata() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin v2", "skinner"), "skin.osk"));
            var import2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin v2.1", "skinner"), "skin.osk"));

            assertImportedBoth(import1, import2);
            assertCorrectMetadata(import1, "test skin v2 [skin]", "skinner", osu);
            assertCorrectMetadata(import2, "test skin v2.1 [skin]", "skinner", osu);
        });

        [Test]
        public Task TestSameMetadataNameDifferentFolderName() => runSkinTest(async osu =>
        {
            var import1 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "my custom skin 1"));
            var import2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "my custom skin 2"));

            assertImportedBoth(import1, import2);
            assertCorrectMetadata(import1, "name 1 [my custom skin 1]", "author 1", osu);
            assertCorrectMetadata(import2, "name 1 [my custom skin 2]", "author 1", osu);
        });

        [Test]
        public Task TestExportThenImportDefaultSkin() => runSkinTest(osu =>
        {
            var skinManager = osu.Dependencies.Get<SkinManager>();

            skinManager.EnsureMutableSkin();

            MemoryStream exportStream = new MemoryStream();

            Guid originalSkinId = skinManager.CurrentSkinInfo.Value.ID;

            skinManager.CurrentSkinInfo.Value.PerformRead(s =>
            {
                Assert.IsFalse(s.Protected);
                Assert.AreEqual(typeof(DefaultSkin), s.CreateInstance(skinManager).GetType());

                new LegacySkinExporter(osu.Dependencies.Get<Storage>()).ExportModelTo(s, exportStream);

                Assert.Greater(exportStream.Length, 0);
            });

            var imported = skinManager.Import(new ImportTask(exportStream, "exported.osk"));

            imported.GetResultSafely().PerformRead(s =>
            {
                Assert.IsFalse(s.Protected);
                Assert.AreNotEqual(originalSkinId, s.ID);
                Assert.AreEqual(typeof(DefaultSkin), s.CreateInstance(skinManager).GetType());
            });

            return Task.CompletedTask;
        });

        [Test]
        public Task TestExportThenImportClassicSkin() => runSkinTest(osu =>
        {
            var skinManager = osu.Dependencies.Get<SkinManager>();

            skinManager.CurrentSkinInfo.Value = skinManager.DefaultLegacySkin.SkinInfo;

            skinManager.EnsureMutableSkin();

            MemoryStream exportStream = new MemoryStream();

            Guid originalSkinId = skinManager.CurrentSkinInfo.Value.ID;

            skinManager.CurrentSkinInfo.Value.PerformRead(s =>
            {
                Assert.IsFalse(s.Protected);
                Assert.AreEqual(typeof(DefaultLegacySkin), s.CreateInstance(skinManager).GetType());

                new LegacySkinExporter(osu.Dependencies.Get<Storage>()).ExportModelTo(s, exportStream);

                Assert.Greater(exportStream.Length, 0);
            });

            var imported = skinManager.Import(new ImportTask(exportStream, "exported.osk"));

            imported.GetResultSafely().PerformRead(s =>
            {
                Assert.IsFalse(s.Protected);
                Assert.AreNotEqual(originalSkinId, s.ID);
                Assert.AreEqual(typeof(DefaultLegacySkin), s.CreateInstance(skinManager).GetType());
            });

            return Task.CompletedTask;
        });

        #endregion

        private void assertCorrectMetadata(ILive<SkinInfo> import1, string name, string creator, OsuGameBase osu)
        {
            import1.PerformRead(i =>
            {
                Assert.That(i.Name, Is.EqualTo(name));
                Assert.That(i.Creator, Is.EqualTo(creator));

                // for extra safety let's reconstruct the skin, reading from the skin.ini.
                var instance = i.CreateInstance((IStorageResourceProvider)osu.Dependencies.Get(typeof(SkinManager)));

                Assert.That(instance.Configuration.SkinInfo.Name, Is.EqualTo(name));
                Assert.That(instance.Configuration.SkinInfo.Creator, Is.EqualTo(creator));
            });
        }

        private void assertImportedBoth(ILive<SkinInfo> import1, ILive<SkinInfo> import2)
        {
            import1.PerformRead(i1 => import2.PerformRead(i2 =>
            {
                Assert.That(i2.ID, Is.Not.EqualTo(i1.ID));
                Assert.That(i2.Hash, Is.Not.EqualTo(i1.Hash));
                Assert.That(i2.Files.First(), Is.Not.EqualTo(i1.Files.First()));
            }));
        }

        private void assertImportedOnce(ILive<SkinInfo> import1, ILive<SkinInfo> import2)
        {
            import1.PerformRead(i1 => import2.PerformRead(i2 =>
            {
                Assert.That(i2.ID, Is.EqualTo(i1.ID));
                Assert.That(i2.Hash, Is.EqualTo(i1.Hash));
                Assert.That(i2.Files.First(), Is.EqualTo(i1.Files.First()));
            }));
        }

        private MemoryStream createEmptyOsk()
        {
            var zipStream = new MemoryStream();
            using var zip = ZipArchive.Create();
            zip.SaveTo(zipStream);
            return zipStream;
        }

        private MemoryStream createOskWithNonIniFile()
        {
            var zipStream = new MemoryStream();
            using var zip = ZipArchive.Create();
            zip.AddEntry("hitcircle.png", new MemoryStream(new byte[] { 0, 1, 2, 3 }));
            zip.SaveTo(zipStream);
            return zipStream;
        }

        private MemoryStream createOskWithIni(string name, string author, bool makeUnique = false, string iniFilename = @"skin.ini", bool includeSectionHeader = true)
        {
            var zipStream = new MemoryStream();
            using var zip = ZipArchive.Create();
            zip.AddEntry(iniFilename, generateSkinIni(name, author, makeUnique, includeSectionHeader));
            zip.SaveTo(zipStream);
            return zipStream;
        }

        private MemoryStream generateSkinIni(string name, string author, bool makeUnique = true, bool includeSectionHeader = true)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            if (includeSectionHeader)
                writer.WriteLine("[General]");

            writer.WriteLine($"Name: {name}");
            writer.WriteLine($"Author: {author}");

            if (makeUnique)
            {
                writer.WriteLine();
                writer.WriteLine($"# unique {Guid.NewGuid()}");
            }

            writer.Flush();

            return stream;
        }

        private async Task runSkinTest(Func<OsuGameBase, Task> action, [CallerMemberName] string callingMethodName = @"")
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(callingMethodName: callingMethodName))
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);
                    await action(osu);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private async Task<ILive<SkinInfo>> loadSkinIntoOsu(OsuGameBase osu, ArchiveReader archive = null)
        {
            var skinManager = osu.Dependencies.Get<SkinManager>();
            return await skinManager.Import(archive);
        }
    }
}
