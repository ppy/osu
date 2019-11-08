// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using osuTK;
using osu.Game.Online.API;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace osu.Game.Overlays.Comments
{
    public abstract class GetCommentRepliesButton : LoadingButton
    {
        private const int duration = 200;

        public readonly BindableList<Comment> Replies = new BindableList<Comment>();
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableInt CurrentPage = new BindableInt();

        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        protected readonly Comment Comment;

        [Resolved]
        private IAPIProvider api { get; set; }

        private SpriteText text;
        private GetCommentRepliesRequest request;

        protected GetCommentRepliesButton(Comment comment)
        {
            Comment = comment;

            AutoSizeAxes = Axes.Both;
            LoadingAnimationSize = new Vector2(8);
            Action = onAction;
        }

        protected override Drawable CreateContent() => new Container
        {
            AutoSizeAxes = Axes.Both,
            Child = text = new SpriteText
            {
                AlwaysPresent = true,
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                Text = ButtonText(),
            }
        };

        protected override void OnLoadStarted() => text.FadeOut(duration, Easing.OutQuint);

        protected override void OnLoadFinished() => text.FadeIn(duration, Easing.OutQuint);

        protected abstract string ButtonText();

        private void onAction()
        {
            CurrentPage.Value += 1;

            Task.Run(async () =>
            {
                var tcs = new TaskCompletionSource<bool>();

                request = new GetCommentRepliesRequest(Comment.Id, Sort.Value, CurrentPage.Value);

                request.Success += response => Schedule(() =>
                {
                    onSuccess(response);
                    tcs.SetResult(true);
                });

                request.Failure += _ => tcs.SetResult(false);

                try
                {
                    request.Perform(api);
                }
                catch
                {
                    tcs.SetResult(false);
                }

                await tcs.Task;
            });
        }

        private void onSuccess(CommentBundle response)
        {
            if (CurrentPage.Value == 1)
            {
                List<Comment> uniqueChildren = new List<Comment>();
                response.Comments.ForEach(c =>
                {
                    if (Replies.All(child => child.Id != c.Id))
                        uniqueChildren.Add(c);
                });
                OnCommentsReceived?.Invoke(uniqueChildren);
            }
            else
                OnCommentsReceived?.Invoke(response.Comments);
        }

        public Action<IEnumerable<Comment>> OnCommentsReceived;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
