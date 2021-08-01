// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

        /// <summary>
        /// Whether the storyboard is considered finished.
        /// </summary>
        public IBindable<bool> HasStoryboardEnded => hasStoryboardEnded;

        private readonly BindableBool hasStoryboardEnded = new BindableBool(true);

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

        private double? lastEventEndTime;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public DrawableStoryboard(Storyboard storyboard)
        {
            Storyboard = storyboard;

            Size = new Vector2(640, 480);

            bool onlyHasVideoElements = Storyboard.Layers.SelectMany(l => l.Elements).Any(e => !(e is StoryboardVideo));

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
        private void load(FileStore fileStore, GameplayClock clock, CancellationToken? cancellationToken, GameHost host)
        {
            if (clock != null)
                Clock = clock;

            TextureStore textureStore = new TextureStore(host.CreateTextureLoaderStore(fileStore.Store), false, scaleAdjust: 1);

            dependencies.Cache(textureStore);

            double storyboardLatestEventTime = Storyboard.LatestEventTime ?? 0;

            var drawableBackground = Storyboard.CreateSpriteFromResourcePath(Storyboard.BeatmapInfo.Metadata?.BackgroundFile, textureStore);

            if (drawableBackground != null && !Storyboard.ReplacesBackground && storyboardLatestEventTime != 0)
            {
                var sprite = new StoryboardSprite(Storyboard.BeatmapInfo.Metadata?.BackgroundFile, Anchor.Centre, new Vector2(320, 240));
                sprite.TimelineGroup.Scale.Add(Easing.None, 0, 0, 1, 480f / drawableBackground.Height);
                sprite.TimelineGroup.Alpha.Add(Easing.None, 0, storyboardLatestEventTime, 1, 1);
                Storyboard.GetLayer("Background").Elements.Insert(0, sprite);
            }

            foreach (var layer in Storyboard.Layers)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                Add(layer.CreateDrawable());
            }

            lastEventEndTime = Storyboard.LatestEventTime;
        }

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
    }
}
