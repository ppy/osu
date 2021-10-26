// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTextBox : BasicTextBox
    {
        /// <summary>
        /// Whether to allow playing a different samples based on the type of character.
        /// If set to false, the same sample will be used for all characters.
        /// </summary>
        protected virtual bool AllowUniqueCharacterSamples => true;

        protected override float LeftRightPadding => 10;

        protected override float CaretWidth => 3;

        protected override SpriteText CreatePlaceholder() => new OsuSpriteText
        {
            Font = OsuFont.GetFont(italics: true),
            Margin = new MarginPadding { Left = 2 },
        };

        private readonly Sample?[] textAddedSamples = new Sample[4];
        private Sample? capsTextAddedSample;
        private Sample? textRemovedSample;
        private Sample? textCommittedSample;
        private Sample? caretMovedSample;

        private OsuCaret? caret;

        public OsuTextBox()
        {
            Height = 40;
            TextContainer.Height = 0.5f;
            CornerRadius = 5;
            LengthLimit = 1000;

            Current.DisabledChanged += disabled => { Alpha = disabled ? 0.3f : 1; };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider, OsuColour colour, AudioManager audio)
        {
            BackgroundUnfocused = colourProvider?.Background5 ?? Color4.Black.Opacity(0.5f);
            BackgroundFocused = colourProvider?.Background4 ?? OsuColour.Gray(0.3f).Opacity(0.8f);
            BackgroundCommit = BorderColour = colourProvider?.Highlight1 ?? colour.Yellow;
            selectionColour = colourProvider?.Background1 ?? new Color4(249, 90, 255, 255);

            if (caret != null)
                caret.SelectionColour = selectionColour;

            Placeholder.Colour = colourProvider?.Foreground1 ?? new Color4(180, 180, 180, 255);

            for (int i = 0; i < textAddedSamples.Length; i++)
                textAddedSamples[i] = audio.Samples.Get($@"Keyboard/key-press-{1 + i}");

            capsTextAddedSample = audio.Samples.Get(@"Keyboard/key-caps");
            textRemovedSample = audio.Samples.Get(@"Keyboard/key-delete");
            textCommittedSample = audio.Samples.Get(@"Keyboard/key-confirm");
            caretMovedSample = audio.Samples.Get(@"Keyboard/key-movement");
        }

        private Color4 selectionColour;

        protected override Color4 SelectionColour => selectionColour;

        protected override void OnUserTextAdded(string added)
        {
            base.OnUserTextAdded(added);

            if (added.Any(char.IsUpper) && AllowUniqueCharacterSamples)
                capsTextAddedSample?.Play();
            else
                textAddedSamples[RNG.Next(0, 3)]?.Play();
        }

        protected override void OnUserTextRemoved(string removed)
        {
            base.OnUserTextRemoved(removed);

            textRemovedSample?.Play();
        }

        protected override void OnTextCommitted(bool textChanged)
        {
            base.OnTextCommitted(textChanged);

            textCommittedSample?.Play();
        }

        protected override void OnCaretMoved(bool selecting)
        {
            base.OnCaretMoved(selecting);

            caretMovedSample?.Play();
        }

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

        protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
        {
            AutoSizeAxes = Axes.Both,
            Child = new OsuSpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: CalculatedTextSize) },
        };

        protected override Caret CreateCaret() => caret = new OsuCaret
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

                protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
                {
                    if (!hasSelection)
                        this.FadeTo(0.7f).FadeTo(0.4f, timingPoint.BeatLength, Easing.InOutSine);
                }
            }
        }
    }
}
