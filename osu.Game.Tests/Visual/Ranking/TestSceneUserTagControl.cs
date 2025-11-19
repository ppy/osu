// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Screens.Ranking;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneUserTagControl : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private int writeRequestCount;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset mouse position", () => InputManager.MoveMouseTo(Vector2.Zero));

            AddStep("set up network requests", () =>
            {
                writeRequestCount = 0;
                dummyAPI.HandleRequest = request =>
                {
                    switch (request)
                    {
                        case ListTagsRequest listTagsRequest:
                        {
                            Scheduler.AddDelayed(() => listTagsRequest.TriggerSuccess(new APITagCollection
                            {
                                Tags =
                                [
                                    new APITag { Id = 0, Name = "uncategorised tag", Description = "This probably isn't real but could be and should be handled.", },
                                    new APITag { Id = 1, Name = "song representation/simple", Description = "Accessible and straightforward map design.", },
                                    new APITag
                                    {
                                        Id = 2, Name = "style/clean",
                                        Description = "Visually uncluttered and organised patterns, often involving few overlaps and equal visual spacing between objects.",
                                    },
                                    new APITag
                                    {
                                        Id = 3, Name = "aim/aim control", Description = "Patterns with velocity or direction changes which strongly go against a player's natural movement pattern.",
                                    },
                                    new APITag { Id = 4, Name = "tap/bursts", Description = "Patterns requiring continuous movement and alternating, typically 9 notes or less.", },
                                    new APITag { Id = 5, Name = "style/mono-heavy", Description = "Features monos used in large amounts.", RulesetId = 1, },
                                ]
                            }), 500);
                            return true;
                        }

                        case GetBeatmapSetRequest getBeatmapSetRequest:
                        {
                            var beatmapSet = CreateAPIBeatmapSet(Beatmap.Value.BeatmapInfo);
                            beatmapSet.Beatmaps.Single().TopTags =
                            [
                                new APIBeatmapTag { TagId = 3, VoteCount = 4 },
                                new APIBeatmapTag { TagId = 2, VoteCount = 3 },
                                new APIBeatmapTag { TagId = 0, VoteCount = 2 },
                            ];
                            Scheduler.AddDelayed(() => getBeatmapSetRequest.TriggerSuccess(beatmapSet), 500);
                            return true;
                        }

                        case AddBeatmapTagRequest:
                        case RemoveBeatmapTagRequest:
                        {
                            writeRequestCount++;
                            Scheduler.AddDelayed(request.TriggerSuccess, 500);
                            return true;
                        }
                    }

                    return false;
                };
            });
        }

        [Test]
        public void TestRulesetSupport()
        {
            AddStep("show for osu! beatmap", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.BeatmapInfo.OnlineID = 42;
                Beatmap.Value = working;
                recreateControl();
            });

            AddStep("show for taiko beatmap", () =>
            {
                var working = CreateWorkingBeatmap(new TaikoRuleset().RulesetInfo);
                working.BeatmapInfo.OnlineID = 44;
                Beatmap.Value = working;
                recreateControl();
            });
        }

        [Test]
        public void TestNotWritable()
        {
            AddStep("show", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.BeatmapInfo.OnlineID = 42;
                Beatmap.Value = working;
                recreateControl(writable: false);
            });

            AddUntilStep("click tag", () =>
            {
                var tag = this.ChildrenOfType<UserTagControl.DrawableUserTag>().FirstOrDefault(t => t.UserTag.Id == 2);
                if (tag == null)
                    return false;

                InputManager.MoveMouseTo(tag);
                InputManager.Click(MouseButton.Left);
                return true;
            });

            AddAssert("no vote requests send", () => writeRequestCount, () => Is.Zero);
        }

        [Test]
        public void TestTagsDoNotMoveUntilMouseMovesAway()
        {
            AddStep("show", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.BeatmapInfo.OnlineID = 42;
                Beatmap.Value = working;
                recreateControl();
            });
            AddUntilStep("wait for ready", () => getTagFlow().Count, () => Is.EqualTo(4));
            AddAssert("tag 2 is second", () => getTagFlow().GetLayoutPosition(getDrawableTagById(2)), () => Is.EqualTo(1));
            AddStep("vote for tag 2", () =>
            {
                InputManager.MoveMouseTo(getDrawableTagById(2));
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("tag 2 voted for", () => getDrawableTagById(2).UserTag.VoteCount.Value, () => Is.EqualTo(4));

            AddStep("remove vote for tag 2", () =>
            {
                InputManager.MoveMouseTo(getDrawableTagById(2));
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("tag 2 not voted for", () => getDrawableTagById(2).UserTag.VoteCount.Value, () => Is.EqualTo(3));
            AddAssert("tag 2 is still second", () => getTagFlow().GetLayoutPosition(getDrawableTagById(2)), () => Is.EqualTo(1));

            AddStep("vote for tag 2", () =>
            {
                InputManager.MoveMouseTo(getDrawableTagById(2));
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("tag 2 voted for", () => getDrawableTagById(2).UserTag.VoteCount.Value, () => Is.EqualTo(4));
            AddStep("move mouse away", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddAssert("tag 2 reordered to first", () => getTagFlow().GetLayoutPosition(getDrawableTagById(2)), () => Is.EqualTo(0));

            FillFlowContainer<UserTagControl.DrawableUserTag> getTagFlow() => this.ChildrenOfType<FillFlowContainer<UserTagControl.DrawableUserTag>>().Single();

            UserTagControl.DrawableUserTag getDrawableTagById(long id) => getTagFlow().Single(t => t.UserTag.Id == id);
        }

        private void recreateControl(bool writable = true)
        {
            Child = new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new UserTagControl(Beatmap.Value.BeatmapInfo)
                {
                    Writable = writable,
                    Width = 700,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }
    }
}
