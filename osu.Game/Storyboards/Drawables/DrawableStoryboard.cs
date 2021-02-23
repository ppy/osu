// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.IO;
using osu.Game.Screens.Play;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboard : Container<DrawableStoryboardLayer>
    {
        [Cached]
        public Storyboard Storyboard { get; }

        protected override Container<DrawableStoryboardLayer> Content { get; }

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

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public DrawableStoryboard(Storyboard storyboard)
        {
            Storyboard = storyboard;
            Size = new Vector2(640, 480);
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
        private void load(FileStore fileStore, GameplayClock clock, CancellationToken? cancellationToken, GameHost host)
        {
            if (clock != null)
                Clock = clock;

            dependencies.Cache(new TextureStore(host.CreateTextureLoaderStore(fileStore.Store), false, scaleAdjust: 1));

            foreach (var layer in Storyboard.Layers)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                Add(layer.CreateDrawable());
            }
        }

        public DrawableStoryboardLayer OverlayLayer => Children.Single(layer => layer.Name == "Overlay");

        private void updateLayerVisibility()
        {
            foreach (var layer in Children)
                layer.Enabled = passing ? layer.Layer.VisibleWhenPassing : layer.Layer.VisibleWhenFailing;
        }
    }
}
