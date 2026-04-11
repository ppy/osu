// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using osu.Game.Database;

namespace osu.Game.Tests.Database
{
    /// <summary>
    /// Tests for <see cref="OsuDbReader"/> using hand-crafted minimal <c>osu!.db</c> streams.
    /// </summary>
    [TestFixture]
    public class OsuDbReaderTest
    {
        /// <summary>
        /// A single beatmap entry with a known modification time should be returned correctly.
        /// </summary>
        [Test]
        public void TestReadDateAddedByFolder()
        {
            var expectedDate = new DateTimeOffset(new DateTime(2015, 6, 1, 12, 0, 0, DateTimeKind.Utc));

            using var stream = buildMinimalOsuDb(version: 20191106, folderName: "241526 Renatus", lastModifiedTicks: expectedDate.UtcTicks);
            var result = OsuDbReader.ReadDateAddedByFolder(stream);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ContainsKey("241526 Renatus"), Is.True);
            Assert.That(result["241526 Renatus"], Is.EqualTo(expectedDate));
        }

        /// <summary>
        /// When multiple beatmap entries share the same folder (different difficulties),
        /// the reader should keep the earliest modification time.
        /// </summary>
        [Test]
        public void TestEarliestDateKeptForSameFolder()
        {
            var earlier = new DateTimeOffset(new DateTime(2014, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var later = new DateTimeOffset(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            using var stream = buildOsuDbWithTwoEntries(
                version: 20191106,
                folderName: "shared-folder",
                ticks1: later.UtcTicks,
                ticks2: earlier.UtcTicks);

            var result = OsuDbReader.ReadDateAddedByFolder(stream);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!["shared-folder"], Is.EqualTo(earlier));
        }

        /// <summary>
        /// Verifies that the legacy entry-size prefix (present in versions &lt; 20191106) is handled correctly.
        /// </summary>
        [Test]
        public void TestLegacyVersionWithSizePrefix()
        {
            var expectedDate = new DateTimeOffset(new DateTime(2013, 3, 15, 8, 0, 0, DateTimeKind.Utc));

            using var stream = buildMinimalOsuDb(version: 20130815, folderName: "legacy-folder", lastModifiedTicks: expectedDate.UtcTicks);
            var result = OsuDbReader.ReadDateAddedByFolder(stream);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!["legacy-folder"], Is.EqualTo(expectedDate));
        }

        /// <summary>
        /// Verifies that beatmap entries with invalid ticks values are skipped gracefully.
        /// </summary>
        [Test]
        public void TestInvalidTicksAreSkipped()
        {
            using var stream = buildMinimalOsuDb(version: 20191106, folderName: "invalid-ticks", lastModifiedTicks: -1);
            var result = OsuDbReader.ReadDateAddedByFolder(stream);

            // The entry should be skipped, resulting in an empty dictionary.
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Verifies that when one entry has invalid ticks but another is valid, only the valid one is kept.
        /// </summary>
        [Test]
        public void TestMixedValidAndInvalidTicks()
        {
            var validDate = new DateTimeOffset(new DateTime(2015, 6, 1, 12, 0, 0, DateTimeKind.Utc));

            using var stream = buildOsuDbWithTwoEntries(
                version: 20191106,
                folderName: "mixed-folder",
                ticks1: -1, // invalid
                ticks2: validDate.UtcTicks); // valid

            var result = OsuDbReader.ReadDateAddedByFolder(stream);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ContainsKey("mixed-folder"), Is.True);
            Assert.That(result["mixed-folder"], Is.EqualTo(validDate));
        }

        /// <summary>
        /// Verifies that files with unreasonable beatmap counts are rejected gracefully.
        /// </summary>
        [Test]
        public void TestUnreasonableBeatmapCountIsRejected()
        {
            var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            bw.Write(20191106); // version
            bw.Write(1);        // folder count
            bw.Write(true);     // account unlocked
            bw.Write(0L);       // unlock date
            writeOsuString(bw, "TestPlayer");
            bw.Write(2_000_000); // unreasonable beatmap count (> 1 million)

            ms.Position = 0;

            var result = OsuDbReader.ReadDateAddedByFolder(ms);

            // Should return empty dictionary, not crash.
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Count, Is.EqualTo(0));
        }

        // ─── helpers ────────────────────────────────────────────────────────────────

        private static MemoryStream buildMinimalOsuDb(int version, string folderName, long lastModifiedTicks)
        {
            var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            writeHeader(bw, version, beatmapCount: 1);
            writeBeatmapEntry(bw, version, folderName, lastModifiedTicks);
            bw.Write(0); // user permissions

            ms.Position = 0;
            return ms;
        }

        private static MemoryStream buildOsuDbWithTwoEntries(int version, string folderName, long ticks1, long ticks2)
        {
            var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            writeHeader(bw, version, beatmapCount: 2);
            writeBeatmapEntry(bw, version, folderName, ticks1);
            writeBeatmapEntry(bw, version, folderName, ticks2);
            bw.Write(0); // user permissions

            ms.Position = 0;
            return ms;
        }

        private static void writeHeader(BinaryWriter bw, int version, int beatmapCount)
        {
            bw.Write(version);
            bw.Write(1);       // folder count
            bw.Write(true);    // account unlocked
            bw.Write(0L);      // date account will be unlocked (ticks)
            writeOsuString(bw, "TestPlayer");
            bw.Write(beatmapCount);
        }

        private static void writeBeatmapEntry(BinaryWriter bw, int version, string folderName, long lastModifiedTicks)
        {
            long startPos = bw.BaseStream.Position;

            if (version < 20191106)
                bw.Write(0); // size placeholder – patched at the end

            writeOsuString(bw, "Artist");
            writeOsuString(bw, "ArtistUnicode");
            writeOsuString(bw, "Title");
            writeOsuString(bw, "TitleUnicode");
            writeOsuString(bw, "Creator");
            writeOsuString(bw, "Hard");
            writeOsuString(bw, "audio.mp3");
            writeOsuString(bw, "abc123md5hash");
            writeOsuString(bw, "beatmap.osu");
            bw.Write((byte)4);    // ranked status
            bw.Write((short)100); // hit circles
            bw.Write((short)50);  // sliders
            bw.Write((short)5);   // spinners
            bw.Write(lastModifiedTicks); // last modification time (Windows ticks)

            if (version < 20140609)
            {
                bw.Write((byte)5);
                bw.Write((byte)4);
                bw.Write((byte)6);
                bw.Write((byte)7);
            }
            else
            {
                bw.Write(9.0f);
                bw.Write(4.0f);
                bw.Write(6.0f);
                bw.Write(8.0f);
            }

            bw.Write(1.4); // slider velocity

            if (version >= 20140609)
            {
                // 4 rulesets, 0 star-rating pairs each
                for (int r = 0; r < 4; r++)
                    bw.Write(0);
            }

            bw.Write(180);    // drain time
            bw.Write(240000); // total time
            bw.Write(5000);   // preview time

            bw.Write(1); // timing point count
            bw.Write(180.0); // BPM
            bw.Write(0.0);   // offset
            bw.Write(true);  // inherited

            bw.Write(12345);   // difficulty ID
            bw.Write(67890);   // beatmap ID
            bw.Write(0);       // thread ID
            bw.Write((byte)0); // grade osu!
            bw.Write((byte)0); // grade taiko
            bw.Write((byte)0); // grade catch
            bw.Write((byte)0); // grade mania
            bw.Write((short)0); // local offset
            bw.Write(0.7f);    // stack leniency
            bw.Write((byte)0); // gameplay mode
            writeOsuString(bw, string.Empty); // source
            writeOsuString(bw, string.Empty); // tags
            bw.Write((short)0); // online offset
            writeOsuString(bw, string.Empty); // title font
            bw.Write(true);    // unplayed
            bw.Write(0L);      // last played
            bw.Write(false);   // is osz2
            writeOsuString(bw, folderName);
            bw.Write(0L);      // last checked
            bw.Write(false);   // ignore sound
            bw.Write(false);   // ignore skin
            bw.Write(false);   // disable storyboard
            bw.Write(false);   // disable video
            bw.Write(false);   // visual override

            if (version < 20140609)
                bw.Write((short)0); // unknown

            bw.Write(0);       // last modification time (duplicate)
            bw.Write((byte)0); // mania scroll speed

            if (version < 20191106)
            {
                long endPos = bw.BaseStream.Position;
                int entrySize = (int)(endPos - startPos - 4);
                bw.BaseStream.Position = startPos;
                bw.Write(entrySize);
                bw.BaseStream.Position = endPos;
            }
        }

        /// <summary>
        /// Writes a string in the osu!.db format: 0x0b prefix + ULEB128 length + UTF-8 bytes.
        /// An empty string is written as 0x0b + 0x00.
        /// </summary>
        private static void writeOsuString(BinaryWriter bw, string value)
        {
            if (value == null)
            {
                bw.Write((byte)0x00);
                return;
            }

            bw.Write((byte)0x0b);
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            writeUleb128(bw, (uint)bytes.Length);
            bw.BaseStream.Write(bytes, 0, bytes.Length);
        }

        private static void writeUleb128(BinaryWriter bw, uint value)
        {
            do
            {
                byte b = (byte)(value & 0x7F);
                value >>= 7;
                if (value != 0) b |= 0x80;
                bw.Write(b);
            } while (value != 0);
        }
    }
}
