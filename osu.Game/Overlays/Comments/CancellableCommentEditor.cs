// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public abstract partial class CancellableCommentEditor : CommentEditor
    {
        public Action? OnCancel;

        [BackgroundDependencyLoader]
        private void load()
        {
            ButtonsContainer.Add(new CancelButton
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Action = () => OnCancel?.Invoke()
            });
        }

        private sealed partial class CancelButton : RoundedButton
        {
            public CancelButton()
            {
                Height = 25;
                AutoSizeAxes = Axes.X;
            }

            protected override SpriteText CreateText() => new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                Margin = new MarginPadding { Horizontal = 20 },
                Text = CommonStrings.ButtonsCancel
            };
        }
    }
}
