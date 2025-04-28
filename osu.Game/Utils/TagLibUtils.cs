// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Game.IO.FileAbstraction;
using TagLib;

namespace osu.Game.Utils
{
    public class TagLibUtils
    {
        /// <summary>
        /// Creates a <see cref="TagLib.File"/> with culture-invariant MIME type detection.
        /// </summary>
        /// <param name="fileAbstraction">The file abstraction of the file to be created.</param>
        /// <returns>The <see cref="TagLib.File"/> created.</returns>
        public static TagLib.File CreateFile(StreamFileAbstraction fileAbstraction) =>
            TagLib.File.Create(fileAbstraction, getMimeType(fileAbstraction.Name), ReadStyle.Average);

        /// <summary>
        /// Creates a <see cref="TagLib.File"/> with culture-invariant MIME type detection.
        /// </summary>
        /// <param name="filePath">The full path of the file to be created.</param>
        /// <returns>The <see cref="TagLib.File"/> created.</returns>
        public static TagLib.File CreateFile(string filePath) =>
            TagLib.File.Create(filePath, getMimeType(filePath), ReadStyle.Average);

        // Manual MIME type resolution to avoid culture variance (ie. https://github.com/ppy/osu/issues/32962)
        private static string getMimeType(string fileName) =>
            @"taglib/" + Path.GetExtension(fileName).TrimStart('.');
    }
}
