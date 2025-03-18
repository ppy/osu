// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;

namespace osu.Game.Graphics.Containers.Draggable
{
    public abstract partial class OsuDraggableItemContainer<TModel> : DraggableItemContainer<TModel>
        where TModel : notnull
    {
        // todo : if ScrollContainer is empty it should ignore drag input.
        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        private Sample sampleSwap = null!;
        private double sampleLastPlaybackTime;

        protected sealed override DraggableItem<TModel> CreateDrawable(TModel item) => CreateOsuDrawable(item);

        protected abstract OsuDraggableItem<TModel> CreateOsuDrawable(TModel item);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Items.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Move)
                    playSwapSample();
            };
        }

        private void playSwapSample()
        {
            if (Time.Current - sampleLastPlaybackTime <= 35)
                return;

            var channel = sampleSwap?.GetChannel();
            if (channel == null)
                return;

            channel.Frequency.Value = 0.96 + RNG.NextDouble(0.08);
            channel.Play();
            sampleLastPlaybackTime = Time.Current;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleSwap = audio.Samples.Get(@"UI/item-swap");
            sampleLastPlaybackTime = Time.Current;
        }

    }
}
