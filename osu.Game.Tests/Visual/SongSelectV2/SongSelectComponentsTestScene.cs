// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public abstract partial class SongSelectComponentsTestScene : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        /// <summary>
        /// The local/online beatmap.
        /// </summary>
        /// <remarks>
        /// This is the same as <see cref="apiBeatmap"/> if online.
        /// </remarks>
        [Cached(typeof(IBindable<IBeatmapInfo?>))]
        protected readonly Bindable<IBeatmapInfo?> BeatmapInfo = new Bindable<IBeatmapInfo?>();

        /// <summary>
        /// The local/online beatmap set.
        /// </summary>
        [Cached(typeof(IBindable<IBeatmapSetInfo?>))]
        private readonly Bindable<IBeatmapSetInfo?> beatmapSetInfo = new Bindable<IBeatmapSetInfo?>();

        /// <summary>
        /// The online beatmap fetched from the api.
        /// </summary>
        [Cached(typeof(IBindable<APIBeatmap?>))]
        private readonly Bindable<APIBeatmap?> apiBeatmap = new Bindable<APIBeatmap?>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // mimics song select's `WorkingBeatmap` binding
            Beatmap.BindValueChanged(b =>
            {
                BeatmapInfo.Value = b.NewValue.BeatmapInfo;
                beatmapSetInfo.Value = b.NewValue?.BeatmapSetInfo;
            });

            // mimics beatmap set overlay's `APIBeatmap` binding
            // after selecting first beatmap from set response (done this way for simplicity)
            BeatmapInfo.BindValueChanged(b =>
            {
                beatmapSetInfo.Value = b.NewValue?.BeatmapSet as APIBeatmapSet;
                apiBeatmap.Value = b.NewValue as APIBeatmap;
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
                beatmapSetInfo.Value = null;
                apiBeatmap.Value = null;
            });
        }
    }
}
