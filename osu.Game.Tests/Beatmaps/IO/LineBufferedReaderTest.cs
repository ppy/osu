// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using osu.Game.IO;

namespace osu.Game.Tests.Beatmaps.IO
{
    [TestFixture]
    public class LineBufferedReaderTest
    {
        [Test]
        public void TestReadLineByLine()
        {
            const string contents = "line 1\rline 2\nline 3";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.AreEqual("line 1", bufferedReader.ReadLine());
                Assert.AreEqual("line 2", bufferedReader.ReadLine());
                Assert.AreEqual("line 3", bufferedReader.ReadLine());
                Assert.IsNull(bufferedReader.ReadLine());
            }
        }

        [Test]
        public void TestPeekLineOnce()
        {
            const string contents = "line 1\r\npeek this\nline 3";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.AreEqual("line 1", bufferedReader.ReadLine());
                Assert.AreEqual("peek this", bufferedReader.PeekLine());
                Assert.AreEqual("peek this", bufferedReader.ReadLine());
                Assert.AreEqual("line 3", bufferedReader.ReadLine());
                Assert.IsNull(bufferedReader.ReadLine());
            }
        }

        [Test]
        public void TestPeekLineMultipleTimes()
        {
            const string contents = "peek this once\nline 2\rpeek this a lot";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.AreEqual("peek this once", bufferedReader.PeekLine());
                Assert.AreEqual("peek this once", bufferedReader.ReadLine());
                Assert.AreEqual("line 2", bufferedReader.ReadLine());
                Assert.AreEqual("peek this a lot", bufferedReader.PeekLine());
                Assert.AreEqual("peek this a lot", bufferedReader.PeekLine());
                Assert.AreEqual("peek this a lot", bufferedReader.PeekLine());
                Assert.AreEqual("peek this a lot", bufferedReader.ReadLine());
                Assert.IsNull(bufferedReader.ReadLine());
            }
        }

        [Test]
        public void TestPeekLineAtEndOfStream()
        {
            const string contents = "first line\r\nsecond line";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.AreEqual("first line", bufferedReader.ReadLine());
                Assert.AreEqual("second line", bufferedReader.ReadLine());
                Assert.IsNull(bufferedReader.PeekLine());
                Assert.IsNull(bufferedReader.ReadLine());
                Assert.IsNull(bufferedReader.PeekLine());
            }
        }

        [Test]
        public void TestPeekReadLineOnEmptyStream()
        {
            using (var stream = new MemoryStream())
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.IsNull(bufferedReader.PeekLine());
                Assert.IsNull(bufferedReader.ReadLine());
                Assert.IsNull(bufferedReader.ReadLine());
                Assert.IsNull(bufferedReader.PeekLine());
            }
        }

        [Test]
        public void TestReadToEndNoPeeks()
        {
            const string contents = "first line\r\nsecond line";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.AreEqual(contents, bufferedReader.ReadToEnd());
            }
        }

        [Test]
        public void TestReadToEndAfterReadsAndPeeks()
        {
            const string contents = "this line is gone\rthis one shouldn't be\r\nthese ones\ndefinitely not";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.AreEqual("this line is gone", bufferedReader.ReadLine());
                Assert.AreEqual("this one shouldn't be", bufferedReader.PeekLine());

                var endingLines = bufferedReader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(3, endingLines.Length);
                Assert.AreEqual("this one shouldn't be", endingLines[0]);
                Assert.AreEqual("these ones", endingLines[1]);
                Assert.AreEqual("definitely not", endingLines[2]);
            }
        }
    }
}
