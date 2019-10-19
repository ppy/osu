// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osuTK.Graphics;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using osuTK;
using osu.Game.Online.API;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public class LoadRepliesButton : LoadingButton
    {
        public readonly Bindable<List<Comment>> ChildComments = new Bindable<List<Comment>>();

        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Comment comment;
        private SpriteText text;
        private GetCommentRepliesRequest request;

        public LoadRepliesButton(Comment comment)
        {
            this.comment = comment;

            Action = onAction;

            AutoSizeAxes = Axes.Both;
            LoadingAnimationSize = new Vector2(8);

            IdleColour = OsuColour.Gray(0.7f);
            HoverColour = Color4.White;
        }

        protected override Container CreateBackground() => new Container
        {
            AutoSizeAxes = Axes.Both
        };

        protected override Drawable CreateContent() => text = new SpriteText
        {
            AlwaysPresent = true,
            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
            Text = @"[+] load replies"
        };

        private void onAction()
        {
            request = new GetCommentRepliesRequest(comment.Id);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(CommentBundle response)
        {
            ChildComments.Value = response.Comments.Single().ChildComments;
            IsLoading = false;
        }

        protected override void OnLoadingFinished() => Hide();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
