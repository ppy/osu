// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneTeamProfileOverlay : OsuTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private TeamProfileOverlay overlay = null!;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create team overlay", () =>
            {
                overlay = new TeamProfileOverlay();

                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[] { (typeof(TeamProfileOverlay), overlay) },
                    Child = overlay,
                };
            });
        }

        [Test]
        public void TestBlank()
        {
            AddStep("show overlay", () => overlay.Show());
        }

        [Test]
        public void TestActualTeam()
        {
            AddStep("setup request handling", () =>
            {
                dummyAPI.HandleRequest = req =>
                {
                    if (req is GetTeamRequest getTeamRequest)
                    {
                        getTeamRequest.TriggerSuccess(TEST_TEAM);
                        return true;
                    }

                    return false;
                };
            });
            AddStep("show team", () => overlay.ShowTeam(new APITeam { Id = 1 }));
        }

        [Test]
        public void TestLogin()
        {
            GetTeamRequest pendingRequest = null!;

            AddStep("setup request handling", () =>
            {
                dummyAPI.HandleRequest = req =>
                {
                    if (dummyAPI.State.Value == APIState.Online && req is GetTeamRequest getTeamRequest)
                    {
                        pendingRequest = getTeamRequest;
                        return true;
                    }

                    return false;
                };
            });
            AddStep("logout", () => dummyAPI.Logout());
            AddStep("show team", () => overlay.ShowTeam(new APITeam { Id = 1 }));
            AddStep("login", () =>
            {
                dummyAPI.Login("username", "password");
                dummyAPI.AuthenticateSecondFactor("12345678");
            });
            AddWaitStep("wait some", 3);
            AddStep("complete request", () => pendingRequest.TriggerSuccess(TEST_TEAM));
        }

        public static readonly APITeam TEST_TEAM = new APITeam
        {
            Name = "Test Team",
            Id = 1,
            ShortName = "TEST",
            CoverUrl = TestResources.COVER_IMAGE_1,
            FlagUrl = TestResources.COVER_IMAGE_2,
            DefaultRuleset = @"fruits",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 13, 6, 0, TimeSpan.Zero),
            Description = @"cool team yeah",
            MembersCount = 1,
            EmptySlots = 8,
            Leader = new APIUser
            {
                Id = 2,
                Username = "peppy",
                CoverUrl = TestResources.COVER_IMAGE_3,
            },
            Statistics = new APITeamStatistics
            {
                Rank = 2,
                Performance = 7923,
                PlayCount = 95342,
                RankedScore = 546346745,
            },
        };
    }
}
