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
using osuTK.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Comments
{
    public class DeleteCommentButton : LoadingButton
    {
        private const int duration = 200;

        public readonly BindableBool IsDeleted = new BindableBool();

        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        [Resolved]
        private IAPIProvider api { get; set; }

        private OsuSpriteText text;
        private DeleteCommentRequest request;

        private readonly long id;

        public DeleteCommentButton(Comment comment)
        {
            id = comment.Id;

            AutoSizeAxes = Axes.Both;
            LoadingAnimationSize = new Vector2(8);
            Action = onAction;

            IdleColour = OsuColour.Gray(0.7f);
            HoverColour = Color4.White;
        }

        protected override Drawable CreateContent() => new Container
        {
            AutoSizeAxes = Axes.Both,
            Child = text = new OsuSpriteText
            {
                AlwaysPresent = true,
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                Text = @"delete"
            }
        };

        protected override void OnLoadStarted() => text.FadeOut(duration, Easing.OutQuint);

        protected override void OnLoadFinished() => text.FadeIn(duration, Easing.OutQuint);

        private void onAction()
        {
            request = new DeleteCommentRequest(id);
            request.Success += _ => onSuccess();
            api.Queue(request);
        }

        private void onSuccess()
        {
            IsDeleted.Value = true;
            IsLoading = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
