// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public abstract partial class SongSelectComponentsTestScene : OsuTestScene
    {
        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        /// <summary>
        /// The beatmap. Can be local/online depending on the context.
        /// </summary>
        [Cached(typeof(IBindable<IBeatmapInfo?>))]
        protected readonly Bindable<IBeatmapInfo?> BeatmapInfo = new Bindable<IBeatmapInfo?>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // mimics song select's `WorkingBeatmap` binding
            Beatmap.BindValueChanged(b =>
            {
                BeatmapInfo.Value = b.NewValue.BeatmapInfo;
            });
        }

        [SetUpSteps]
        public virtual void SetUpSteps()
        {
            AddStep("reset dependencies", () =>
            {
                Beatmap.Value = Beatmap.Default;
                SelectedMods.SetDefault();
                BeatmapInfo.Value = null;
            });
        }
    }
}
