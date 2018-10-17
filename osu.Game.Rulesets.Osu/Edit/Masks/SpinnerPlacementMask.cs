// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Edit.Masks
{
    public class SpinnerPlacementMask : PlacementMask
    {
        protected new Spinner HitObject => (Spinner)base.HitObject;

        private PlacementState state;

        private readonly Drawable background;
        private readonly HitCircle innerCircle;

        public SpinnerPlacementMask()
            : base(new Spinner())
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Masking = true,
                Children = new[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.5f,
                    },
                    new HitCircleMask(innerCircle = new HitCircle())
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            };

            // Temporary
            HitObject.Position = OsuPlayfield.BASE_SIZE / 2;
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap workingBeatmap, OsuColour colours)
        {
            background.Colour = colours.Yellow;

            innerCircle.ApplyDefaults(workingBeatmap.Value.Beatmap.ControlPointInfo, workingBeatmap.Value.Beatmap.BeatmapInfo.BaseDifficulty);
        }

        protected override void Update()
        {
            base.Update();

            switch (state)
            {
                case PlacementState.Start:
                    HitObject.StartTime = EditorClock.CurrentTime;
                    break;
                case PlacementState.End:
                    HitObject.EndTime = EditorClock.CurrentTime;
                    break;
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            switch (state)
            {
                case PlacementState.Start:
                    BeginPlacement();
                    state = PlacementState.End;
                    return true;
                case PlacementState.End:
                    EndPlacement();
                    return true;
            }

            return base.OnClick(e);
        }

        private enum PlacementState
        {
            Start,
            End
        }
    }
}
