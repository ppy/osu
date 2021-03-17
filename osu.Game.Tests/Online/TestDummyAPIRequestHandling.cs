// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Online
{
    [HeadlessTest]
    public class TestDummyAPIRequestHandling : OsuTestScene
    {
        [Test]
        public void TestGenericRequestHandling()
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

            AddAssert("response event fired", () => response != null);

            AddAssert("request has response", () => request.Result == response);
        }

        [Test]
        public void TestQueueRequestHandling()
        {
            registerHandler();

            LeaveChannelRequest request;
            bool gotResponse = false;

            AddStep("fire request", () =>
            {
                gotResponse = false;
                request = new LeaveChannelRequest(new Channel());
                request.Success += () => gotResponse = true;
                API.Queue(request);
            });

            AddAssert("response event fired", () => gotResponse);
        }

        [Test]
        public void TestPerformRequestHandling()
        {
            registerHandler();

            LeaveChannelRequest request;
            bool gotResponse = false;

            AddStep("fire request", () =>
            {
                gotResponse = false;
                request = new LeaveChannelRequest(new Channel());
                request.Success += () => gotResponse = true;
                API.Perform(request);
            });

            AddAssert("response event fired", () => gotResponse);
        }

        [Test]
        public void TestPerformAsyncRequestHandling()
        {
            registerHandler();

            LeaveChannelRequest request;
            bool gotResponse = false;

            AddStep("fire request", () =>
            {
                gotResponse = false;
                request = new LeaveChannelRequest(new Channel());
                request.Success += () => gotResponse = true;
                API.PerformAsync(request);
            });

            AddAssert("response event fired", () => gotResponse);
        }

        private void registerHandler()
        {
            AddStep("register request handling", () => ((DummyAPIAccess)API).HandleRequest = req =>
            {
                switch (req)
                {
                    case LeaveChannelRequest cRequest:
                        cRequest.TriggerSuccess();
                        break;
                }
            });
        }
    }
}
