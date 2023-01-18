// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osuTK.Graphics;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboard : CompositeDrawable
    {
        [Cached]
        public Storyboard Storyboard { get; }

        /// <summary>
        /// Whether the storyboard is considered finished.
        /// </summary>
        public IBindable<bool> HasStoryboardEnded => hasStoryboardEnded;

        private readonly BindableBool hasStoryboardEnded = new BindableBool(true);

        /// <summary>
        /// All layers in the storyboard.
        /// </summary>
        private readonly IList<DrawableStoryboardLayer> layers = new List<DrawableStoryboardLayer>();

        protected override Vector2 DrawScale => new Vector2(Parent.DrawHeight / 480);

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

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public DrawableStoryboard(Storyboard storyboard, IReadOnlyList<Mod> mods = null)
        {
            Storyboard = storyboard;
            Mods = mods ?? Array.Empty<Mod>();

            Size = new Vector2(640, 480);

            bool onlyHasVideoElements = Storyboard.Layers.SelectMany(l => l.Elements).Any(e => !(e is StoryboardVideo));

            Width = Height * (storyboard.BeatmapInfo.WidescreenStoryboard || onlyHasVideoElements ? 16 / 9f : 4 / 3f);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            if (Storyboard.ReplacesBackground && Storyboard.HasDrawable)
            {
                AddInternal(new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                });
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(IGameplayClock clock, CancellationToken? cancellationToken, GameHost host, RealmAccess realm)
        {
            if (clock != null)
                Clock = clock;

            dependencies.Cache(new TextureStore(host.Renderer, host.CreateTextureLoaderStore(new RealmFileStore(realm, host.Storage).Store), false, scaleAdjust: 1));

            foreach (var layer in Storyboard.Layers)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                var drawable = layer.CreateDrawable();
                layers.Add(drawable);
                AddInternal(drawable);
            }

            lastEventEndTime = Storyboard.LatestEventTime;
        }

        protected override void Update()
        {
            base.Update();
            hasStoryboardEnded.Value = lastEventEndTime == null || Time.Current >= lastEventEndTime;
        }

        public DrawableStoryboardLayer OverlayLayer => layers.Single(layer => layer.Name == "Overlay");

        private void updateLayerVisibility()
        {
            foreach (var layer in layers)
                layer.Enabled = passing ? layer.Layer.VisibleWhenPassing : layer.Layer.VisibleWhenFailing;
        }
    }
}
