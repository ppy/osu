// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMirror : Mod
    {
        public override string Name => "镜像";
        public override string Acronym => "MR";
        public override string Description => "镜像模式!";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;
    }
}
