// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class GlobalSkinLookup : ISkinLookup
    {
        public readonly LookupType Lookup;

        public string LookupName => Lookup.ToString();

        public GlobalSkinLookup(LookupType lookup)
        {
            Lookup = lookup;
        }

        public enum LookupType
        {
            MainHUDComponents,
            SongSelect
        }
    }
}
