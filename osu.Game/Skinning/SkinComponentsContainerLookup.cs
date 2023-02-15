// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class SkinComponentsContainerLookup : ISkinComponentLookup
    {
        public readonly TargetArea Target;

        public SkinComponentsContainerLookup(TargetArea target)
        {
            Target = target;
        }

        public enum TargetArea
        {
            MainHUDComponents,
            SongSelect
        }
    }
}
