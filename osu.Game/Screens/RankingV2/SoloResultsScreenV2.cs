// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Screens.RankingV2
{
    public partial class SoloResultsScreenV2 : ScreenWithBeatmapBackground
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private SkinnableContainer mainScreen = null!;

        public SoloResultsScreenV2(IScoreInfo score)
        {
            this.score.Value = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren =
            [
                mainScreen = new SkinnableContainer(new GlobalSkinnableContainerLookup(GlobalSkinnableContainers.Results))
                {
                    RelativeSizeAxes = Axes.Both,
                },
            ];
        }

        public void StartAnimating() => mainScreen.StartAnimationSequence();

        public void FinishAnimating() => mainScreen.FinishAnimationSequence();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            // `SkinnableModDisplay` binds to the global mods bindable to read mods to display
            // setting it here seems less bad than adjusting that component to receive an `IScoreInfo` and magically decide what to show.
            var ruleset = score.Value.Ruleset.CreateInstance();
            Mods.Value = score.Value.Mods.Select(m => m.ToMod(ruleset)).ToArray();
        }
    }
}
