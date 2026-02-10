// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneUpdateableTeamFlag : OsuTestScene
    {
        [Test]
        public void TestHideOnNull()
        {
            UpdateableTeamFlag flag = null!;

            AddStep("create flag with team", () => Child = flag = new UpdateableTeamFlag(createTeam(), hideOnNull: true) { Width = 300, Height = 150 });
            AddAssert("flag is present", () => flag.IsPresent, () => Is.True);
            AddStep("set team to null", () => flag.Team = null);
            AddAssert("flag is not present", () => flag.IsPresent, () => Is.False);
        }

        [Test]
        public void DontHideOnNull()
        {
            UpdateableTeamFlag flag = null!;

            AddStep("create flag with team", () => Child = flag = new UpdateableTeamFlag(createTeam(), hideOnNull: false) { Width = 300, Height = 150 });
            AddAssert("flag is present", () => flag.IsPresent, () => Is.True);
            AddStep("set team to null", () => flag.Team = null);
            AddAssert("flag is present", () => flag.IsPresent, () => Is.True);
        }

        private static APITeam createTeam() => new APITeam
        {
            Id = 2,
            Name = "mom?",
            ShortName = "MOM",
            FlagUrl = @"https://assets.ppy.sh/teams/flag/1/b46fb10dbfd8a35dc50e6c00296c0dc6172dffc3ed3d3a4b379277ba498399fe.png",
        };
    }
}
