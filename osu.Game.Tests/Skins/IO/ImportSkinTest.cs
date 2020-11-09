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
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins().Count, Is.EqualTo(1));

                    // the first should be overwritten by the second import.
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins().First().Files.First().FileInfoID, Is.EqualTo(imported2.Files.First().FileInfoID));
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
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins().Count, Is.EqualTo(2));

                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins().First().Files.First().FileInfoID, Is.EqualTo(imported.Files.First().FileInfoID));
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins().Last().Files.First().FileInfoID, Is.EqualTo(imported2.Files.First().FileInfoID));
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
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins().Count, Is.EqualTo(2));

                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins().First().Files.First().FileInfoID, Is.EqualTo(imported.Files.First().FileInfoID));
                    Assert.That(osu.Dependencies.Get<SkinManager>().GetAllUserSkins().Last().Files.First().FileInfoID, Is.EqualTo(imported2.Files.First().FileInfoID));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private MemoryStream createOsk(string name, string author)
        {
            var zipStream = new MemoryStream();
            using var zip = ZipArchive.Create();
            zip.AddEntry("skin.ini", generateSkinIni(name, author));
            zip.SaveTo(zipStream);
            return zipStream;
        }

        private MemoryStream generateSkinIni(string name, string author)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("[General]");
            writer.WriteLine($"Name: {name}");
            writer.WriteLine($"Author: {author}");
            writer.WriteLine();
            writer.WriteLine($"# unique {Guid.NewGuid()}");

            writer.Flush();

            return stream;
        }

        private async Task<SkinInfo> loadSkinIntoOsu(OsuGameBase osu, ArchiveReader archive = null)
        {
            var skinManager = osu.Dependencies.Get<SkinManager>();
            return await skinManager.Import(archive);
        }
    }
}
