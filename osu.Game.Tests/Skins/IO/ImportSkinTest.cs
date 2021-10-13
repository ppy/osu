// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
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
        [Test]
        public async Task TestBasicImport()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(ImportSkinTest)))
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "skin.osk"));

                    Assert.That(imported.Name, Is.EqualTo("test skin"));
                    Assert.That(imported.Creator, Is.EqualTo("skinner"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportTwiceWithSameMetadata()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(ImportSkinTest)))
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "skin.osk"));
                    var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin", "skinner"), "skin2.osk"));

                    Assert.That(imported2.ID, Is.Not.EqualTo(imported.ID));
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins(true).Count, Is.EqualTo(1));

                    // the first should be overwritten by the second import.
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins(true).First().Files.First().FileInfoID, Is.EqualTo(imported2.Files.First().FileInfoID));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportTwiceWithNoMetadata()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(ImportSkinTest)))
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    // if a user downloads two skins that do have skin.ini files but don't have any creator metadata in the skin.ini, they should both import separately just for safety.
                    var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk(string.Empty, string.Empty), "download.osk"));
                    var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk(string.Empty, string.Empty), "download.osk"));

                    Assert.That(imported2.ID, Is.Not.EqualTo(imported.ID));
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins(true).Count, Is.EqualTo(2));

                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins(true).First().Files.First().FileInfoID, Is.EqualTo(imported.Files.First().FileInfoID));
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins(true).Last().Files.First().FileInfoID, Is.EqualTo(imported2.Files.First().FileInfoID));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportTwiceWithDifferentMetadata()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(ImportSkinTest)))
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin v2", "skinner"), "skin.osk"));
                    var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("test skin v2.1", "skinner"), "skin2.osk"));

                    Assert.That(imported2.ID, Is.Not.EqualTo(imported.ID));
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins(true).Count, Is.EqualTo(2));

                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins(true).First().Files.First().FileInfoID, Is.EqualTo(imported.Files.First().FileInfoID));
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins(true).Last().Files.First().FileInfoID, Is.EqualTo(imported2.Files.First().FileInfoID));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportUpperCasedOskArchive()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(ImportSkinTest)))
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("name 1", "author 1"), "skin1.OsK"));

                    Assert.That(imported.Name, Is.EqualTo("name 1"));
                    Assert.That(imported.Creator, Is.EqualTo("author 1"));

                    var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("name 1", "author 1"), "skin1.oSK"));

                    Assert.That(imported2.Hash, Is.EqualTo(imported.Hash));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestSameMetadataNameDifferentFolderName()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost(nameof(ImportSkinTest)))
            {
                try
                {
                    var osu = LoadOsuIntoHost(host);

                    var imported = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("name 1", "author 1", false), "my custom skin 1"));
                    Assert.That(imported.Name, Is.EqualTo("name 1 [my custom skin 1]"));
                    Assert.That(imported.Creator, Is.EqualTo("author 1"));

                    var imported2 = await loadSkinIntoOsu(osu, new ZipArchiveReader(createOsk("name 1", "author 1", false), "my custom skin 2"));
                    Assert.That(imported2.Name, Is.EqualTo("name 1 [my custom skin 2]"));
                    Assert.That(imported2.Creator, Is.EqualTo("author 1"));

                    Assert.That(imported2.Hash, Is.Not.EqualTo(imported.Hash));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private MemoryStream createOsk(string name, string author, bool makeUnique = true)
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

        private async Task<SkinInfo> loadSkinIntoOsu(OsuGameBase osu, ArchiveReader archive = null)
        {
            var skinManager = osu.Dependencies.Get<SkinManager>();
            return (await skinManager.Import(archive)).Value;
        }
    }
}
