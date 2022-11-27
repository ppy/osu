﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public abstract partial class CancellableCommentEditor : CommentEditor
    {
        public Action OnCancel;

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

        private partial class CancelButton : OsuHoverContainer
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            private readonly Box background;

            public CancelButton()
                : base(HoverSampleSet.Button)
            {
                AutoSizeAxes = Axes.Both;
                Child = new CircularContainer
                {
                    Masking = true,
                    Height = 25,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Horizontal = 20 },
                            Text = CommonStrings.ButtonsCancel
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light4;
                HoverColour = colourProvider.Light3;
            }
        }
    }
}
