// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Framework.Bindables;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneProfileRulesetSelector : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        public TestSceneProfileRulesetSelector()
        {
            var user = new Bindable<UserProfileData?>();

            Child = new ProfileRulesetSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                User = { BindTarget = user }
            };
            AddStep("User on osu ruleset", () => user.Value = new UserProfileData(new APIUser { Id = 0, PlayMode = "osu" }, new OsuRuleset().RulesetInfo));
            AddStep("User on taiko ruleset", () => user.Value = new UserProfileData(new APIUser { Id = 1, PlayMode = "osu" }, new TaikoRuleset().RulesetInfo));
            AddStep("User on catch ruleset", () => user.Value = new UserProfileData(new APIUser { Id = 2, PlayMode = "osu" }, new CatchRuleset().RulesetInfo));
            AddStep("User on mania ruleset", () => user.Value = new UserProfileData(new APIUser { Id = 3, PlayMode = "osu" }, new ManiaRuleset().RulesetInfo));

            AddStep("User with osu as default", () => user.Value = new UserProfileData(new APIUser { Id = 0, PlayMode = "osu" }, new OsuRuleset().RulesetInfo));
            AddStep("User with taiko as default", () => user.Value = new UserProfileData(new APIUser { Id = 1, PlayMode = "taiko" }, new OsuRuleset().RulesetInfo));
            AddStep("User with catch as default", () => user.Value = new UserProfileData(new APIUser { Id = 2, PlayMode = "fruits" }, new OsuRuleset().RulesetInfo));
            AddStep("User with mania as default", () => user.Value = new UserProfileData(new APIUser { Id = 3, PlayMode = "mania" }, new OsuRuleset().RulesetInfo));

            AddStep("null user", () => user.Value = null);
        }
    }
}
