// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.MapPool;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneMapPoolScreen : TournamentScreenTestScene
    {
        private MapPoolScreen screen = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("reset state", () =>
            {
                base.Content.Child = screen = new TestMapPoolScreen { Width = 0.95f };
                resetState();
            });
        }

        private void resetState()
        {
            Ladder.SplitMapPoolByMods.Value = true;

            Ladder.CurrentMatch.Value = new TournamentMatch();
            Ladder.CurrentMatch.Value = Ladder.Matches.First();
            Ladder.CurrentMatch.Value.PicksBans.Clear();
            Ladder.CurrentMatch.Value.Round.Value!.BanCount.Value = 2;
            Ladder.CurrentMatch.Value.Round.Value!.ProtectCount.Value = 0;
            Ladder.CurrentMatch.Value!.Round.Value!.AllowPickOpponentProtect.Value = true;
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
        });

        [Test]
        public void TestFewMaps()
        {
            loadMaps(8);

            assertTwoWide();
        }

        [Test]
        public void TestJustEnoughMaps()
        {
            loadMaps(18);

            assertTwoWide();
        }

        [Test]
        public void TestManyMaps()
        {
            loadMaps(19);

            assertThreeWide();
        }

        [Test]
        public void TestJustEnoughMods()
        {
            AddStep("load many maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 11; i++)
                    addBeatmap(i > 4 ? Ruleset.Value.CreateInstance().AllMods.ElementAt(i).Acronym : "NM");
            });

            AddStep("reset state", resetState);

            assertTwoWide();
        }

        private void assertTwoWide() =>
            AddAssert("ensure layout width is 2", () => screen.ChildrenOfType<FillFlowContainer<FillFlowContainer<TournamentBeatmapPanel>>>().First().Padding.Left > 0);

        private void assertThreeWide() =>
            AddAssert("ensure layout width is 3", () => screen.ChildrenOfType<FillFlowContainer<FillFlowContainer<TournamentBeatmapPanel>>>().First().Padding.Left == 0);

        [Test]
        public void TestManyMods()
        {
            AddStep("load many maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 12; i++)
                    addBeatmap(i > 4 ? Ruleset.Value.CreateInstance().AllMods.ElementAt(i).Acronym : "NM");
            });

            AddStep("reset state", resetState);

            assertThreeWide();
        }

        [Test]
        public void TestSplitMapPoolByMods()
        {
            AddStep("load many maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 12; i++)
                    addBeatmap(i > 4 ? Ruleset.Value.CreateInstance().AllMods.ElementAt(i).Acronym : "NM");
            });

            AddStep("disable splitting map pool by mods", () => Ladder.SplitMapPoolByMods.Value = false);

            AddStep("reset state", resetState);
        }

        [Test]
        public void TestBanOrderMultipleBans()
        {
            AddStep("set ban count", () => Ladder.CurrentMatch.Value!.Round.Value!.BanCount.Value = 2);

            loadMaps(5);

            AddStep("start bans from blue team", () => screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Ban").TriggerClick());

            AddStep("ban map", () => clickBeatmapPanel(0));
            checkTotalPickBans(1);
            checkLastPick(ChoiceType.Ban, TeamColour.Blue);

            AddStep("ban map", () => clickBeatmapPanel(1));
            checkTotalPickBans(2);
            checkLastPick(ChoiceType.Ban, TeamColour.Red);

            AddStep("ban map", () => clickBeatmapPanel(2));
            checkTotalPickBans(3);
            checkLastPick(ChoiceType.Ban, TeamColour.Red);

            AddStep("pick map", () => clickBeatmapPanel(3));
            checkTotalPickBans(4);
            checkLastPick(ChoiceType.Ban, TeamColour.Blue);

            AddStep("pick map", () => clickBeatmapPanel(4));
            checkTotalPickBans(5);
            checkLastPick(ChoiceType.Pick, TeamColour.Blue);
        }

        [Test]
        public void TestPickBanOrder()
        {
            AddStep("set ban count", () => Ladder.CurrentMatch.Value!.Round.Value!.BanCount.Value = 1);

            loadMaps(5);

            AddStep("start bans from blue team", () => screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Ban").TriggerClick());

            AddStep("ban map", () => clickBeatmapPanel(0));
            checkTotalPickBans(1);
            checkLastPick(ChoiceType.Ban, TeamColour.Blue);

            AddStep("ban map", () => clickBeatmapPanel(1));
            checkTotalPickBans(2);
            checkLastPick(ChoiceType.Ban, TeamColour.Red);

            AddStep("pick map", () => clickBeatmapPanel(2));
            checkTotalPickBans(3);
            checkLastPick(ChoiceType.Pick, TeamColour.Red);

            AddStep("pick map", () => clickBeatmapPanel(3));
            checkTotalPickBans(4);
            checkLastPick(ChoiceType.Pick, TeamColour.Blue);

            AddStep("pick map", () => clickBeatmapPanel(4));
            checkTotalPickBans(5);
            checkLastPick(ChoiceType.Pick, TeamColour.Red);

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
                Ladder.CurrentMatch.Value.PicksBans.Clear();
            });
        }

        [Test]
        public void TestMultipleTeamBans()
        {
            AddStep("set ban count", () => Ladder.CurrentMatch.Value!.Round.Value!.BanCount.Value = 3);

            loadMaps(12);

            AddStep("start bans with red team", () => screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Red Ban").TriggerClick());

            AddStep("first ban", () => clickBeatmapPanel(0));
            AddAssert("red ban registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban && pb.Team == TeamColour.Red),
                () => Is.EqualTo(1));

            AddStep("ban two more maps", () =>
            {
                clickBeatmapPanel(1);
                clickBeatmapPanel(2);
            });

            AddAssert("three bans registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban),
                () => Is.EqualTo(3));
            AddAssert("both new bans for blue team",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban && pb.Team == TeamColour.Blue),
                () => Is.EqualTo(2));

            AddStep("ban two more maps", () =>
            {
                clickBeatmapPanel(3);
                clickBeatmapPanel(4);
            });

            AddAssert("five bans registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban),
                () => Is.EqualTo(5));
            AddAssert("both new bans for red team",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban && pb.Team == TeamColour.Red),
                () => Is.EqualTo(3));

            AddStep("ban last map", () => clickBeatmapPanel(5));
            AddAssert("six bans registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban),
                () => Is.EqualTo(6));
            AddAssert("red banned three",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban && pb.Team == TeamColour.Red),
                () => Is.EqualTo(3));
            AddAssert("blue banned three",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban && pb.Team == TeamColour.Blue),
                () => Is.EqualTo(3));

            AddStep("pick map", () => clickBeatmapPanel(6));
            AddAssert("one pick registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick),
                () => Is.EqualTo(1));
            AddAssert("pick was blue's",
                () => Ladder.CurrentMatch.Value!.PicksBans.Last().Team,
                () => Is.EqualTo(TeamColour.Blue));

            AddStep("pick map", () => clickBeatmapPanel(7));
            AddAssert("two picks registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick),
                () => Is.EqualTo(2));
            AddAssert("pick was red's",
                () => Ladder.CurrentMatch.Value!.PicksBans.Last().Team,
                () => Is.EqualTo(TeamColour.Red));

            AddStep("pick map", () => clickBeatmapPanel(8));
            AddAssert("three picks registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick),
                () => Is.EqualTo(3));
            AddAssert("pick was blue's",
                () => Ladder.CurrentMatch.Value!.PicksBans.Last().Team,
                () => Is.EqualTo(TeamColour.Blue));

            AddStep("reset match", () =>
            {
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
                Ladder.CurrentMatch.Value.PicksBans.Clear();
            });
        }

        [Test]
        public void TestProtectBanPickOrder()
        {
            AddStep("set protect count = 2", () => Ladder.CurrentMatch.Value!.Round.Value!.ProtectCount.Value = 2);
            AddStep("set ban count = 1", () => Ladder.CurrentMatch.Value!.Round.Value!.BanCount.Value = 1);

            loadMaps(12);

            AddStep("start blue team protect", () => screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Protect").TriggerClick());

            AddStep("click first map", () => clickBeatmapPanel(0));

            AddAssert("protect registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Protect),
                () => Is.EqualTo(1));

            AddStep("click 3 more maps", () =>
            {
                clickBeatmapPanel(1);
                clickBeatmapPanel(2);
                clickBeatmapPanel(3);
            });

            AddAssert("four protects registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Protect),
                () => Is.EqualTo(4));

            AddStep("click 2 more maps", () =>
            {
                clickBeatmapPanel(4);
                clickBeatmapPanel(5);
            });

            AddAssert("two bans registered",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Ban),
                () => Is.EqualTo(2));
        }

        [Test]
        public void TestDisallowPickOpponentProtect()
        {
            AddStep("set protect count to 1", () => Ladder.CurrentMatch.Value!.Round.Value!.ProtectCount.Value = 1);
            AddStep("opponent protect pick = false", () => Ladder.CurrentMatch.Value!.Round.Value!.AllowPickOpponentProtect.Value = false);
            loadMaps(5);

            AddStep("add protects", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Protect").TriggerClick();
                clickBeatmapPanel(0);
                clickBeatmapPanel(1);
            });

            AddStep("red picks blue protect", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Red Pick").TriggerClick();
                clickBeatmapPanel(0);
            });
            AddAssert("blue protect was not picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick && pb.Team == TeamColour.Red),
                () => Is.EqualTo(0));

            AddStep("blue picks red protect", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Pick").TriggerClick();
                clickBeatmapPanel(1);
            });

            AddAssert("red protect was not picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick && pb.Team == TeamColour.Red),
                () => Is.EqualTo(0));

            AddStep("blue picks blue protect", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Pick").TriggerClick();
                clickBeatmapPanel(0);
            });
            AddAssert("blue protect was picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick && pb.Team == TeamColour.Blue),
                () => Is.EqualTo(1));

            AddStep("red picks red protect", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Red Pick").TriggerClick();
                clickBeatmapPanel(1);
            });
            AddAssert("red protect was picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick && pb.Team == TeamColour.Red),
                () => Is.EqualTo(1));
        }

        [Test]
        public void TestAllowPickOpponentProtect()
        {
            AddStep("set protect count to 1", () => Ladder.CurrentMatch.Value!.Round.Value!.ProtectCount.Value = 2);
            AddStep("opponent protect pick = true", () => Ladder.CurrentMatch.Value!.Round.Value!.AllowPickOpponentProtect.Value = true);
            loadMaps(5);

            AddStep("add protects", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Protect").TriggerClick();
                clickBeatmapPanel(0);
                clickBeatmapPanel(1);
                clickBeatmapPanel(2);
                clickBeatmapPanel(3);
            });

            AddStep("red picks blue protect", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Red Pick").TriggerClick();
                clickBeatmapPanel(0);
            });
            AddAssert("blue protect was picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick && pb.Team == TeamColour.Red),
                () => Is.EqualTo(1));

            AddStep("blue picks red protect", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Pick").TriggerClick();
                clickBeatmapPanel(1);
            });

            AddAssert("red protect was picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick && pb.Team == TeamColour.Red),
                () => Is.EqualTo(1));

            AddStep("blue picks blue protect", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Blue Pick").TriggerClick();
                clickBeatmapPanel(2);
            });
            AddAssert("blue protect was picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick && pb.Team == TeamColour.Blue),
                () => Is.EqualTo(2));

            AddStep("red picks red protect", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Red Pick").TriggerClick();
                clickBeatmapPanel(3);
            });
            AddAssert("red protect was picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick && pb.Team == TeamColour.Red),
                () => Is.EqualTo(2));
        }

        [Test]
        public void TestRemoveProtect()
        {
            loadMaps(1);

            AddStep("protect a map", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Red Protect").TriggerClick();
                clickBeatmapPanel(0);
            });

            AddAssert("map was protected",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Protect),
                () => Is.EqualTo(1));

            AddStep("pick a map", () =>
            {
                screen.ChildrenOfType<TourneyButton>().First(btn => btn.Text == "Red Pick").TriggerClick();
                clickBeatmapPanel(0);
            });

            AddAssert("map was picked",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick),
                () => Is.EqualTo(1));

            AddStep("remove pick", () => clickBeatmapPanel(0, MouseButton.Right));

            AddAssert("pick was removed",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Pick),
                () => Is.EqualTo(0));
            AddAssert("protect remains",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Protect),
                () => Is.EqualTo(1));

            AddStep("remove protect", () => clickBeatmapPanel(0, MouseButton.Right));

            AddAssert("protect was removed",
                () => Ladder.CurrentMatch.Value!.PicksBans.Count(pb => pb.Type == ChoiceType.Protect),
                () => Is.EqualTo(0));
        }

        private void checkTotalPickBans(int expected) => AddAssert($"total pickbans is {expected}", () => Ladder.CurrentMatch.Value!.PicksBans, () => Has.Count.EqualTo(expected));

        private void checkLastPick(ChoiceType expectedChoice, TeamColour expectedColour) =>
            AddAssert($"last choice was {expectedChoice} by {expectedColour}",
                () => Ladder.CurrentMatch.Value!.PicksBans.Select(pb => (pb.Type, pb.Team)).Last(),
                () => Is.EqualTo((expectedChoice, expectedColour)));

        private void addBeatmap(string mods = "NM")
        {
            Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Add(new RoundBeatmap
            {
                Beatmap = CreateSampleBeatmap(),
                Mods = mods
            });
        }

        private void clickBeatmapPanel(int index, MouseButton button = MouseButton.Left)
        {
            InputManager.MoveMouseTo(screen.ChildrenOfType<TournamentBeatmapPanel>().ElementAt(index));
            InputManager.Click(button);
        }

        private void loadMaps(int count)
        {
            AddStep($"load {count} map(s)", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < count; i++)
                    addBeatmap();

                // to force mappool screen display update
                Ladder.CurrentMatch.Value = new TournamentMatch();
                Ladder.CurrentMatch.Value = Ladder.Matches.First();
            });
        }

        private partial class TestMapPoolScreen : MapPoolScreen
        {
            // this is a bit of a test-specific workaround.
            // the way pick/ban is implemented is a bit funky; the screen itself is what handles the mouse there,
            // rather than the beatmap panels themselves.
            // in some extreme situations headless it may turn out that the panels overflow the screen,
            // and as such picking stops working anymore outside of the bounds of the screen drawable.
            // this override makes it so the screen sees all of the input at all times, making that impossible to happen.
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
        }
    }
}
