// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Bindables;
using osuTK.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Comments
{
    public class DeleteCommentButton : LoadingButton
    {
        private const int duration = 200;

        public readonly BindableBool IsDeleted = new BindableBool();

        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        private OsuSpriteText text;

        public DeleteCommentButton()
        {
            AutoSizeAxes = Axes.Both;
            LoadingAnimationSize = new Vector2(8);
            Action = OnAction;

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

        protected virtual void OnAction()
        {
            Scheduler.AddDelayed(() =>
            {
                IsDeleted.Value = true;
                IsLoading = false;
            }, 50);
        }
    }
}
