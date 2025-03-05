// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Tournaments;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osu.Game.Screens.OnlinePlay.Tournaments.Tabs;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Tournaments
{
    public partial class TestSceneTournamentsRoomSubScreen : OnlinePlayTestScene
    {
        private TournamentsRoomSubScreen screen = null!;
        private TournamentInfo tournamentInfo = null!;
        // private BeatmapManager beatmaps = null!;
        // private BeatmapSetInfo importedSet = null!;

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("load tournament", () =>
            {
                SelectedRoom.Value = new Room { Name = "Test Room" };
                tournamentInfo = createTournamentInfo1();
                LoadScreen(screen = new TestTournamentsRoomSubScreen(SelectedRoom.Value!) { TournamentInfo = tournamentInfo });
            });

            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestCurrentTabSwitching()
        {
            AddStep("Show all tabs", () => tournamentInfo.VisibleTabs.Value = TournamentsTabs.All);

            AddStep("Select settings tab", () =>
            {
                var temp = ((FillFlowContainer)screen.ChildrenOfType<TournamentsRoomFooter>().First().Child).Children.Last();
                InputManager.MoveMouseTo(temp);
                InputManager.Click(MouseButton.Left);
                InputManager.MoveMouseTo(temp, new Vector2(0, -60));
            });
            AddAssert("Settings tab is Current", () => tournamentInfo.CurrentTabType.Value == TournamentsTab.Settings);

            AddStep("Hide settings tab", () => tournamentInfo.SetTabVisibility(TournamentsTab.Settings, false));
            AddAssert("Schedule tab is Current", () => tournamentInfo.CurrentTabType.Value == TournamentsTab.Schedule);

            AddStep("Select players tab", () =>
            {
                var temp = ((FillFlowContainer)screen.ChildrenOfType<TournamentsRoomFooter>().First().Child).Children.Last((d) =>
                    ((TournamentsRoomFooterButton)d).TabType == TournamentsTab.Players);
                InputManager.MoveMouseTo(temp);
                InputManager.Click(MouseButton.Left);
                InputManager.MoveMouseTo(temp, new Vector2(0, -60));
            });
            AddAssert("Players tab is Current", () => tournamentInfo.CurrentTabType.Value == TournamentsTab.Players);

            AddStep("Hide players tab", () => tournamentInfo.SetTabVisibility(TournamentsTab.Players, false));
            AddAssert("Qualifiers tab is Current", () => tournamentInfo.CurrentTabType.Value == TournamentsTab.Qualifiers);

            AddStep("Hide all but info tab", () => tournamentInfo.VisibleTabs.Value = TournamentsTabs.Info);
            AddAssert("Info tab is Current", () => tournamentInfo.CurrentTabType.Value == TournamentsTab.Info);
        }

        private TournamentInfo createTournamentInfo1()
        {

            List<TournamentUser> players = [];
            foreach (int id in Enumerable.Range(1, 8))
            {
                players.Add(new TournamentUser() { OnlineID = id });
                // PopulatePlayer(players.Last(), success: () => Console.WriteLine("Successfully populated player."), immediate: true);
            }

            BindableList<TournamentTeam> teams = [];
            foreach (int i in Enumerable.Range(0, 4))
            {
                teams.Add(new TournamentTeam(players.GetRange(i, 2))
                {
                    FullName = { Value = "Team" + i.ToString() },
                    FlagName = { Value = "NO" },
                    Acronym = { Value = "T" + i.ToString() },
                    ID = i,
                });
            }

            BindableList<TournamentMatch> matches = [];
            foreach (int i in Enumerable.Range(0, 3))
            {
                matches.Add(new TournamentMatch([teams[i], teams[i + 1]]));
                matches.Last().Position.Value = new Point(i * 240, 0);
            }

            return new TournamentInfo()
            {
                IsEditing = { Value = true },
                VisibleTabs = { Value = TournamentsTabs.Info | TournamentsTabs.Players | TournamentsTabs.Results },

                Matches = matches,
                Teams = teams,
            };
        }


        private partial class TestTournamentsRoomSubScreen : TournamentsRoomSubScreen
        {
            [Resolved(canBeNull: true)]
            private IDialogOverlay? dialogOverlay { get; set; }

            public TestTournamentsRoomSubScreen(Room room)
                : base(room)
            {
            }

            public override bool OnExiting(ScreenExitEvent e)
            {
                // For testing purposes allow the screen to exit without confirming on second attempt.
                if (!ExitConfirmed && dialogOverlay?.CurrentDialog is ConfirmDiscardChangesDialog confirmDialog)
                {
                    confirmDialog.PerformAction<PopupDialogDangerousButton>();
                    return true;
                }

                return base.OnExiting(e);
            }
        }
    }
}
