// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class SkinnableTargetLookup : ISkinLookup
    {
        public readonly SkinnableTarget Target;

        public string LookupName => Target.ToString();

        public SkinnableTargetLookup(SkinnableTarget target)
        {
            Target = target;
        }
    }
}
