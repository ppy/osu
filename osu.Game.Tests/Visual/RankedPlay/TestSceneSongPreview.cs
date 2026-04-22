// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
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
        private readonly Bindable<bool> previewEnabled = new BindableBool(true);

        private readonly BeatmapRequestHandler requestHandler = new BeatmapRequestHandler();

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup request handler", () => ((DummyAPIAccess)API).HandleRequest = requestHandler.HandleRequest);

            AddStep("add cards", () =>
            {
                PlayerHandOfCards handOfCards;

                Child = handOfCards = new PlayerHandOfCards
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(0.5f),
                };

                foreach (var beatmap in requestHandler.Beatmaps.Take(3))
                {
                    handOfCards.AddCard(new RevealedRankedPlayCardWithPlaylistItem(beatmap), handCard =>
                    {
                        handCard.Card.SongPreviewEnabled.BindTarget = previewEnabled;
                    });
                }
            });

            AddUntilStep("load tracks", () => this.ChildrenOfType<RankedPlayCard>().All(card => card.PreviewTrackLoaded));
        }

        [Test]
        public void TestSongPreview()
        {
            AddStep("move mouse to first card", () => InputManager.MoveMouseTo(getCard(0)));

            AddAssert("first track running", () => getCard(0).PreviewTrackRunning);
            AddAssert("only one track running", () => this.ChildrenOfType<RankedPlayCard>().Count(c => c.PreviewTrackRunning) == 1);

            AddStep("move mouse to second card", () => InputManager.MoveMouseTo(getCard(1)));

            AddAssert("second track running", () => getCard(1).PreviewTrackRunning);
            AddAssert("only one track running", () => this.ChildrenOfType<RankedPlayCard>().Count(c => c.PreviewTrackRunning) == 1);

            AddStep("disable preview", () => previewEnabled.Value = false);

            AddAssert("no tracks running", () => !this.ChildrenOfType<RankedPlayCard>().Any(c => c.PreviewTrackRunning));

            AddStep("move mouse to third card", () => InputManager.MoveMouseTo(getCard(2)));

            AddAssert("no tracks running", () => !this.ChildrenOfType<RankedPlayCard>().Any(c => c.PreviewTrackRunning));

            AddStep("enable preview", () => previewEnabled.Value = true);

            AddAssert("third track running", () => getCard(2).PreviewTrackRunning);
        }

        private RankedPlayCard getCard(int index) => this.ChildrenOfType<RankedPlayCard>().ElementAt(index);
    }
}
