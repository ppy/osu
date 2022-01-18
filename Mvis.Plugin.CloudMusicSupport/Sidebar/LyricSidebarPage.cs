using System;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Screens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.LLin;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Types;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar
{
    public class LyricSidebarSectionContainer : PluginSidebarPage
    {
        private LoadingSpinner loading;

        public LyricSidebarSectionContainer(LLinPlugin plugin)
            : base(plugin)
        {
            Icon = FontAwesome.Solid.Music;
        }

        public override IPluginFunctionProvider GetFunctionEntry()
            => new LyricFunctionProvider(this);

        [Resolved]
        private IImplementLLin mvisScreen { get; set; }

        private LyricPlugin plugin => (LyricPlugin)Plugin;

        public Guid BeatmapSetId;

        private ScreenStack screenStack;

        private Toolbox toolbox;

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider provider)
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        screenStack = new SidebarScreenStack
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Width = 0.4f
                        },
                        toolbox = new Toolbox
                        {
                            OnBackAction = screenStack.Exit
                        }
                    }
                },
                loading = new LoadingLayer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };

            plugin.CurrentStatus.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case LyricPlugin.Status.Finish:
                    case LyricPlugin.Status.Failed:
                        loading.Hide();
                        break;

                    default:
                        loading.Show();
                        break;
                }
            }, true);

            screenStack.ScreenPushed += onScreenChanged;
            screenStack.ScreenExited += onScreenChanged;
        }

        private void onScreenChanged(IScreen lastscreen, IScreen newscreen)
        {
            if (newscreen is SidebarScreen screen)
                toolbox.AddButtonRange(screen.Entries, (screen is LyricViewScreen));
        }

        protected override void LoadComplete()
        {
            mvisScreen.OnBeatmapChanged(refreshBeatmap, this);
            refreshBeatmap(mvisScreen.Beatmap.Value);

            screenStack.Push(new LyricViewScreen());
            base.LoadComplete();
        }

        private void refreshBeatmap(WorkingBeatmap working)
        {
            BeatmapSetId = working.BeatmapSetInfo.ID;
            toolbox.IdText = $"ID: {BeatmapSetId}";
        }
    }
}
