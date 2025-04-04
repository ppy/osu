// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI.ReplayAnalysis;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class ReplayAnalysisOverlay : CompositeDrawable
    {
        private BindableBool showClickMarkers { get; } = new BindableBool();
        private BindableBool showFrameMarkers { get; } = new BindableBool();
        private BindableBool showCursorPath { get; } = new BindableBool();
        private BindableInt displayLength { get; } = new BindableInt();

        protected ClickMarkerContainer? ClickMarkers;
        protected FrameMarkerContainer? FrameMarkers;
        protected CursorPathContainer? CursorPath;

        private readonly Replay replay;

        private int replayFrameIndex;

        public ReplayAnalysisOverlay(Replay replay)
        {
            RelativeSizeAxes = Axes.Both;

            this.replay = replay;
        }

        private bool requireDisplay => showClickMarkers.Value || showFrameMarkers.Value || showCursorPath.Value;

        [BackgroundDependencyLoader]
        private void load(OsuRulesetConfigManager config)
        {
            config.BindWith(OsuRulesetSetting.ReplayClickMarkersEnabled, showClickMarkers);
            config.BindWith(OsuRulesetSetting.ReplayFrameMarkersEnabled, showFrameMarkers);
            config.BindWith(OsuRulesetSetting.ReplayCursorPathEnabled, showCursorPath);
            config.BindWith(OsuRulesetSetting.ReplayAnalysisDisplayLength, displayLength);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            displayLength.BindValueChanged(_ =>
            {
                // Need to fully reload to make this work.
                invalidateLoaded();
            }, true);
        }

        /// <summary>
        /// Invalidated when containers are not loaded nor loading, false if loading, and true if loaded
        /// </summary>
        /// <remarks>
        /// Knowing the loading/loaded state is for avoiding an enumeration error when adding
        /// new entries and not starting a new load while loading
        /// </remarks>
        private readonly Cached<bool> loadState = new Cached<bool>();

        private CancellationTokenSource? generationCancellationSource;

        private void invalidateLoaded()
        {
            loadState.Invalidate();
            replayFrameIndex = 0;
        }

        protected override void Update()
        {
            base.Update();

            if (requireDisplay)
            {
                initialise();
                // adding entries while the component is asynchronously loading
                // can collide with enumeration operations and cause an error
                if (loadState.IsValid && loadState.Value)
                    addEntries();
            }

            if (ClickMarkers != null) ClickMarkers.Alpha = showClickMarkers.Value ? 1 : 0;
            if (FrameMarkers != null) FrameMarkers.Alpha = showFrameMarkers.Value ? 1 : 0;
            if (CursorPath != null) CursorPath.Alpha = showCursorPath.Value ? 1 : 0;
        }

        private void initialise()
        {
            if (loadState.IsValid)
                return;

            loadState.Value = false;

            generationCancellationSource?.Cancel();
            generationCancellationSource = new CancellationTokenSource();

            // It's faster to reinitialise the whole drawable stack than use `Clear` on `PooledDrawableWithLifetimeContainer`
            var newDrawables = new Drawable[]
            {
                CursorPath = new CursorPathContainer(),
                ClickMarkers = new ClickMarkerContainer(),
                FrameMarkers = new FrameMarkerContainer(),
            };

            LoadComponentsAsync(newDrawables, drawables =>
            {
                InternalChildrenEnumerable = drawables;
                loadState.Value = true;
            }, generationCancellationSource.Token);
        }

        private void addEntries()
        {
            bool leftHeld = false;
            bool rightHeld = false;

            // This should probably be async as well, but it's a bit of a pain to debounce and everything.
            // Let's address concerns when they are raised.
            while (replayFrameIndex < replay.Frames.Count)
            {
                var osuFrame = (OsuReplayFrame)replay.Frames[replayFrameIndex];

                bool leftButton = osuFrame.Actions.Contains(OsuAction.LeftButton);
                bool rightButton = osuFrame.Actions.Contains(OsuAction.RightButton);

                if (leftHeld && !leftButton)
                    leftHeld = false;
                else if (!leftHeld && leftButton)
                {
                    leftHeld = true;
                    ClickMarkers!.Add(new AnalysisFrameEntry(osuFrame.Time, displayLength.Value, osuFrame.Position, OsuAction.LeftButton));
                }

                if (rightHeld && !rightButton)
                    rightHeld = false;
                else if (!rightHeld && rightButton)
                {
                    rightHeld = true;
                    ClickMarkers!.Add(new AnalysisFrameEntry(osuFrame.Time, displayLength.Value, osuFrame.Position, OsuAction.RightButton));
                }

                FrameMarkers!.Add(new AnalysisFrameEntry(osuFrame.Time, displayLength.Value, osuFrame.Position, osuFrame.Actions.ToArray()));
                CursorPath!.Add(new AnalysisFrameEntry(osuFrame.Time, displayLength.Value, osuFrame.Position));

                replayFrameIndex++;
            }
        }
    }
}
