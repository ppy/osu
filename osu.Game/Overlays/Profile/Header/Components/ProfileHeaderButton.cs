// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public abstract class ProfileHeaderButton : UserBindingComponent, IHasTooltip
    {
        public virtual string TooltipText { get; set; }

        protected override Container<Drawable> Content => content;

        protected Action Action
        {
            set => content.Action = value;
            get => content.Action;
        }

        private readonly ContentContainer content;

        protected ProfileHeaderButton()
        {
            AutoSizeAxes = Axes.X;
            AddInternal(content = new ContentContainer());
        }

        protected class ContentContainer : OsuHoverContainer
        {
            private readonly Box background;
            private readonly Container content;

            protected override Container<Drawable> Content => content;

            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            public ContentContainer()
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                IdleColour = Color4.Black;
                HoverColour = OsuColour.Gray(0.1f);

                base.Content.Add(new CircularContainer
                {
                    Masking = true,
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        content = new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Horizontal = 10 },
                        }
                    }
                });
            }
        }
    }
}
