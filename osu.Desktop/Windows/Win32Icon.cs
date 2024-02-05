// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Desktop.Windows
{
    public class Win32Icon
    {
        public readonly string Path;

        internal Win32Icon(string name)
        {
            string dir = System.IO.Path.GetDirectoryName(typeof(Win32Icon).Assembly.Location)!;
            Path = System.IO.Path.Join(dir, name);
        }
    }
}
