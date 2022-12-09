// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Allocation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments.Buttons
{
    public partial class ShowMoreRepliesButton : LoadingButton
    {
        protected override IEnumerable<Drawable> EffectTargets => new[] { text };

        private OsuSpriteText text;

        public ShowMoreRepliesButton()
        {
            AutoSizeAxes = Axes.Both;
            LoadingAnimationSize = new Vector2(8);
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            IdleColour = colourProvider.Light2;
            HoverColour = colourProvider.Light1;
        }

        protected override Drawable CreateContent() => new Container
        {
            AutoSizeAxes = Axes.Both,
            Child = text = new OsuSpriteText
            {
                AlwaysPresent = true,
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                Text = CommonStrings.ButtonsShowMore
            }
        };

        protected override void OnLoadStarted() => text.FadeOut(200, Easing.OutQuint);

        protected override void OnLoadFinished() => text.FadeIn(200, Easing.OutQuint);
    }
}
