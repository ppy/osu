// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch
{
    public class CatchSkinComponent : GameplaySkinComponent<CatchSkinComponents>
    {
        public CatchSkinComponent(CatchSkinComponents component)
            : base(component)
        {
        }

        protected override string RulesetPrefix => "catch"; // todo: use CatchRuleset.SHORT_NAME;

        protected override string ComponentName => Component.ToString().ToLower();
    }
}
