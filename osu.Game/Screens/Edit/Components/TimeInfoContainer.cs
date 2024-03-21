// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Components
{
    public partial class TimeInfoContainer : BottomBarContainer
    {
        private OsuSpriteText trackTimer = null!;
        private OsuSpriteText bpm = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            Background.Colour = colourProvider.Background5;

            Children = new Drawable[]
            {
                trackTimer = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Spacing = new Vector2(-2, 0),
                    Font = OsuFont.Torus.With(size: 36, fixedWidth: true, weight: FontWeight.Light),
                    Y = -10,
                },
                bpm = new OsuSpriteText
                {
                    Colour = colours.Orange1,
                    Anchor = Anchor.CentreLeft,
                    Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                    Position = new Vector2(2, 5),
                }
            };
        }

        private double? lastTime;
        private double? lastBPM;

        protected override void Update()
        {
            base.Update();

            if (lastTime != editorClock.CurrentTime)
            {
                lastTime = editorClock.CurrentTime;
                trackTimer.Text = editorClock.CurrentTime.ToEditorFormattedString();
            }

            double newBPM = editorBeatmap.ControlPointInfo.TimingPointAt(editorClock.CurrentTime).BPM;

            if (lastBPM != newBPM)
            {
                lastBPM = newBPM;
                bpm.Text = @$"{newBPM:0} BPM";
            }
        }
    }
}
