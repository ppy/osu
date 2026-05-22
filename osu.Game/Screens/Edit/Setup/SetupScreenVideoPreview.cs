// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class SetupScreenVideoPreview : CompositeDrawable
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        private DependencyContainer dependencies = null!;
        private TextureStore textureStore = null!;
        private readonly Container content;

        public SetupScreenVideoPreview()
        {
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 3.5f,
            };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(GameHost host, RealmAccess realmAccess)
        {
            var lookupStore = new DrawableStoryboard.StoryboardResourceLookupStore(working.Value.Storyboard, realmAccess, host);
            dependencies.CacheAs(textureStore = new TextureStore(host.Renderer, host.CreateTextureLoaderStore(lookupStore), false, scaleAdjust: 1));

            UpdateVideo();
        }

        public void UpdateVideo()
        {
            var video = working.Value.Storyboard.PrimaryVideo;

            if (video == null)
            {
                displayPlaceholder();
                return;
            }

            var stream = textureStore.GetStream(video.Path);

            if (stream == null)
            {
                displayPlaceholder();
                return;
            }

            LoadComponentAsync(new Video(stream, startAtCurrentTime: false)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
                Loop = true,
            }, v =>
            {
                content.Child = v;
                v.FadeInFromZero(500);
            });
        }

        private void displayPlaceholder()
        {
            content.Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.GreySeaFoamDarker,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 24))
                {
                    Text = EditorSetupStrings.DragToSetVideo,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both
                }
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (textureStore.IsNotNull())
                textureStore.Dispose();
        }
    }
}
