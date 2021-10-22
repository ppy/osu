// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardContentBackground : ModelBackedDrawable<IBeatmapSetOnlineInfo>
    {
        public IBeatmapSetOnlineInfo BeatmapSet
        {
            get => Model;
            set => Model = value;
        }

        public new bool Masking
        {
            get => base.Masking;
            set => base.Masking = value;
        }

        public BindableBool Dimmed { get; private set; } = new BindableBool();

        protected override double LoadDelay => 500;

        protected override double TransformDuration => 400;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background2
            };
        }

        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
            => new DelayedLoadUnloadWrapper(createContentFunc, timeBeforeLoad);

        protected override Drawable? CreateDrawable(IBeatmapSetOnlineInfo? model)
        {
            if (model == null)
                return null;

            return new BufferedBackground(model)
            {
                RelativeSizeAxes = Axes.Both,
                Dimmed = { BindTarget = Dimmed }
            };
        }

        private class BufferedBackground : BufferedContainer
        {
            public BindableBool Dimmed { get; } = new BindableBool();

            private readonly IBeatmapSetOnlineInfo onlineInfo;

            private readonly Box background;
            private OnlineBeatmapSetCover? cover;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public BufferedBackground(IBeatmapSetOnlineInfo onlineInfo)
            {
                this.onlineInfo = onlineInfo;

                RelativeSizeAxes = Axes.Both;
                InternalChild = background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                background.Colour = colourProvider.Background2;

                LoadComponentAsync(new OnlineBeatmapSetCover(onlineInfo)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill
                }, loaded =>
                {
                    cover = loaded;
                    cover.Colour = Colour4.Transparent;
                    AddInternal(cover);
                    FinishTransforms(true);
                    updateState();
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Dimmed.BindValueChanged(_ => updateState(), true);
                FinishTransforms(true);
            }

            private void updateState()
            {
                if (cover == null)
                    return;

                background.FadeColour(Dimmed.Value ? colourProvider.Background4 : colourProvider.Background2, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);

                var gradient = ColourInfo.GradientHorizontal(Colour4.White.Opacity(0), Colour4.White.Opacity(0.2f));
                cover.FadeColour(gradient, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            }
        }
    }
}
