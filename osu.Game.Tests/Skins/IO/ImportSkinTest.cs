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
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "skin.osk"));

            // When the import filename doesn't match, it should be appended (and update the skin.ini).
            Assert.That(imported.Name, Is.EqualTo("test skin [skin]"));
            Assert.That(imported.Creator, Is.EqualTo("skinner"));
        });

        [Test]
        public Task TestSingleImportMatchingFilename() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "test skin.osk"));

            // When the import filename matches it shouldn't be appended.
            Assert.That(imported.Name, Is.EqualTo("test skin"));
            Assert.That(imported.Creator, Is.EqualTo("skinner"));
        });

        #endregion

        #region Cases where imports should match existing

        [Test]
        public Task TestImportTwiceWithSameMetadataAndFilename() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "skin.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "skin.osk"));

            assertImportedOnce(imported, imported2);
        });

        [Test]
        public Task TestImportTwiceWithNoMetadataSameDownloadFilename() => runSkinTest(async osu =>
        {
            // if a user downloads two skins that do have skin.ini files but don't have any creator metadata in the skin.ini, they should both import separately just for safety.
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk(string.Empty, string.Empty), "download.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk(string.Empty, string.Empty), "download.osk"));

            assertImportedOnce(imported, imported2);
        });

        [Test]
        public Task TestImportUpperCasedOskArchive() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("name 1", "author 1"), "name 1.OsK"));
            assertCorrectMetadata(imported, "name 1", "author 1");

            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("name 1", "author 1"), "name 1.oSK"));

            assertImportedOnce(imported, imported2);
        });

        #endregion

        #region Cases where imports should be uniquely imported

        [Test]
        public Task TestImportTwiceWithSameMetadataButDifferentFilename() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "skin.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "skin2.osk"));

            assertImportedBoth(imported, imported2);
        });

        [Test]
        public Task TestImportTwiceWithNoMetadataDifferentDownloadFilename() => runSkinTest(async osu =>
        {
            // if a user downloads two skins that do have skin.ini files but don't have any creator metadata in the skin.ini, they should both import separately just for safety.
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk(string.Empty, string.Empty), "download.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk(string.Empty, string.Empty), "download2.osk"));

            assertImportedBoth(imported, imported2);
        });

        [Test]
        public Task TestImportTwiceWithSameFilenameDifferentMetadata() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin v2", "skinner"), "skin.osk"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin v2.1", "skinner"), "skin.osk"));

            assertImportedBoth(imported, imported2);
            assertCorrectMetadata(imported, "test skin v2 [skin]", "skinner");
            assertCorrectMetadata(imported2, "test skin v2.1 [skin]", "skinner");
        });

        [Test]
        public Task TestSameMetadataNameDifferentFolderName() => runSkinTest(async osu =>
        {
            var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("name 1", "author 1"), "my custom skin 1"));
            var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("name 1", "author 1"), "my custom skin 2"));

            assertImportedBoth(imported, imported2);
            assertCorrectMetadata(imported, "name 1 [my custom skin 2]", "author 1");
            assertCorrectMetadata(imported, "name 1 [my custom skin 2]", "author 1");
        });

        #endregion

        private void assertCorrectMetadata(SkinInfo imported, string name, string creator)
        {
            Assert.That(imported.Name, Is.EqualTo(name));
            Assert.That(imported.Creator, Is.EqualTo(creator));
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

        private MemoryStream createOsk(string name, string author, bool makeUnique = false)
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
