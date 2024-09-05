// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    /// <summary>
    /// A marker which shows one click, with visuals focusing on the button which was clicked and the precise location of the click.
    /// </summary>
    public partial class ClickMarker : AnalysisMarker
    {
        private CircularProgress leftClickDisplay = null!;
        private CircularProgress rightClickDisplay = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.125f),
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Colour = Colours.Gray5,
                },
                new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.Gray5,
                    Masking = true,
                    BorderThickness = 2.2f,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0,
                    },
                },
                leftClickDisplay = new CircularProgress
                {
                    Colour = Colours.Yellow,
                    Size = new Vector2(0.95f),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Rotation = 180,
                    Progress = 0.5f,
                    InnerRadius = 0.18f,
                    RelativeSizeAxes = Axes.Both,
                },
                rightClickDisplay = new CircularProgress
                {
                    Colour = Colours.Yellow,
                    Size = new Vector2(0.95f),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Progress = 0.5f,
                    InnerRadius = 0.18f,
                    RelativeSizeAxes = Axes.Both,
                },
            };

            Size = new Vector2(16);
        }

        protected override void OnApply(AnalysisFrameEntry entry)
        {
            base.OnApply(entry);

            leftClickDisplay.Alpha = entry.Action.Contains(OsuAction.LeftButton) ? 1 : 0;
            rightClickDisplay.Alpha = entry.Action.Contains(OsuAction.RightButton) ? 1 : 0;
        }
    }
}
