// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu
{
    public class OsuSkinComponent : GameplaySkinComponent<OsuSkinComponents>
    {
        public OsuSkinComponent(OsuSkinComponents component)
            : base(component)
        {
        }

        protected override string RulesetPrefix => OsuRuleset.SHORT_NAME;

        protected override string ComponentName => Component.ToString().ToLower();
    }
}
