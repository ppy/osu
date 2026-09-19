// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace osu.Game.Utils
{
    /// <summary>
    /// Extracts prominent combo colours from a beatmap background.
    /// </summary>
    public static class BackgroundComboColourExtractor
    {
        public const int MAX_COLOUR_COUNT = 8;

        // Resolution to downscale to before quantizing.
        private const int max_dimension = 128;

        // Ranking criteria: avoid colours with lightness <50 or >220.
        private const float min_lightness = 50 / 255f;
        private const float max_lightness = 220 / 255f;

        public static Colour4[] Extract(Stream imageStream)
        {
            using var image = Image.Load<Rgba32>(imageStream);

            image.Mutate(context => context.Resize(new ResizeOptions
            {
                Size = new Size(max_dimension, max_dimension),
                Mode = ResizeMode.Max,
            }));

            var quantizer = new WuQuantizer(new QuantizerOptions
            {
                MaxColors = MAX_COLOUR_COUNT,
            });

            using var frameQuantizer = quantizer.CreatePixelSpecificQuantizer<Rgba32>(SixLabors.ImageSharp.Configuration.Default);
            using var result = frameQuantizer.BuildPaletteAndQuantizeFrame(image.Frames.RootFrame, image.Bounds);

            return result.Palette.ToArray()
                         .Where(p => p.A >= 128)
                         .Select(p => clampLightness(new Colour4(p.R, p.G, p.B, 255)))
                         .ToArray();
        }

        private static Colour4 clampLightness(Colour4 colour)
        {
            var hsl = colour.ToHSL();
            return Colour4.FromHSL(hsl.X, hsl.Y, Math.Clamp(hsl.Z, min_lightness, max_lightness));
        }
    }
}
