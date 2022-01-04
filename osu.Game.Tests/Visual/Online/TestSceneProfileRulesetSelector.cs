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

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneProfileRulesetSelector : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        public TestSceneProfileRulesetSelector()
        {
            ProfileRulesetSelector selector;
            var user = new Bindable<APIUser>();

            Child = selector = new ProfileRulesetSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                User = { BindTarget = user }
            };

            AddStep("set osu! as default", () => selector.SetDefaultRuleset(new OsuRuleset().RulesetInfo));
            AddStep("set mania as default", () => selector.SetDefaultRuleset(new ManiaRuleset().RulesetInfo));
            AddStep("set taiko as default", () => selector.SetDefaultRuleset(new TaikoRuleset().RulesetInfo));
            AddStep("set catch as default", () => selector.SetDefaultRuleset(new CatchRuleset().RulesetInfo));

            AddStep("User with osu as default", () => user.Value = new APIUser { PlayMode = "osu" });
            AddStep("User with mania as default", () => user.Value = new APIUser { PlayMode = "mania" });
            AddStep("User with taiko as default", () => user.Value = new APIUser { PlayMode = "taiko" });
            AddStep("User with catch as default", () => user.Value = new APIUser { PlayMode = "fruits" });
            AddStep("null user", () => user.Value = null);
        }
    }
}
