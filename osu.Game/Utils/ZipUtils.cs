// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using SharpCompress.Archives.Zip;

namespace osu.Game.Utils
{
    public static class ZipUtils
    {
        public static bool IsZipArchive(MemoryStream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);

                using (var arc = ZipArchive.Open(stream))
                {
                    foreach (var entry in arc.Entries)
                    {
                        using (entry.OpenEntryStream())
                        {
                        }
                    }

                    // aside from opening every zip entry not failing, we also require there to *be* at least one entry.
                    // if there are no entries, the best case is that it's an actual empty zip
                    // and as such probably useless to whatever wants to use it later.
                    // the worst case is that it's actually *not* a zip and instead a stream of binary
                    // which *accidentally* happened to contain the magic sequence of bytes for the zip header (50 4b 05 06),
                    // and if that's the case, then we are *misclassifying* it as a zip by returning `true` unconditionally.
                    return arc.Entries.Count > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        public static bool IsZipArchive(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                using (var arc = ZipArchive.Open(path))
                {
                    foreach (var entry in arc.Entries)
                    {
                        using (entry.OpenEntryStream())
                        {
                        }
                    }

                    // aside from opening every zip entry not failing, we also require there to *be* at least one entry.
                    // if there are no entries, the best case is that it's an actual empty zip
                    // and as such probably useless to whatever wants to use it later.
                    // the worst case is that it's actually *not* a zip and instead a stream of binary
                    // which *accidentally* happened to contain the magic sequence of bytes for the zip header (50 4b 05 06),
                    // and if that's the case, then we are *misclassifying* it as a zip by returning `true` unconditionally.
                    return arc.Entries.Count > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
