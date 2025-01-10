// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    [Cached]
    public partial class BeatmapCarouselV2 : Carousel
    {
        private IBindableList<BeatmapSetInfo> detachedBeatmaps = null!;

        private readonly DrawablePool<BeatmapCarouselPanel> carouselPanelPool = new DrawablePool<BeatmapCarouselPanel>(100);

        public BeatmapCarouselV2()
        {
            DebounceDelay = 100;
            DistanceOffscreenToPreload = 100;

            Filters = new ICarouselFilter[]
            {
                new Sorter(),
                new Grouper(),
            };

            AddInternal(carouselPanelPool);
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapStore beatmapStore, CancellationToken? cancellationToken)
        {
            detachedBeatmaps = beatmapStore.GetBeatmapSets(cancellationToken);
            detachedBeatmaps.BindCollectionChanged(beatmapSetsChanged, true);
        }

        protected override Drawable GetDrawableForDisplay(CarouselItem item)
        {
            var drawable = carouselPanelPool.Get();
            drawable.FlashColour(Color4.Red, 2000);

            return drawable;
        }

        protected override CarouselItem CreateCarouselItemForModel(object model) => new BeatmapCarouselItem(model);

        private void beatmapSetsChanged(object? beatmaps, NotifyCollectionChangedEventArgs changed)
        {
            // TODO: moving management of BeatmapInfo tracking to BeatmapStore might be something we want to consider.
            // right now we are managing this locally which is a bit of added overhead.
            IEnumerable<BeatmapSetInfo>? newBeatmapSets = changed.NewItems?.Cast<BeatmapSetInfo>();
            IEnumerable<BeatmapSetInfo>? beatmapSetInfos = changed.OldItems?.Cast<BeatmapSetInfo>();

            switch (changed.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Items.AddRange(newBeatmapSets!.SelectMany(s => s.Beatmaps));
                    break;

                case NotifyCollectionChangedAction.Remove:

                    foreach (var set in beatmapSetInfos!)
                    {
                        foreach (var beatmap in set.Beatmaps)
                            Items.RemoveAll(i => i is BeatmapInfo bi && beatmap.Equals(bi));
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
            }
        }

        public FilterCriteria Criteria { get; private set; } = new FilterCriteria();

        public void Filter(FilterCriteria criteria)
        {
            Criteria = criteria;
            QueueFilter();
        }
    }

    public partial class BeatmapCarouselPanel : PoolableDrawable, ICarouselPanel
    {
        [Resolved]
        private BeatmapCarouselV2 carousel { get; set; } = null!;

        public CarouselItem? Item
        {
            get => item;
            set
            {
                item = value;

                selected.UnbindBindings();

                if (item != null)
                    selected.BindTo(item.Selected);
            }
        }

        private readonly BindableBool selected = new BindableBool();
        private CarouselItem? item;

        [BackgroundDependencyLoader]
        private void load()
        {
            selected.BindValueChanged(value =>
            {
                if (value.NewValue)
                {
                    BorderThickness = 5;
                    BorderColour = Color4.Pink;
                }
                else
                {
                    BorderThickness = 0;
                }
            });
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();
            Item = null;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            Size = new Vector2(500, Item.DrawHeight);
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = (Item.Model is BeatmapInfo ? Color4.Aqua : Color4.Yellow).Darken(5),
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuSpriteText
                {
                    Text = Item.ToString() ?? string.Empty,
                    Padding = new MarginPadding(5),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            carousel.CurrentSelection = Item!.Model;
            return true;
        }
    }

    public class BeatmapCarouselItem : CarouselItem
    {
        public readonly Guid ID;

        public override float DrawHeight => Model is BeatmapInfo ? 40 : 80;

        public BeatmapCarouselItem(object model)
            : base(model)
        {
            ID = (Model as IHasGuidPrimaryKey)?.ID ?? Guid.NewGuid();
        }

        public override string? ToString()
        {
            switch (Model)
            {
                case BeatmapInfo bi:
                    return $"Difficulty: {bi.DifficultyName} ({bi.StarRating:N1}*)";

                case BeatmapSetInfo si:
                    return $"{si.Metadata}";
            }

            return Model.ToString();
        }
    }

    public class Grouper : ICarouselFilter
    {
        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            // TODO: perform grouping based on FilterCriteria

            CarouselItem? lastItem = null;

            var newItems = new List<CarouselItem>(items.Count());

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (item.Model is BeatmapInfo b1)
                {
                    // Add set header
                    if (lastItem == null || (lastItem.Model is BeatmapInfo b2 && b2.BeatmapSet!.OnlineID != b1.BeatmapSet!.OnlineID))
                        newItems.Add(new BeatmapCarouselItem(b1.BeatmapSet!));
                }

                newItems.Add(item);
                lastItem = item;
            }

            return newItems;
        }, cancellationToken).ConfigureAwait(false);
    }

    public class Sorter : ICarouselFilter
    {
        public async Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            return items.OrderDescending(Comparer<CarouselItem>.Create((a, b) =>
            {
                if (a.Model is BeatmapInfo ab && b.Model is BeatmapInfo bb)
                    return ab.OnlineID.CompareTo(bb.OnlineID);

                if (a is BeatmapCarouselItem aItem && b is BeatmapCarouselItem bItem)
                    return aItem.ID.CompareTo(bItem.ID);

                return 0;
            }));
        }, cancellationToken).ConfigureAwait(false);
    }
}
