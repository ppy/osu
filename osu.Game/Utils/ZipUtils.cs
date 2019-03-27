// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using SharpCompress.Archives.Zip;

namespace osu.Game.Utils
{
    public static class ZipUtils
    {
        public static bool IsZipArchive(string path)
        {
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
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
