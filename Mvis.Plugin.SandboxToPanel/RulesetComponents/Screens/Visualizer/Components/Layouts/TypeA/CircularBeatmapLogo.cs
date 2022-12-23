using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.MusicHelpers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

#nullable disable

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts.TypeA
{
    public partial class CircularBeatmapLogo : CurrentBeatmapProvider
    {
        private const int base_size = 350;
        private const int progress_padding = 10;

        public Color4 ProgressColour
        {
            get => progress.Colour;
            set => progress.Colour = value;
        }

        public new Bindable<int> Size = new Bindable<int>(base_size);

        private CircularProgress progress;
        private Container progressWrapper;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new UpdateableBeatmapBackground
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                progressWrapper = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = progress = new CircularProgress
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        InnerRadius = 0.03f,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Size.BindValueChanged(s =>
            {
                base.Size = new Vector2(s.NewValue);
                progressWrapper.Padding = new MarginPadding(progress_padding * s.NewValue / base_size);
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            var track = Beatmap.Value?.Track;
            progress.Current.Value = (track == null || track.Length == 0) ? 0 : (track.CurrentTime / track.Length);
        }
    }
}
