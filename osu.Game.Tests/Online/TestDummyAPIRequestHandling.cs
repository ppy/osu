// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Online
{
    public class TestDummyAPIRequestHandling : OsuTestScene
    {
        public TestDummyAPIRequestHandling()
        {
            AddStep("register request handling", () => ((DummyAPIAccess)API).HandleRequest = req =>
            {
                switch (req)
                {
                    case CommentVoteRequest cRequest:
                        cRequest.TriggerSuccess(new CommentBundle());
                        break;
                }
            });

            CommentVoteRequest request = null;
            CommentBundle response = null;

            AddStep("fire request", () =>
            {
                response = null;
                request = new CommentVoteRequest(1, CommentVoteAction.Vote);
                request.Success += res => response = res;
                API.Queue(request);
            });

            AddAssert("got response", () => response != null);
        }
    }
}
