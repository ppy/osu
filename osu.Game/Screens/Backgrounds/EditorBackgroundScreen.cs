// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Screens.Backgrounds
{
    public partial class EditorBackgroundScreen : BackgroundScreen
    {
        private readonly Container dimContainer;

        private CancellationTokenSource? cancellationTokenSource;
        private Bindable<float> dimLevel = null!;
        private Bindable<bool> showStoryboard = null!;

        private BeatmapBackgroundWithStoryboard? background;

        private readonly Container content;
        private readonly Box blackBox;

        // We retrieve IBindable<WorkingBeatmap> from our dependency cache instead of passing WorkingBeatmap directly into EditorBackgroundScreen.
        // Otherwise, DummyWorkingBeatmap will be erroneously passed in whenever creating a new beatmap (since the Schedule() in the Editor that populates
        // a new WorkingBeatmap with correct values generally runs after EditorBackgroundScreen is created), which causes any background changes to not be displayed.
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        public EditorBackgroundScreen(EditorBeatmap editorBeatmap)
        {
            InternalChild = dimContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    // This adds overdraw but makes transitions not suck.
                    // There's probably a better way to do this, but it's high effort.
                    blackBox = new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                    content = new EditorSkinProvidingContainer(editorBeatmap)
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            dimLevel = config.GetBindable<float>(OsuSetting.EditorDim);
            showStoryboard = config.GetBindable<bool>(OsuSetting.EditorShowStoryboard);

            content.Child = createContent();
            updateState(withAnimation: false);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            dimLevel.BindValueChanged(_ => dimContainer.FadeColour(OsuColour.Gray(1 - dimLevel.Value), 500, Easing.OutQuint), true);
            showStoryboard.BindValueChanged(_ => updateState());

            updateState(withAnimation: false);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            blackBox.LifetimeEnd = LatestTransformEndTime;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // The storyboard will do weird things with clock time changing on exit, so let's just hide it instead.
            background?.UnloadStoryboard();

            return base.OnExiting(e);
        }

        public void RefreshBackgroundAsync()
        {
            cancellationTokenSource?.Cancel();
            LoadComponentAsync(createContent(), loaded =>
            {
                content.Child = loaded;
                updateState(withAnimation: false);
            }, (cancellationTokenSource = new CancellationTokenSource()).Token);
        }

        private Drawable createContent() => background = new BeatmapBackgroundWithStoryboard(beatmap.Value)
        {
            RelativeSizeAxes = Axes.Both,
            StoryboardLoaded = () => updateState(withAnimation: false)
        };

        private void updateState(bool withAnimation = true)
        {
            background?.Storyboard.FadeTo(showStoryboard.Value ? 1 : 0, withAnimation ? 500 : 0, Easing.OutQuint);
            // if the storyboard is disabled, in some cases (e.g. involving `StoryboardReplacesBackground`)
            // we still need to show the background sprite, because if we don't, then there will be no background shown at all
            background?.Sprite.FadeTo(showStoryboard.Value ? 0 : 1, withAnimation ? 500 : 0, Easing.OutQuint);
        }

        public override bool Equals(BackgroundScreen? other)
        {
            if (other is not EditorBackgroundScreen otherBeatmapBackground)
                return false;

            return base.Equals(other) && beatmap == otherBeatmapBackground.beatmap;
        }
    }
}
