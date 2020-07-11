using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Mvis.Storyboard
{
    public class BackgroundStoryboardContainer : Container
    {
        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        public DimmableStoryboard dimmableSB;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            this.Add(dimmableSB = new DimmableStoryboard(b.Value.Storyboard)
            {
                RelativeSizeAxes = Axes.Both,
                Name = "Storyboard"
            });
            dimmableSB.IgnoreUserSettings.Value = true;
            dimmableSB.EnableUserDim.Value = false;
        }
    }
}