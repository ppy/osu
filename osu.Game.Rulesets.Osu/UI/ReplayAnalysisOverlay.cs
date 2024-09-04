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

        protected readonly HitMarkersContainer HitMarkers;
        protected readonly AimMarkersContainer AimMarkers;
        protected readonly AimLinesContainer AimLines;

        private readonly Replay replay;

        public ReplayAnalysisOverlay(Replay replay)
        {
            RelativeSizeAxes = Axes.Both;

            this.replay = replay;

            InternalChildren = new Drawable[]
            {
                HitMarkers = new HitMarkersContainer(),
                AimLines = new AimLinesContainer(),
                AimMarkers = new AimMarkersContainer(),
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

            hitMarkersEnabled.BindValueChanged(enabled => HitMarkers.FadeTo(enabled.NewValue ? 1 : 0), true);
            aimMarkersEnabled.BindValueChanged(enabled => AimMarkers.FadeTo(enabled.NewValue ? 1 : 0), true);
            aimLinesEnabled.BindValueChanged(enabled => AimLines.FadeTo(enabled.NewValue ? 1 : 0), true);
        }

        private void loadReplay()
        {
            bool leftHeld = false;
            bool rightHeld = false;

            foreach (var frame in replay.Frames)
            {
                var osuFrame = (OsuReplayFrame)frame;

                AimMarkers.Add(new AimPointEntry(osuFrame.Time, osuFrame.Position));
                AimLines.Add(new AimPointEntry(osuFrame.Time, osuFrame.Position));

                bool leftButton = osuFrame.Actions.Contains(OsuAction.LeftButton);
                bool rightButton = osuFrame.Actions.Contains(OsuAction.RightButton);

                if (leftHeld && !leftButton)
                    leftHeld = false;
                else if (!leftHeld && leftButton)
                {
                    HitMarkers.Add(new HitMarkerEntry(osuFrame.Time, osuFrame.Position, true));
                    leftHeld = true;
                }

                if (rightHeld && !rightButton)
                    rightHeld = false;
                else if (!rightHeld && rightButton)
                {
                    HitMarkers.Add(new HitMarkerEntry(osuFrame.Time, osuFrame.Position, false));
                    rightHeld = true;
                }
            }
        }
    }
}
