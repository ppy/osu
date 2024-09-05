// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        private BindableBool hitMarkersEnabled { get; } = new BindableBool();
        private BindableBool aimMarkersEnabled { get; } = new BindableBool();
        private BindableBool aimLinesEnabled { get; } = new BindableBool();

        protected readonly ClickMarkerContainer ClickMarkers;
        protected readonly FrameMarkerContainer FrameMarkers;
        protected readonly CursorPathContainer CursorPath;

        private readonly Replay replay;

        public ReplayAnalysisOverlay(Replay replay)
        {
            RelativeSizeAxes = Axes.Both;

            this.replay = replay;

            InternalChildren = new Drawable[]
            {
                CursorPath = new CursorPathContainer(),
                ClickMarkers = new ClickMarkerContainer(),
                FrameMarkers = new FrameMarkerContainer(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuRulesetConfigManager config)
        {
            loadReplay();

            config.BindWith(OsuRulesetSetting.ReplayHitMarkersEnabled, hitMarkersEnabled);
            config.BindWith(OsuRulesetSetting.ReplayAimMarkersEnabled, aimMarkersEnabled);
            config.BindWith(OsuRulesetSetting.ReplayAimLinesEnabled, aimLinesEnabled);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            hitMarkersEnabled.BindValueChanged(enabled => ClickMarkers.FadeTo(enabled.NewValue ? 1 : 0), true);
            aimMarkersEnabled.BindValueChanged(enabled => FrameMarkers.FadeTo(enabled.NewValue ? 1 : 0), true);
            aimLinesEnabled.BindValueChanged(enabled => CursorPath.FadeTo(enabled.NewValue ? 1 : 0), true);
        }

        private void loadReplay()
        {
            bool leftHeld = false;
            bool rightHeld = false;

            OsuAction? lastAction = null;

            foreach (var frame in replay.Frames)
            {
                var osuFrame = (OsuReplayFrame)frame;

                bool leftButton = osuFrame.Actions.Contains(OsuAction.LeftButton);
                bool rightButton = osuFrame.Actions.Contains(OsuAction.RightButton);

                if (leftHeld && !leftButton)
                    leftHeld = false;
                else if (!leftHeld && leftButton)
                {
                    leftHeld = true;
                    lastAction = OsuAction.LeftButton;
                    ClickMarkers.Add(new AnalysisFrameEntry(osuFrame.Time, osuFrame.Position, OsuAction.LeftButton));
                }

                if (rightHeld && !rightButton)
                    rightHeld = false;
                else if (!rightHeld && rightButton)
                {
                    rightHeld = true;
                    lastAction = OsuAction.RightButton;
                    ClickMarkers.Add(new AnalysisFrameEntry(osuFrame.Time, osuFrame.Position, OsuAction.RightButton));
                }

                if (!leftButton && !rightButton)
                    lastAction = null;

                FrameMarkers.Add(new AnalysisFrameEntry(osuFrame.Time, osuFrame.Position, lastAction));
                CursorPath.Add(new AnalysisFrameEntry(osuFrame.Time, osuFrame.Position));
            }
        }
    }
}
