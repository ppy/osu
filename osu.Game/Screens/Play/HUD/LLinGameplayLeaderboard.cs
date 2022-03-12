// Partial copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
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
using Realms;

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

            if (songSelect.CurrentScope.Value != BeatmapLeaderboardScope.Local)
                loadScoreForLeaderboard();
            else
                Schedule(loadScoreForLeaderboard);
        }

        private void loadScoreForLeaderboard()
        {
            try
            {
                loadScore(infos => Schedule(() => onScoreLoaded(infos)));
            }
            catch (Exception e)
            {
                notificationOverlay.Post(new SimpleNotification
                {
                    Text = "载入成绩时出现了网络错误: " + e.Message
                });
            }
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

        [Resolved]
        private OsuConfigManager osuConfig { get; set; }

        private Bindable<ScoringMode> configScoringMode;

        private void onScoreLoaded(IEnumerable<ScoreInfo> scoreInfos)
        {
            configScoringMode ??= osuConfig.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);

            foreach (var info in scoreInfos)
            {
                var loadedScore = Add(info.User, false);
                var btsd = (ScoreManager.TotalScoreBindableDouble)scoreManager.GetBindableTotalScoreDouble(info);

                //btsd.ScoringMode.BindValueChanged(v =>
                //{
                //    Logger.Log($"模式变更：{v.NewValue}");
                //});

                //bug: 从scoreManager获取的TotalScoreBindableDouble中ScoringMode永远不会变更，即使它绑定到了osuConfig
                //我明明什么都没变，为什么会这样？
                configScoringMode.BindValueChanged(v =>
                {
                    //Logger.Log($"全局模式变更：{v.NewValue}");
                    btsd.ScoringMode.Value = v.NewValue;
                });

                btsd.UnbindBindings();

                loadedScore.Accuracy.Value = info.Accuracy;
                loadedScore.Combo.Value = info.Combo;
                loadedScore.TotalScore.BindTo(btsd);
            }
        }

        #region 加载成绩

        [Resolved]
        private RealmAccess realm { get; set; }

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
        private IDisposable scoreSubscription;

        /// <summary>
        /// From <see cref="BeatmapLeaderboard"/>
        /// </summary>
        private void loadScore(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            var scope = songSelect.CurrentScope;
            var targetBeatmapInfo = player.Beatmap.Value.BeatmapInfo;

            if (scope.Value == BeatmapLeaderboardScope.Local)
            {
                scoreSubscription = realm.RegisterForNotifications(r =>
                    r.All<ScoreInfo>().Filter($"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(targetBeatmapInfo.ID)} == $0"
                                              + $" AND {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $1"
                                              + $" AND {nameof(ScoreInfo.DeletePending)} == false"
                        , targetBeatmapInfo.ID, ruleset.Value.ShortName), localScoresChanged);

                void localScoresChanged(IRealmCollection<ScoreInfo> sender, ChangeSet changes, Exception exception)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        return;

                    var scores = sender.AsEnumerable();

                    if (songSelect.FilterMods.Value && !mods.Value.Any())
                    {
                        // we need to filter out all scores that have any mods to get all local nomod scores
                        scores = scores.Where(s => !s.Mods.Any());
                    }
                    else if (songSelect.FilterMods.Value)
                    {
                        // otherwise find all the scores that have *any* of the currently selected mods (similar to how web applies mod filters)
                        // we're creating and using a string list representation of selected mods so that it can be translated into the DB query itself
                        var selectedMods = mods.Value.Select(m => m.Acronym);
                        scores = scores.Where(s => s.Mods.Any(m => selectedMods.Contains(m.Acronym)));
                    }

                    scores = scores.Detach();

                    scoreManager.OrderByTotalScoreAsync(scores.ToArray(), cancellationTokenSource.Token)
                                .ContinueWith(ordered => Schedule(() =>
                                {
                                    if (cancellationTokenSource.IsCancellationRequested)
                                        return;

                                    scoresCallback.Invoke(ordered.GetResultSafely());
                                }), TaskContinuationOptions.OnlyOnRanToCompletion);
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
