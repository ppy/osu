// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Intro;
using osu.Game.Tests.Visual.Matchmaking;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneIntroScreen : MatchmakingTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            IntroScreen introScreen = null!;

            AddStep("Add screen", () => Child = introScreen = new IntroScreen());

            AddStep("play animation", () => introScreen.PlayIntroSequence(
                new UserWithRating(new APIUser
                {
                    Id = 2,
                    Username = "User 1",
                    CoverUrl = "https://assets.ppy.sh/user-profile-covers/13845312/53e4eda7ad3ce41f0990c041179d8ab5d553fef988835f346a8d8da0482506ec.png"
                }, 1234),
                new UserWithRating(new APIUser
                {
                    Id = 3,
                    Username = "User 2",
                    CoverUrl = "https://assets.ppy.sh/user-profile-covers/14102976/10144df2f1c6fb2101726e0f89087a6061bc75755d88e59a9faf2c84034f2c71.jpeg"
                }, 1234),
                6.3f
            ));
        }
    }
}
