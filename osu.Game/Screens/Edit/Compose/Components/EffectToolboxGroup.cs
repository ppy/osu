// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class EffectToolboxGroup : EditorToolboxGroup
    {
        private readonly bool showScrollSpeed;

        private OsuSpriteText? scrollSpeedText;
        private OsuSpriteText kiaiText = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        private EffectControlPoint? displayedPoint;
        private double displayedScrollSpeed;
        private bool displayedKiai;

        public EffectToolboxGroup(bool showScrollSpeed)
            : base("effect")
        {
            this.showScrollSpeed = showScrollSpeed;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Spacing = new Vector2(5);

            if (showScrollSpeed)
                Add(scrollSpeedText = new OsuSpriteText());

            Add(kiaiText = new OsuSpriteText());
        }

        protected override void Update()
        {
            base.Update();

            var point = editorBeatmap.ControlPointInfo.EffectPointAt(editorClock.CurrentTime);

            if (ReferenceEquals(point, displayedPoint) && point.ScrollSpeed == displayedScrollSpeed && point.KiaiMode == displayedKiai)
                return;

            displayedPoint = point;
            displayedScrollSpeed = point.ScrollSpeed;
            displayedKiai = point.KiaiMode;

            scrollSpeedText?.Text = $"Scroll: {point.ScrollSpeed:n2}x";

            kiaiText.Text = $"Kiai: {(point.KiaiMode ? "On" : "Off")}";
        }
    }
}
