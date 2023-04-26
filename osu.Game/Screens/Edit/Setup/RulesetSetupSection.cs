// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Setup
{
    public abstract partial class RulesetSetupSection : SetupSection
    {
        public sealed override LocalisableString Title => EditorSetupStrings.RulesetHeader(rulesetInfo.Name);

        private readonly RulesetInfo rulesetInfo;

        protected RulesetSetupSection(RulesetInfo rulesetInfo)
        {
            this.rulesetInfo = rulesetInfo;
        }
    }
}
