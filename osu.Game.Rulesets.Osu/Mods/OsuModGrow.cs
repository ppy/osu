// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModGrow : OsuModeObjectScaleTween
    {
        public override string Name => "Grow";

        public override string Acronym => "GR";

        public override IconUsage Icon => FontAwesome.Solid.ArrowsAltV;

        public override string Description => "Hit them at the right size!";

        protected override float StartScale => 0.5f;
    }
}
