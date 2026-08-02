// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    /// <summary>
    /// A lookup class which is only for internal use, and explicitly to get a user-level configuration.
    /// </summary>
    internal class UserSkinComponentLookup : ISkinComponentLookup
    {
        public readonly ISkinComponentLookup Component;

        public UserSkinComponentLookup(ISkinComponentLookup component)
        {
            Component = component;
        }
    }
}
