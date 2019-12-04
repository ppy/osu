// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Comments
{
    public class OnlineCommentsContainer : CommentsContainer
    {
        private int currentPage;
        private CommentBundleParameters parameters;

        public void ShowComments(CommentableType type, long id)
        {
            parameters = new CommentBundleParameters(type, id);
            Sort.TriggerChange();
        }

        protected override APIRequest FetchComments(Action<CommentBundle> commentsCallback)
        {
            if (parameters == null)
                return null;

            var req = new GetCommentsRequest(parameters, Sort.Value, ++currentPage);
            req.Success += r => commentsCallback?.Invoke(r);
            return req;
        }

        protected override void OnLoadStarted()
        {
            currentPage = 0;
            base.OnLoadStarted();
        }
    }
}
