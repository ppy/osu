// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    /// <summary>
    /// A marker which shows one movement frame, include any buttons which are pressed.
    /// </summary>
    public partial class FrameMarker : AnalysisMarker
    {
        private CircularProgress leftClickDisplay = null!;
        private CircularProgress rightClickDisplay = null!;
        private Circle mainCircle = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                mainCircle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.Pink2,
                },
                leftClickDisplay = new CircularProgress
                {
                    Colour = Colours.Yellow,
                    Size = new Vector2(0.8f),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Rotation = 180,
                    Progress = 0.5f,
                    InnerRadius = 0.5f,
                    RelativeSizeAxes = Axes.Both,
                },
                rightClickDisplay = new CircularProgress
                {
                    Colour = Colours.Yellow,
                    Size = new Vector2(0.8f),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Progress = 0.5f,
                    InnerRadius = 0.5f,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override void OnApply(AnalysisFrameEntry entry)
        {
            base.OnApply(entry);
            Size = new Vector2(entry.Action.Any() ? 4 : 2.5f);

            mainCircle.Colour = entry.Action.Any() ? Colours.Gray4 : Colours.Pink2;

            leftClickDisplay.Alpha = entry.Action.Contains(OsuAction.LeftButton) ? 1 : 0;
            rightClickDisplay.Alpha = entry.Action.Contains(OsuAction.RightButton) ? 1 : 0;
        }
    }
}
