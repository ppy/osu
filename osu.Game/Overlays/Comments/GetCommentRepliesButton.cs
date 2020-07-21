// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public abstract class GetCommentRepliesButton : LoadingButton
    {
        private const int duration = 200;

        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        private OsuSpriteText text;

        protected GetCommentRepliesButton()
        {
            AutoSizeAxes = Axes.Both;
            LoadingAnimationSize = new Vector2(8);
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

        protected override void OnLoadStarted() => text.FadeOut(duration, Easing.OutQuint);

        protected override void OnLoadFinished() => text.FadeIn(duration, Easing.OutQuint);
    }
}
