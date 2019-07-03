// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDeflate : OsuModeObjectScaleTween
    {
        public override string Name => "Deflate";

        public override string Acronym => "DF";

        public override IconUsage Icon => FontAwesome.Solid.CompressArrowsAlt;

        public override string Description => "Hit them at the right size!";

        protected override float StartScale => 2f;
    }
}
