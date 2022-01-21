// Partial copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Play.HUD
{
    [LongRunningLoad]
    public class LLinGameplayLeaderboard : GameplayLeaderboard, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private Player player { get; set; }

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private MConfigManager config { get; set; }

        private readonly BindableDouble currentScore = new BindableDouble();
        private readonly BindableDouble currentAcc = new BindableDouble();
        private readonly BindableInt currentCombo = new BindableInt();

        private readonly Bindable<LeaderboardState> displayState = new Bindable<LeaderboardState>();
        private bool updateHint;

        private readonly HintText hintText = new HintText
        {
            Y = -20 - 5,
            Alpha = 0,
            Margin = new MarginPadding { Left = 5 },
            Font = OsuFont.GetFont(size: 20)
        };

        private APIUser user;

        public LLinGameplayLeaderboard(APIUser user = null)
        {
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(hintText);

            //绑定分数
            currentScore.BindTo(scoreProcessor.TotalScore);
            currentAcc.BindTo(scoreProcessor.Accuracy);
            currentCombo.BindTo(scoreProcessor.Combo);

            user ??= api.LocalUser.Value;

            var localScore = Add(user, true);
            localScore.Accuracy.BindTo(currentAcc);
            localScore.Combo.BindTo(currentCombo);
            localScore.TotalScore.BindTo(currentScore);

            config.BindWith(MSetting.InGameLeaderboardState, displayState);
            displayState.BindValueChanged(_ =>
            {
                updateHint = true;
                updateDisplayState();
            }, true);

            player.IsBreakTime.BindValueChanged(_ => updateDisplayState());

            loadScore(infos => Schedule(() => onScoreLoaded(infos)));
        }

        private void updateDisplayState()
        {
            LeaderboardState state = displayState.Value;

            if (player.IsBreakTime.Value) state = LeaderboardState.Expand;

            switch (state)
            {
                case LeaderboardState.Expand:
                    Scroll.MoveToX(0, 300, Easing.OutQuint);
                    Expanded.Value = true;
                    break;

                case LeaderboardState.Fold:
                    Scroll.MoveToX(0, 300, Easing.OutQuint);
                    Expanded.Value = false;
                    break;

                case LeaderboardState.Hide:
                    Scroll.MoveToX(-Width, 300, Easing.OutQuint);
                    break;
            }

            if (updateHint)
            {
                switch (displayState.Value)
                {
                    case LeaderboardState.Expand:
                        hintText.SetText("排行榜将一直展开");
                        break;

                    case LeaderboardState.Fold:
                        hintText.SetText("排行榜将自动折叠");
                        break;

                    case LeaderboardState.Hide:
                        hintText.SetText(player.IsBreakTime.Value ? "排行榜将在休息时间结束后隐藏" : "排行榜已隐藏",
                            !player.IsBreakTime.Value);
                        break;
                }
            }

            updateHint = false;
        }

        private void onScoreLoaded(IEnumerable<ScoreInfo> scoreInfos)
        {
            foreach (var info in scoreInfos)
            {
                var loadedScore = Add(info.User, false);
                loadedScore.Accuracy.Value = info.Accuracy;
                loadedScore.Combo.Value = info.Combo;
                loadedScore.TotalScore.Value = info.TotalScore;
            }
        }

        #region 加载成绩

        [Resolved]
        private RealmContextFactory realmFactory { get; set; }

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private SongSelect songSelect { get; set; }

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        [Resolved]
        private NotificationOverlay notificationOverlay { get; set; }

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// From <see cref="BeatmapLeaderboard"/>
        /// </summary>
        private void loadScore(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            var scope = songSelect.CurrentScope;
            var targetBeatmapInfo = player.Beatmap.Value.BeatmapInfo;

            if (scope.Value == BeatmapLeaderboardScope.Local)
            {
                using (var realm = realmFactory.CreateContext())
                {
                    var scores = realm.All<ScoreInfo>()
                                      .AsEnumerable()
                                      // TODO: update to use a realm filter directly (or at least figure out the beatmap part to reduce scope).
                                      .Where(s => !s.DeletePending && s.BeatmapInfo.ID == targetBeatmapInfo.ID && s.Ruleset.OnlineID == ruleset.Value.ID);

                    // we need to filter out all scores that have any mods to get all local nomod scores
                    //scores = scores.Where(s => !s.Mods.Any());
                    scores = scores.Detach();

                    scoreManager.OrderByTotalScoreAsync(scores.ToArray(), cancellationTokenSource.Token)
                                .ContinueWith(o => scoresCallback?.Invoke(o.GetResultSafely()),
                                    TaskContinuationOptions.OnlyOnRanToCompletion);
                }

                return;
            }

            if (!api.IsLoggedIn
                || !api.LocalUser.Value.IsSupporter
                && (scope.Value != BeatmapLeaderboardScope.Global || songSelect.FilterMods.Value)
                && scope.Value != BeatmapLeaderboardScope.Local)
                return;

            var targetMods = songSelect.FilterMods.Value ? mods.Value : null;

            var req = new GetScoresRequest(targetBeatmapInfo, ruleset.Value ?? targetBeatmapInfo.Ruleset, scope.Value, targetMods);

            req.Success += r =>
            {
                scoreManager.OrderByTotalScoreAsync(r.Scores.Select(s => s.CreateScoreInfo(rulesetStore, targetBeatmapInfo)).ToArray(), cancellationTokenSource.Token)
                            .ContinueWith(task => Schedule(() =>
                            {
                                if (cancellationTokenSource.Token.IsCancellationRequested)
                                    return;

                                var scores = task.GetResultSafely();

                                foreach (var scoreInfo in scores)
                                {
                                    ((ScoreInfo)scoreInfo).Combo = ((ScoreInfo)scoreInfo).MaxCombo;
                                }

                                scoresCallback?.Invoke(task.GetResultSafely());
                            }), TaskContinuationOptions.OnlyOnRanToCompletion);
            };

            req.Failure += e =>
            {
                notificationOverlay.Post(new SimpleNotification
                {
                    Text = "无法查询成绩，因为: " + e.Message
                });
            };

            req.Perform(api);
        }

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            cancellationTokenSource.Cancel();
            base.Dispose(isDisposing);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.LLinSwitchLeaderboardMode:
                    var state = displayState.Value;

                    if (state == LeaderboardState.Hide)
                        state = LeaderboardState.Expand;
                    else
                        state++;

                    displayState.Value = state;

                    break;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private class HintText : OsuSpriteText
        {
            public void SetText(LocalisableString text, bool immedateFadeout = false)
            {
                Text = text;
                this.FadeIn(300, Easing.OutQuint).Delay(immedateFadeout ? 0 : 1500).FadeOut(300, Easing.OutQuint);
            }
        }
    }

    public enum LeaderboardState
    {
        Expand,
        Fold,
        Hide
    }
}
