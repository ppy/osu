// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
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
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin.osk"));

            // When the import filename doesn't match, it should be appended (and update the skin.ini).
            assertCorrectMetadata(imported, "test skin [skin]", "skinner", osu);
        });

        [Test]
        public Task TestSingleImportMatchingFilename() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "test skin.osk"));

            // When the import filename matches it shouldn't be appended.
            assertCorrectMetadata(imported, "test skin", "skinner", osu);
        });

        [Test]
        public Task TestSingleImportNoIniFile() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithNonIniFile(), "test skin.osk"));

            // When the import filename matches it shouldn't be appended.
            assertCorrectMetadata(imported, "test skin", "Unknown", osu);
        });

        [Test]
        public Task TestEmptyImportFails() => runSkinTest(osu =>
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => loadSkinIntoOsu(osu, new ZipArchiveReader(createEmptyOsk(), "test skin.osk")));

            return Task.CompletedTask;
        });

        #endregion

        #region Cases where imports should match existing

        [Test]
        public Task TestImportTwiceWithSameMetadataAndFilename() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin.osk"));

            assertImportedOnce(imported, imported2);
        });

        [Test]
        public Task TestImportTwiceWithNoMetadataSameDownloadFilename() => runSkinTest(async osu =>
        {
            // if a user downloads two skins that do have skin.ini files but don't have any creator metadata in the skin.ini, they should both import separately just for safety.
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni(string.Empty, string.Empty), "download.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni(string.Empty, string.Empty), "download.osk"));

            assertImportedOnce(imported, imported2);
        });

        [Test]
        public Task TestImportUpperCasedOskArchive() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "name 1.OsK"));
            assertCorrectMetadata(imported, "name 1", "author 1", osu);

            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "name 1.oSK"));

            assertImportedOnce(imported, imported2);
        });

        [Test]
        public Task TestSameMetadataNameSameFolderName() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "my custom skin 1"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "my custom skin 1"));

            assertImportedOnce(imported, imported2);
            assertCorrectMetadata(imported, "name 1 [my custom skin 1]", "author 1", osu);
        });

        #endregion

        #region Cases where imports should be uniquely imported

        [Test]
        public Task TestImportTwiceWithSameMetadataButDifferentFilename() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin", "skinner"), "skin2.osk"));

            assertImportedBoth(imported, imported2);
        });

        [Test]
        public Task TestImportTwiceWithNoMetadataDifferentDownloadFilename() => runSkinTest(async osu =>
        {
            // if a user downloads two skins that do have skin.ini files but don't have any creator metadata in the skin.ini, they should both import separately just for safety.
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni(string.Empty, string.Empty), "download.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni(string.Empty, string.Empty), "download2.osk"));

            assertImportedBoth(imported, imported2);
        });

        [Test]
        public Task TestImportTwiceWithSameFilenameDifferentMetadata() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin v2", "skinner"), "skin.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("test skin v2.1", "skinner"), "skin.osk"));

            assertImportedBoth(imported, imported2);
            assertCorrectMetadata(imported, "test skin v2 [skin]", "skinner", osu);
            assertCorrectMetadata(imported2, "test skin v2.1 [skin]", "skinner", osu);
        });

        [Test]
        public Task TestSameMetadataNameDifferentFolderName() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "my custom skin 1"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOskWithIni("name 1", "author 1"), "my custom skin 2"));

            assertImportedBoth(imported, imported2);
            assertCorrectMetadata(imported, "name 1 [my custom skin 1]", "author 1", osu);
            assertCorrectMetadata(imported2, "name 1 [my custom skin 2]", "author 1", osu);
        });

        #endregion

        private void assertCorrectMetadata(SkinInfo imported, string name, string creator, OsuGameBase osu)
        {
            Assert.That(imported.Name, Is.EqualTo(name));
            Assert.That(imported.Creator, Is.EqualTo(creator));

            // for extra safety let's reconstruct the skin, reading from the skin.ini.
            var instance = imported.CreateInstance((IStorageResourceProvider)osu.Dependencies.Get(typeof(SkinManager)));

            Assert.That(instance.Configuration.SkinInfo.Name, Is.EqualTo(name));
            Assert.That(instance.Configuration.SkinInfo.Creator, Is.EqualTo(creator));
        }

        private void assertImportedBoth(SkinInfo imported, SkinInfo imported2)
        {
            Assert.That(imported2.ID, Is.Not.EqualTo(imported.ID));
            Assert.That(imported2.Hash, Is.Not.EqualTo(imported.Hash));
            Assert.That(imported2.Files.Select(f => f.FileInfoID), Is.Not.EquivalentTo(imported.Files.Select(f => f.FileInfoID)));
        }

        private void assertImportedOnce(SkinInfo imported, SkinInfo imported2)
        {
            Assert.That(imported2.ID, Is.EqualTo(imported.ID));
            Assert.That(imported2.Hash, Is.EqualTo(imported.Hash));
            Assert.That(imported2.Files.Select(f => f.FileInfoID), Is.EquivalentTo(imported.Files.Select(f => f.FileInfoID)));
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

        private MemoryStream createOskWithIni(string name, string author, bool makeUnique = false)
        {
            var zipStream = new MemoryStream();
            using var zip = ZipArchive.Create();
            zip.AddEntry("skin.ini", generateSkinIni(name, author, makeUnique));
            zip.SaveTo(zipStream);
            return zipStream;
        }

        private MemoryStream generateSkinIni(string name, string author, bool makeUnique = true)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

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
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(callingMethodName))
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

        private async Task<SkinInfo> loadSkinIntoOsu(OsuGameBase osu, ArchiveReader archive = null)
        {
            var skinManager = osu.Dependencies.Get<SkinManager>();
            return (await skinManager.Import(archive)).Value;
        }
    }
}
