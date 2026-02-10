// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Team;
using osu.Game.Overlays.Team.Sections;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneTeamProfileMembersSection : OsuTestScene
    {
        private const int per_page = 51;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private MembersSection section = null!;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create section", () =>
            {
                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[]
                    {
                        (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Pink))
                    },
                    Child = new OsuScrollContainer(Direction.Vertical)
                    {
                        Width = 890,
                        RelativeSizeAxes = Axes.Y,
                        Padding = new MarginPadding(20),
                        Child = section = new MembersSection(),
                    },
                };
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("setup request handling", () =>
            {
                const int member_count = 80;
                IEnumerable<APITeamMember> members = from i in Enumerable.Range(1, member_count) select GenerateMember(i);

                dummyAPI.HandleRequest = req =>
                {
                    if (req is GetTeamMembersRequest getTeamMembersRequest)
                    {
                        getTeamMembersRequest.TriggerSuccess(new TeamMembersResponse
                        {
                            Items = members.Take(per_page).ToArray(),
                            Total = member_count,
                        });

                        members = members.Skip(per_page);

                        return true;
                    }

                    return false;
                };
            });
            AddStep("show", () => section.TeamData.Value = GenerateTeam());
        }

        [Test]
        public void TestShowMore()
        {
            AddStep("setup request handling", () =>
            {
                const int member_count = 180;
                IEnumerable<APITeamMember> members = from i in Enumerable.Range(1, member_count) select GenerateMember(i);

                dummyAPI.HandleRequest = req =>
                {
                    if (req is GetTeamMembersRequest getTeamMembersRequest)
                    {
                        getTeamMembersRequest.TriggerSuccess(new TeamMembersResponse
                        {
                            Items = members.Take(per_page).ToArray(),
                            Total = member_count,
                        });

                        members = members.Skip(per_page);

                        return true;
                    }

                    return false;
                };
            });
            AddStep("show", () => section.TeamData.Value = GenerateTeam());
            AddRepeatStep("click show more", () => this.ChildrenOfType<ShowMoreButton>().First().TriggerClick(), 3);
            AddUntilStep("wait for button to hide", () => this.ChildrenOfType<ShowMoreButton>().First().IsPresent, () => Is.False);
        }

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

        public static TeamProfileData GenerateTeam() => new TeamProfileData(new APITeam
        {
            Leader = new APIUser
            {
                Id = 2,
                Username = "peppy",
                CoverUrl = TestResources.COVER_IMAGE_3,
            },
        }, new OsuRuleset().RulesetInfo);
    }
}
