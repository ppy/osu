// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTextBox : BasicTextBox
    {
        protected override float LeftRightPadding => 10;

        protected override float CaretWidth => 3;

        protected override SpriteText CreatePlaceholder() => new OsuSpriteText
        {
            Font = OsuFont.GetFont(italics: true),
            Colour = new Color4(180, 180, 180, 255),
            Margin = new MarginPadding { Left = 2 },
        };

        public OsuTextBox()
        {
            Height = 40;
            TextContainer.Height = 0.5f;
            CornerRadius = 5;
            LengthLimit = 1000;

            Current.DisabledChanged += disabled => { Alpha = disabled ? 0.3f : 1; };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            BackgroundUnfocused = Color4.Black.Opacity(0.5f);
            BackgroundFocused = OsuColour.Gray(0.3f).Opacity(0.8f);
            BackgroundCommit = BorderColour = colour.Yellow;
        }

        protected override Color4 SelectionColour => new Color4(249, 90, 255, 255);

        protected override void OnFocus(FocusEvent e)
        {
            BorderThickness = 3;
            base.OnFocus(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            BorderThickness = 0;

            base.OnFocusLost(e);
        }

        protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: CalculatedTextSize) };

        protected override Caret CreateCaret() => new OsuCaret
        {
            CaretWidth = CaretWidth,
            SelectionColour = SelectionColour,
        };

        private class OsuCaret : Caret
        {
            private const float caret_move_time = 60;

            private readonly CaretBeatSyncedContainer beatSync;

            public OsuCaret()
            {
                RelativeSizeAxes = Axes.Y;
                Size = new Vector2(1, 0.9f);

                Colour = Color4.Transparent;
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                Masking = true;
                CornerRadius = 1;
                InternalChild = beatSync = new CaretBeatSyncedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }

            public override void Hide() => this.FadeOut(200);

            public float CaretWidth { get; set; }

            public Color4 SelectionColour { get; set; }

            public override void DisplayAt(Vector2 position, float? selectionWidth)
            {
                beatSync.HasSelection = selectionWidth != null;

                if (selectionWidth != null)
                {
                    this.MoveTo(new Vector2(position.X, position.Y), 60, Easing.Out);
                    this.ResizeWidthTo(selectionWidth.Value + CaretWidth / 2, caret_move_time, Easing.Out);
                    this.FadeColour(SelectionColour, 200, Easing.Out);
                }
                else
                {
                    this.MoveTo(new Vector2(position.X - CaretWidth / 2, position.Y), 60, Easing.Out);
                    this.ResizeWidthTo(CaretWidth, caret_move_time, Easing.Out);
                    this.FadeColour(Color4.White, 200, Easing.Out);
                }
            }

            private class CaretBeatSyncedContainer : BeatSyncedContainer
            {
                private bool hasSelection;

                public bool HasSelection
                {
                    set
                    {
                        hasSelection = value;
                        if (value)

                            this.FadeTo(0.5f, 200, Easing.Out);
                    }
                }

                public CaretBeatSyncedContainer()
                {
                    MinimumBeatLength = 300;
                    InternalChild = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    };
                }

                protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
                {
                    if (!hasSelection)
                        this.FadeTo(0.7f).FadeTo(0.4f, timingPoint.BeatLength, Easing.InOutSine);
                }
            }
        }
    }
}
