// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using osu.Game.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class BackgroundComboColourExtractorTest
    {
        [Test]
        public void TestExtractFindsDistinctColours()
        {
            using var stream = createImage((x, y) => (x < 50, y < 50) switch
            {
                (true, true) => new Rgba32(30, 120, 220), // blue
                (false, true) => new Rgba32(220, 180, 30), // yellow
                (true, false) => new Rgba32(40, 180, 60), // green
                (false, false) => new Rgba32(220, 40, 40), // red
            });

            var colours = BackgroundComboColourExtractor.Extract(stream);

            Assert.That(colours.Any(c => c.B > c.R && c.B > c.G), Is.True); // blue
            Assert.That(colours.Any(c => c.R > c.B && c.G > c.B), Is.True); // yellow
            Assert.That(colours.Any(c => c.G > c.R && c.G > c.B), Is.True); // green
            Assert.That(colours.Any(c => c.R > c.G && c.R > c.B), Is.True); // red
        }

        private static MemoryStream createImage(Func<int, int, Rgba32> getPixel)
        {
            var image = new Image<Rgba32>(100, 100);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                    image[x, y] = getPixel(x, y);
            }

            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
