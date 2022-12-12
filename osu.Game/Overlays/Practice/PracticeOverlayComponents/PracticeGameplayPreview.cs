// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Practice.PracticeOverlayComponents
{
    public partial class PracticeGameplayPreview : CompositeDrawable
    {
        private DrawableRuleset drawableRuleset = null!;

        private GameplayClockContainer gameplayClockContainer = null!;

        public PracticeGameplayPreview()
        {
            //Stops Catch Ruleset and OsuRuleset from clipping the header;
            Masking = true;
            RelativeSizeAxes = Axes.Both;
        }

        private double startTime;
        private double lastTime;

        private Ruleset ruleset;

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private Bindable<RulesetInfo> targetRuleset { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var currentRuleset = targetRuleset.Value.CreateInstance();
            var modAutoplay = currentRuleset.RulesetInfo.CreateInstance().GetAutoplayMod();

            IBeatmap playableBeatmap = loadPlayableBeatmap(new Mod[] { modAutoplay! }, new CancellationToken());

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

            startTime = playableBeatmap.HitObjects.FirstOrDefault()!.StartTime;
            lastTime = playableBeatmap.HitObjects.LastOrDefault()!.GetEndTime();
        }

        private IBeatmap loadPlayableBeatmap(Mod[] gameplayMods, CancellationToken cancellationToken)
        {
            IBeatmap playable;

            try
            {
                if (beatmap.Value.Beatmap == null)
                    throw new InvalidOperationException("Beatmap was not loaded");

                var rulesetInfo = targetRuleset.Value ?? beatmap.Value.BeatmapInfo.Ruleset;
                ruleset = rulesetInfo.CreateInstance();

                if (ruleset == null)
                    throw new RulesetLoadException("Instantiation failure");

                try
                {
                    playable = beatmap.Value.GetPlayableBeatmap(ruleset.RulesetInfo, gameplayMods, cancellationToken);
                }
                catch (BeatmapInvalidForRulesetException)
                {
                    // A playable beatmap may not be creatable with the user's preferred ruleset, so try using the beatmap's default ruleset
                    rulesetInfo = beatmap.Value.BeatmapInfo.Ruleset;
                    ruleset = rulesetInfo.CreateInstance();

                    playable = beatmap.Value.GetPlayableBeatmap(rulesetInfo, gameplayMods, cancellationToken);
                }

                if (playable.HitObjects.Count == 0)
                {
                    Logger.Log("Beatmap contains no hit objects!", level: LogLevel.Error);
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                // Load has been cancelled. No logging is required.
                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap successfully!");
                //couldn't load, hard abort!
                return null;
            }

            return playable;
        }

        public void SeekTo(double percent)
        {
            gameplayClockContainer.Seek(startTime + percent * (lastTime - startTime));
        }
    }
}
