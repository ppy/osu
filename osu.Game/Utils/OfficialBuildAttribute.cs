// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Utils
{
    /// <summary>
    /// This attribute is automatically attached to assemblies of projects built with a <c>Version</c> property specified.
    /// It is purely informational - used to alert the user of the (lack of) online capabilities and of the game's version.
    /// <p />
    /// Refer to <c>Directory.Build.targets</c> in the repository root for how this attribute is attached.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class OfficialBuildAttribute : Attribute
    {
        public readonly string Version;

        public OfficialBuildAttribute(string version)
        {
            Version = version;
        }
    }
}
