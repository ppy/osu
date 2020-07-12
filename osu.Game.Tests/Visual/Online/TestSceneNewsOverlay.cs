// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneNewsOverlay : OsuTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private NewsOverlay news;

        [SetUp]
        public void SetUp() => Schedule(() => Child = news = new NewsOverlay());

        [Test]
        public void TestRequest()
        {
            setUpNewsResponse(responseExample);
            AddStep("Show", () => news.Show());
            AddStep("Show article", () => news.ShowArticle("article"));
        }

        private void setUpNewsResponse(GetNewsResponse r)
            => AddStep("set up response", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    if (!(request is GetNewsRequest getNewsRequest))
                        return;

                    getNewsRequest.TriggerSuccess(r);
                };
            });

        private GetNewsResponse responseExample => new GetNewsResponse
        {
            NewsPosts = new[]
            {
                new APINewsPost
                {
                    Title = "This post has an image which starts with \"/\" and has many authors!",
                    Preview = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                    Author = "someone, someone1, someone2, someone3, someone4",
                    FirstImage = "/help/wiki/shared/news/banners/monthly-beatmapping-contest.png",
                    PublishedAt = DateTimeOffset.Now
                },
                new APINewsPost
                {
                    Title = "This post has a full-url image! (HTML entity: &amp;)",
                    Preview = "boom (HTML entity: &amp;)",
                    Author = "user (HTML entity: &amp;)",
                    FirstImage = "https://assets.ppy.sh/artists/88/header.jpg",
                    PublishedAt = DateTimeOffset.Now
                }
            }
        };
    }
}
