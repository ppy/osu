// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch
{
    public class CatchSkinComponentLookup : GameplaySkinComponentLookup<CatchSkinComponents>
    {
        public CatchSkinComponentLookup(CatchSkinComponents component)
            : base(component)
        {
        }

        public override RulesetInfo Ruleset => new CatchRuleset().RulesetInfo;
    }
}
