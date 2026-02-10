// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Placeholders;
using osu.Game.Overlays;
using osu.Game.Overlays.Team.Header.Components;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
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
        public void TestBasic()
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

                    if (req is GetTeamMembersRequest getTeamMembersRequest)
                    {
                        getTeamMembersRequest.TriggerSuccess(new TeamMembersResponse
                        {
                            Items = (from i in Enumerable.Range(1, 10) select GenerateMember(i)).ToArray(),
                            Total = 10,
                        });
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

                    if (req is GetTeamMembersRequest getTeamMembersRequest)
                    {
                        getTeamMembersRequest.TriggerSuccess(new TeamMembersResponse
                        {
                            Items = (from i in Enumerable.Range(1, 10) select GenerateMember(i)).ToArray(),
                            Total = 10,
                        });
                    }

                    return false;
                };
            });
            AddStep("logout", () => dummyAPI.Logout());
            AddStep("show team", () => overlay.ShowTeam(new APITeam { Id = 1 }));
            AddUntilStep("login prompt is present", () => this.ChildrenOfType<LoginPlaceholder>().First().IsPresent, () => Is.True);
            AddStep("login", () =>
            {
                dummyAPI.Login("username", "password");
                dummyAPI.AuthenticateSecondFactor("12345678");
            });
            AddUntilStep("loading layer is present", () => this.ChildrenOfType<LoadingLayer>().Any(l => l.IsPresent));
            AddWaitStep("wait some", 3);
            AddStep("complete request", () => pendingRequest.TriggerSuccess(TEST_TEAM));
            AddUntilStep("loading layer is not present", () => this.ChildrenOfType<LoadingLayer>().All(l => !l.IsPresent));
        }

        [Test]
        public void TestRulesets()
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

                    if (req is GetTeamMembersRequest getTeamMembersRequest)
                    {
                        getTeamMembersRequest.TriggerSuccess(new TeamMembersResponse
                        {
                            Items = (from i in Enumerable.Range(1, 10) select GenerateMember(i)).ToArray(),
                            Total = 10,
                        });
                    }

                    return false;
                };
            });
            AddStep("osu", () => overlay.ShowTeam(new APITeam { Id = 1 }, new OsuRuleset().RulesetInfo));
            AddWaitStep("wait some", 3);
            AddStep("complete request", () => pendingRequest.TriggerSuccess(TEST_TEAM));
            AddAssert("osu is selected", () => this.ChildrenOfType<TeamRulesetSelector>().First().Current.Value, () => Is.EqualTo(new OsuRuleset().RulesetInfo));
            AddStep("taiko", () => overlay.ShowTeam(new APITeam { Id = 1 }, new TaikoRuleset().RulesetInfo));
            AddWaitStep("wait some", 3);
            AddStep("complete request", () => pendingRequest.TriggerSuccess(TEST_TEAM));
            AddAssert("taiko is selected", () => this.ChildrenOfType<TeamRulesetSelector>().First().Current.Value, () => Is.EqualTo(new TaikoRuleset().RulesetInfo));
            AddStep("catch", () => overlay.ShowTeam(new APITeam { Id = 1 }, new CatchRuleset().RulesetInfo));
            AddWaitStep("wait some", 3);
            AddStep("complete request", () => pendingRequest.TriggerSuccess(TEST_TEAM));
            AddAssert("catch is selected", () => this.ChildrenOfType<TeamRulesetSelector>().First().Current.Value, () => Is.EqualTo(new CatchRuleset().RulesetInfo));
            AddStep("mania", () => overlay.ShowTeam(new APITeam { Id = 1 }, new ManiaRuleset().RulesetInfo));
            AddWaitStep("wait some", 3);
            AddStep("complete request", () => pendingRequest.TriggerSuccess(TEST_TEAM));
            AddAssert("mania is selected", () => this.ChildrenOfType<TeamRulesetSelector>().First().Current.Value, () => Is.EqualTo(new ManiaRuleset().RulesetInfo));
        }

        public static readonly APITeam TEST_TEAM = new APITeam
        {
            Name = "mom?",
            Id = 1,
            ShortName = "MOM",
            CoverUrl = TestResources.COVER_IMAGE_1,
            FlagUrl = "https://assets.ppy.sh/teams/flag/1/b46fb10dbfd8a35dc50e6c00296c0dc6172dffc3ed3d3a4b379277ba498399fe.png",
            DefaultRulesetId = new OsuRuleset().RulesetInfo.OnlineID,
            CreatedAt = new DateTimeOffset(2026, 1, 1, 13, 6, 0, TimeSpan.Zero),
            Description = @"cool team yeah",
            IsOpen = true,
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

        public static APITeamMember GenerateMember(int id) => new APITeamMember
        {
            CreatedAt = "2026-01-22T019:05:15+00:00",
            User = GenerateUser(id),
        };

        public static readonly string[] COVERS =
        {
            TestResources.COVER_IMAGE_1,
            TestResources.COVER_IMAGE_2,
            TestResources.COVER_IMAGE_3,
            TestResources.COVER_IMAGE_4,
        };

        public static APIUser GenerateUser(int id)
        {
            return new APIUser
            {
                Id = id,
                Username = $"user{id}",
                CoverUrl = COVERS[RNG.Next(0, COVERS.Length - 1)],
            };
        }
    }
}
