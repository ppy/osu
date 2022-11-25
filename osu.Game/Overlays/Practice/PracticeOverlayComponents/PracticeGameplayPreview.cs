// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Practice.PracticeOverlayComponents
{
    public class PracticeGameplayPreview : CompositeDrawable
    {
        private DrawableRuleset drawableRuleset = null!;

        private GameplayClockContainer gameplayClockContainer = null!;

        public PracticeGameplayPreview()
        {
            //Stops Catch Ruleset and OsuRuleset from clipping the header;
            Masking = true;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            var currentRuleset = beatmap.Value.BeatmapInfo.Ruleset.CreateInstance();
            var modAutoplay = currentRuleset.RulesetInfo.CreateInstance().GetAutoplayMod();

            var playableBeatmap = beatmap.Value.GetPlayableBeatmap(currentRuleset.RulesetInfo);
            var rulesetSkinProvider = new RulesetSkinProvidingContainer(currentRuleset, playableBeatmap, beatmap.Value.Skin);

            InternalChild = gameplayClockContainer = new GameplayClockContainer(new FramedBeatmapClock());

            gameplayClockContainer.Add(rulesetSkinProvider);
            gameplayClockContainer.Start();

            //We dont want the preview to use mods.
            drawableRuleset = currentRuleset.CreateDrawableRulesetWith(playableBeatmap, new[] { modAutoplay! });
            rulesetSkinProvider.Add(drawableRuleset);

            drawableRuleset.Cursor?.FadeTo(0);
            drawableRuleset.Playfield.DisplayJudgements.Value = false;
            drawableRuleset.FrameStablePlayback = false;

            var autoplayMod = drawableRuleset.Mods.OfType<ModAutoplay>().Single();
            drawableRuleset.SetReplayScore(autoplayMod.CreateScoreFromReplayData(playableBeatmap, drawableRuleset.Mods));
        }

        public void SeekTo(double seekTime)
        {
            gameplayClockContainer.Seek(seekTime);
        }
    }
}
