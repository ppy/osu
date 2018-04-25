// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
