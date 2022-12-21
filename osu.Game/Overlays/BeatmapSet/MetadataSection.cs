// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public abstract partial class MetadataSection : MetadataSection<string>
    {
        public override string Text
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.FadeOut(TRANSITION_DURATION);
                    return;
                }

                base.Text = value;
            }
        }

        protected MetadataSection(MetadataType type, Action<string>? searchAction = null)
            : base(type, searchAction)
        {
        }
    }

    public abstract partial class MetadataSection<T> : Container
    {
        private readonly FillFlowContainer textContainer;
        private TextFlowContainer? textFlow;

        protected readonly Action<T>? SearchAction;

        protected const float TRANSITION_DURATION = 250;

        protected MetadataSection(MetadataType type, Action<T>? searchAction = null)
        {
            SearchAction = searchAction;

            Alpha = 0;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = textContainer = new FillFlowContainer
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,

                Margin = new MarginPadding { Top = 15 },
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new OsuSpriteText
                        {
                            Text = type.GetLocalisableDescription(),
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                        },
                    },
                },
            };
        }

        public virtual T Text
        {
            set
            {
                if (value == null)
                {
                    this.FadeOut(TRANSITION_DURATION);
                    return;
                }

                this.FadeIn(TRANSITION_DURATION);

                setTextAsync(value);
            }
        }

        private void setTextAsync(T text)
        {
            LoadComponentAsync(new LinkFlowContainer(s => s.Font = s.Font.With(size: 14))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Colour = Color4.White.Opacity(0.75f),
            }, loaded =>
            {
                textFlow?.Expire();

                AddMetadata(text, loaded);

                textContainer.Add(textFlow = loaded);

                // fade in if we haven't yet.
                textContainer.FadeIn(TRANSITION_DURATION);
            });
        }

        protected abstract void AddMetadata(T text, LinkFlowContainer loaded);
    }
}
