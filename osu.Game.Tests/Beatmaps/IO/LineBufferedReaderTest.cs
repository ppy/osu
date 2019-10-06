// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            const string contents = @"line 1
line 2
line 3";

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
            const string contents = @"line 1
peek this
line 3";

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
            const string contents = @"peek this once
line 2
peek this a lot";

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
            const string contents = @"first line
second line";

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
            const string contents = @"first line
second line";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.AreEqual(contents, bufferedReader.ReadToEnd());
            }
        }

        [Test]
        public void TestReadToEndAfterReadsAndPeeks()
        {
            const string contents = @"this line is gone
this one shouldn't be
these ones
definitely not";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            using (var bufferedReader = new LineBufferedReader(stream))
            {
                Assert.AreEqual("this line is gone", bufferedReader.ReadLine());
                Assert.AreEqual("this one shouldn't be", bufferedReader.PeekLine());
                const string ending = @"this one shouldn't be
these ones
definitely not";
                Assert.AreEqual(ending, bufferedReader.ReadToEnd());
            }
        }
    }
}
