// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;

namespace osu.Game.Graphics.Containers
{
    public abstract partial class OsuRearrangeableListContainer<TModel> : RearrangeableListContainer<TModel>
    {
        /// <summary>
        /// Whether any item is currently being dragged. Used to hide other items' drag handles.
        /// </summary>
        protected readonly BindableBool DragActive = new BindableBool();

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        private Sample sampleSwap;
        private double sampleLastPlaybackTime;

        protected sealed override RearrangeableListItem<TModel> CreateDrawable(TModel item) => CreateOsuDrawable(item).With(d =>
        {
            d.DragActive.BindTo(DragActive);
        });

        protected abstract OsuRearrangeableListItem<TModel> CreateOsuDrawable(TModel item);

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
            if (!DragActive.Value)
                return;

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
