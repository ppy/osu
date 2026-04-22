// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneSongPreview : RankedPlayTestScene
    {
        private readonly BeatmapRequestHandler requestHandler = new BeatmapRequestHandler();

        private PlayerHandOfCards handOfCards = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup request handler", () => ((DummyAPIAccess)API).HandleRequest = requestHandler.HandleRequest);

            AddStep("add cards", () =>
            {
                Child = handOfCards = new PlayerHandOfCards
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(0.5f),
                };

                foreach (var beatmap in requestHandler.Beatmaps.Take(3))
                    handOfCards.AddCard(new RevealedRankedPlayCardWithPlaylistItem(beatmap));
            });

            AddUntilStep("load tracks", () => this.ChildrenOfType<RankedPlayCard>().All(card => card.SongPreview.TrackLoaded));
        }

        [Test]
        public void TestSongPreview()
        {
            AddStep("move mouse to first card", () => InputManager.MoveMouseTo(getCard(0)));

            AddAssert("first track running", () => getCard(0).SongPreview.IsRunning);
            AddAssert("only one track running", () => this.ChildrenOfType<RankedPlayCard>().Count(c => c.SongPreview.IsRunning) == 1);

            AddStep("move mouse to second card", () => InputManager.MoveMouseTo(getCard(1)));
            AddAssert("second track running", () => getCard(1).SongPreview.IsRunning);
            AddAssert("only one track running", () => this.ChildrenOfType<RankedPlayCard>().Count(c => c.SongPreview.IsRunning) == 1);

            AddStep("disable preview", () => handOfCards.CurrentPlayingPreview.Value = null);
            AddAssert("no tracks running", () => !this.ChildrenOfType<RankedPlayCard>().Any(c => c.SongPreview.IsRunning));

            AddStep("move mouse to third card", () => InputManager.MoveMouseTo(getCard(2)));
            AddAssert("third track running", () => getCard(2).SongPreview.IsRunning);

            AddStep("move mouse away", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddAssert("third track running", () => getCard(2).SongPreview.IsRunning);

            AddStep("move mouse to second card", () => InputManager.MoveMouseTo(getCard(1)));
            AddAssert("second track running", () => getCard(1).SongPreview.IsRunning);
            AddAssert("only one track running", () => this.ChildrenOfType<RankedPlayCard>().Count(c => c.SongPreview.IsRunning) == 1);
        }

        private RankedPlayCard getCard(int index) => this.ChildrenOfType<RankedPlayCard>().ElementAt(index);
    }
}
