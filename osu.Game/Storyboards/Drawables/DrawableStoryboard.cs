// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboard : Container<DrawableStoryboardLayer>
    {
        [Cached(typeof(Storyboard))]
        public Storyboard Storyboard { get; }

        /// <summary>
        /// Whether the storyboard is considered finished.
        /// </summary>
        public IBindable<bool> HasStoryboardEnded => hasStoryboardEnded;

        private readonly BindableBool hasStoryboardEnded = new BindableBool(true);

        protected override Container<DrawableStoryboardLayer> Content { get; }

        protected override Vector2 DrawScale => new Vector2(Parent!.DrawHeight / 480);

        private bool passing = true;

        public bool Passing
        {
            get => passing;
            set
            {
                if (passing == value) return;

                passing = value;
                updateLayerVisibility();
            }
        }

        public override bool RemoveCompletedTransforms => false;

        private double? lastEventEndTime;

        [Cached(typeof(IReadOnlyList<Mod>))]
        public IReadOnlyList<Mod> Mods { get; }

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private DependencyContainer dependencies = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public DrawableStoryboard(Storyboard storyboard, IReadOnlyList<Mod>? mods = null)
        {
            Storyboard = storyboard;
            Mods = mods ?? Array.Empty<Mod>();

            Size = new Vector2(640, 480);

            bool onlyHasVideoElements = Storyboard.Layers.SelectMany(l => l.Elements).All(e => e is StoryboardVideo);

            Width = Height * (storyboard.BeatmapInfo.WidescreenStoryboard || onlyHasVideoElements ? 16 / 9f : 4 / 3f);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddInternal(Content = new Container<DrawableStoryboardLayer>
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(IGameplayClock? clock, CancellationToken? cancellationToken)
        {
            if (clock != null)
                Clock = clock;

            dependencies.CacheAs(typeof(TextureStore),
                new TextureStore(host.Renderer, host.CreateTextureLoaderStore(
                    CreateResourceLookupStore()
                ), false, scaleAdjust: 1));

            foreach (var layer in Storyboard.Layers)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                Add(layer.CreateDrawable());
            }

            lastEventEndTime = Storyboard.LatestEventTime;
        }

        protected virtual IResourceStore<byte[]> CreateResourceLookupStore() => new StoryboardResourceLookupStore(Storyboard, realm, host);

        protected override void Update()
        {
            base.Update();
            hasStoryboardEnded.Value = lastEventEndTime == null || Time.Current >= lastEventEndTime;
        }

        public DrawableStoryboardLayer OverlayLayer => Children.Single(layer => layer.Name == "Overlay");

        private void updateLayerVisibility()
        {
            foreach (var layer in Children)
                layer.Enabled = passing ? layer.Layer.VisibleWhenPassing : layer.Layer.VisibleWhenFailing;
        }

        private class StoryboardResourceLookupStore : IResourceStore<byte[]>
        {
            private readonly IResourceStore<byte[]> realmFileStore;
            private readonly Storyboard storyboard;

            public StoryboardResourceLookupStore(Storyboard storyboard, RealmAccess realm, GameHost host)
            {
                realmFileStore = new RealmFileStore(realm, host.Storage).Store;
                this.storyboard = storyboard;
            }

            public void Dispose() =>
                realmFileStore.Dispose();

            public byte[] Get(string name)
            {
                string? storagePath = storyboard.GetStoragePathFromStoryboardPath(name);

                return string.IsNullOrEmpty(storagePath)
                    ? null!
                    : realmFileStore.Get(storagePath);
            }

            public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken())
            {
                string? storagePath = storyboard.GetStoragePathFromStoryboardPath(name);

                return string.IsNullOrEmpty(storagePath)
                    ? Task.FromResult<byte[]>(null!)
                    : realmFileStore.GetAsync(storagePath, cancellationToken);
            }

            public Stream? GetStream(string name)
            {
                string? storagePath = storyboard.GetStoragePathFromStoryboardPath(name);

                return string.IsNullOrEmpty(storagePath)
                    ? null
                    : realmFileStore.GetStream(storagePath);
            }

            public IEnumerable<string> GetAvailableResources() =>
                realmFileStore.GetAvailableResources();
        }
    }
}
