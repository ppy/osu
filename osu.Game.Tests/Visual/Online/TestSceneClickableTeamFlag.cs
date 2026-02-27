// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneClickableTeamFlag : OsuManualInputManagerTestScene
    {
        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create flags", () =>
            {
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(10f),
                    Children = new[]
                    {
                        new ClickableTeamFlag(
                            new APITeam
                            {
                                Id = 1,
                                Name = "Collective Wangs",
                                ShortName = "WANG",
                            }, showTooltipOnHover: false) { Width = 300, Height = 150 },
                        new ClickableTeamFlag(
                            new APITeam
                            {
                                Id = 2,
                                Name = "mom?",
                                ShortName = "MOM",
                                FlagUrl = "https://assets.ppy.sh/teams/flag/1/b46fb10dbfd8a35dc50e6c00296c0dc6172dffc3ed3d3a4b379277ba498399fe.png",
                            }, showTooltipOnHover: true) { Width = 300, Height = 150 },
                    },
                };
            });
        }

        [Test]
        public void TestHover()
        {
            AddStep("hover flag with no tooltip", () => InputManager.MoveMouseTo(this.ChildrenOfType<ClickableTeamFlag>().ElementAt(0)));
            AddWaitStep("wait", 3);
            AddAssert("tooltip is not visible", () => this.ChildrenOfType<OsuTooltipContainer.OsuTooltip>().FirstOrDefault()?.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddStep("hover flag with tooltip", () => InputManager.MoveMouseTo(this.ChildrenOfType<ClickableTeamFlag>().ElementAt(1)));
            AddUntilStep("wait for tooltip to show", () => this.ChildrenOfType<OsuTooltipContainer.OsuTooltip>().FirstOrDefault()?.State.Value, () => Is.EqualTo(Visibility.Visible));
        }
    }
}
