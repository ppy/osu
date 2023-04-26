// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Sections;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneHistoricalSection : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        public TestSceneHistoricalSection()
        {
            HistoricalSection section;

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.2f)
            });

            Add(new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = section = new HistoricalSection(),
            });

            AddStep("Show peppy", () => section.User.Value = new UserProfileData(new APIUser { Id = 2 }, new OsuRuleset().RulesetInfo));
            AddStep("Show WubWoofWolf", () => section.User.Value = new UserProfileData(new APIUser { Id = 39828 }, new OsuRuleset().RulesetInfo));
        }
    }
}
