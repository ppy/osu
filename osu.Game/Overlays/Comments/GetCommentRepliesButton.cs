// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using osuTK;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Framework.Bindables;
using System;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public abstract class GetCommentRepliesButton : LoadingButton
    {
        private const int duration = 200;

        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableInt CurrentPage = new BindableInt();
        public readonly BindableList<Comment> LoadedReplies = new BindableList<Comment>();

        public Action<IEnumerable<Comment>> OnCommentsReceived;

        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Comment comment;
        private OsuSpriteText text;
        private GetCommentRepliesRequest request;

        protected GetCommentRepliesButton(Comment comment)
        {
            this.comment = comment;

            AutoSizeAxes = Axes.Both;
            LoadingAnimationSize = new Vector2(8);
            Action = onAction;
        }

        protected override Drawable CreateContent() => new Container
        {
            AutoSizeAxes = Axes.Both,
            Child = text = new OsuSpriteText
            {
                AlwaysPresent = true,
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                Text = GetText()
            }
        };

        protected abstract string GetText();

        private void onAction()
        {
            CurrentPage.Value++;

            request = new GetCommentRepliesRequest(comment.Id, Sort.Value, CurrentPage.Value);
            request.Success += onSuccess;
            api.PerformAsync(request);
        }

        private void onSuccess(CommentBundle response)
        {
            var receivedComments = response.Comments;

            var uniqueComments = new List<Comment>();

            // We may receive already loaded comments
            receivedComments.ForEach(c =>
            {
                if (LoadedReplies.All(loadedReply => loadedReply.Id != c.Id))
                    uniqueComments.Add(c);
            });

            LoadedReplies.AddRange(uniqueComments);
        }

        protected override void OnLoadStarted() => text.FadeOut(duration, Easing.OutQuint);

        protected override void OnLoadFinished() => text.FadeIn(duration, Easing.OutQuint);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
