// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public partial class TestSceneBeatmapCardFavouriteButton : OsuManualInputManagerTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Test]
        public void TestInitialState([Values] bool favourited)
        {
            APIBeatmapSet beatmapSetInfo = null;
            FavouriteButton button = null;

            AddStep("create beatmap set", () =>
            {
                beatmapSetInfo = CreateAPIBeatmapSet(Ruleset.Value);
                beatmapSetInfo.HasFavourited = favourited;
            });
            AddStep("create button", () => Child = button = new FavouriteButton(beatmapSetInfo)
            {
                Size = new Vector2(25f, 50f),
                Scale = new Vector2(2f),
            });

            assertCorrectIcon(favourited);
            AddAssert("correct tooltip text", () => button.TooltipText == (favourited ? BeatmapsetsStrings.ShowDetailsUnfavourite : BeatmapsetsStrings.ShowDetailsFavourite));
        }

        [Test]
        public void TestRequestHandling()
        {
            APIBeatmapSet beatmapSetInfo = null;
            FavouriteButton button = null;
            BeatmapFavouriteAction? lastRequestAction = null;

            AddStep("create beatmap set", () => beatmapSetInfo = CreateAPIBeatmapSet(Ruleset.Value));
            AddStep("create button", () => Child = button = new FavouriteButton(beatmapSetInfo)
            {
                Size = new Vector2(25f, 50f),
                Scale = new Vector2(2f),
            });

            assertCorrectIcon(false);

            AddStep("register request handling", () => dummyAPI.HandleRequest = request =>
            {
                if (!(request is PostBeatmapFavouriteRequest favouriteRequest))
                    return false;

                lastRequestAction = favouriteRequest.Action;
                request.TriggerSuccess();
                return true;
            });

            AddStep("click icon", () =>
            {
                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("favourite request sent", () => lastRequestAction == BeatmapFavouriteAction.Favourite);
            assertCorrectIcon(true);

            AddStep("click icon", () =>
            {
                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("unfavourite request sent", () => lastRequestAction == BeatmapFavouriteAction.UnFavourite);
            assertCorrectIcon(false);
        }

        private void assertCorrectIcon(bool favourited) => AddAssert("icon correct",
            () => this.ChildrenOfType<SpriteIcon>().Single().Icon.Equals(favourited ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart));
    }
}
