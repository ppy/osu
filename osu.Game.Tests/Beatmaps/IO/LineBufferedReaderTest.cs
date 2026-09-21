// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
                ClassicAssert.AreEqual("line 1", bufferedReader.ReadLine());
                ClassicAssert.AreEqual("line 2", bufferedReader.ReadLine());
                ClassicAssert.AreEqual("line 3", bufferedReader.ReadLine());
                ClassicAssert.Null(bufferedReader.ReadLine());
            }
        }

        [Test]
        public void TestPeekLineOnce()
        {
            const string contents = "line 1\r\npeek this\nline 3";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                ClassicAssert.AreEqual("line 1", bufferedReader.ReadLine());
                ClassicAssert.AreEqual("peek this", bufferedReader.PeekLine());
                ClassicAssert.AreEqual("peek this", bufferedReader.ReadLine());
                ClassicAssert.AreEqual("line 3", bufferedReader.ReadLine());
                ClassicAssert.Null(bufferedReader.ReadLine());
            }
        }

        [Test]
        public void TestPeekLineMultipleTimes()
        {
            const string contents = "peek this once\nline 2\rpeek this a lot";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                ClassicAssert.AreEqual("peek this once", bufferedReader.PeekLine());
                ClassicAssert.AreEqual("peek this once", bufferedReader.ReadLine());
                ClassicAssert.AreEqual("line 2", bufferedReader.ReadLine());
                ClassicAssert.AreEqual("peek this a lot", bufferedReader.PeekLine());
                ClassicAssert.AreEqual("peek this a lot", bufferedReader.PeekLine());
                ClassicAssert.AreEqual("peek this a lot", bufferedReader.PeekLine());
                ClassicAssert.AreEqual("peek this a lot", bufferedReader.ReadLine());
                ClassicAssert.Null(bufferedReader.ReadLine());
            }
        }

        [Test]
        public void TestPeekLineAtEndOfStream()
        {
            const string contents = "first line\r\nsecond line";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                ClassicAssert.AreEqual("first line", bufferedReader.ReadLine());
                ClassicAssert.AreEqual("second line", bufferedReader.ReadLine());
                ClassicAssert.Null(bufferedReader.PeekLine());
                ClassicAssert.Null(bufferedReader.ReadLine());
                ClassicAssert.Null(bufferedReader.PeekLine());
            }
        }

        [Test]
        public void TestPeekReadLineOnEmptyStream()
        {
            using (var stream = new MemoryStream())
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                ClassicAssert.Null(bufferedReader.PeekLine());
                ClassicAssert.Null(bufferedReader.ReadLine());
                ClassicAssert.Null(bufferedReader.ReadLine());
                ClassicAssert.Null(bufferedReader.PeekLine());
            }
        }

        [Test]
        public void TestReadToEndNoPeeks()
        {
            const string contents = "first line\r\nsecond line";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                ClassicAssert.AreEqual(contents, bufferedReader.ReadToEnd());
            }
        }

        [Test]
        public void TestReadToEndAfterReadsAndPeeks()
        {
            const string contents = "this line is gone\rthis one shouldn't be\r\nthese ones\ndefinitely not";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                ClassicAssert.AreEqual("this line is gone", bufferedReader.ReadLine());
                ClassicAssert.AreEqual("this one shouldn't be", bufferedReader.PeekLine());

                string[] endingLines = bufferedReader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                ClassicAssert.AreEqual(3, endingLines.Length);
                ClassicAssert.AreEqual("this one shouldn't be", endingLines[0]);
                ClassicAssert.AreEqual("these ones", endingLines[1]);
                ClassicAssert.AreEqual("definitely not", endingLines[2]);
            }
        }
    }
}
