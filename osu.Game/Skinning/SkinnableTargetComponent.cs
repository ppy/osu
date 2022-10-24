// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Skinning
{
    public class SkinnableTargetComponent : ISkinComponent
    {
        public readonly SkinnableTarget Target;

        public string LookupName => Target.ToString();

        public SkinnableTargetComponent(SkinnableTarget target)
        {
            Target = target;
        }
    }
}
