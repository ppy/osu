// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
    public partial class OsuTextBox : BasicTextBox
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

        private OsuCaret? caret;

        private bool selectionStarted;
        private double sampleLastPlaybackTime;

        protected enum FeedbackSampleType
        {
            TextAdd,
            TextAddCaps,
            TextRemove,
            TextConfirm,
            TextInvalid,
            CaretMove,
            SelectCharacter,
            SelectWord,
            SelectAll,
            Deselect
        }

        private Dictionary<FeedbackSampleType, Sample?[]> sampleMap = new Dictionary<FeedbackSampleType, Sample?[]>();

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

            // Note that `KeyBindingRow` uses similar logic for input feedback, so remember to update there if changing here.
            var textAddedSamples = new Sample?[4];
            for (int i = 0; i < textAddedSamples.Length; i++)
                textAddedSamples[i] = audio.Samples.Get($@"Keyboard/key-press-{1 + i}");

            sampleMap = new Dictionary<FeedbackSampleType, Sample?[]>
            {
                { FeedbackSampleType.TextAdd, textAddedSamples },
                { FeedbackSampleType.TextAddCaps, new[] { audio.Samples.Get(@"Keyboard/key-caps") } },
                { FeedbackSampleType.TextRemove, new[] { audio.Samples.Get(@"Keyboard/key-delete") } },
                { FeedbackSampleType.TextConfirm, new[] { audio.Samples.Get(@"Keyboard/key-confirm") } },
                { FeedbackSampleType.TextInvalid, new[] { audio.Samples.Get(@"Keyboard/key-invalid") } },
                { FeedbackSampleType.CaretMove, new[] { audio.Samples.Get(@"Keyboard/key-movement") } },
                { FeedbackSampleType.SelectCharacter, new[] { audio.Samples.Get(@"Keyboard/select-char") } },
                { FeedbackSampleType.SelectWord, new[] { audio.Samples.Get(@"Keyboard/select-word") } },
                { FeedbackSampleType.SelectAll, new[] { audio.Samples.Get(@"Keyboard/select-all") } },
                { FeedbackSampleType.Deselect, new[] { audio.Samples.Get(@"Keyboard/deselect") } }
            };
        }

        private Color4 selectionColour;

        protected override Color4 SelectionColour => selectionColour;

        protected override void OnUserTextAdded(string added)
        {
            base.OnUserTextAdded(added);

            if (!added.Any(CanAddCharacter))
                return;

            if (added.Any(char.IsUpper) && AllowUniqueCharacterSamples)
                PlayFeedbackSample(FeedbackSampleType.TextAddCaps);
            else
                PlayFeedbackSample(FeedbackSampleType.TextAdd);
        }

        protected override void OnUserTextRemoved(string removed)
        {
            base.OnUserTextRemoved(removed);

            PlayFeedbackSample(FeedbackSampleType.TextRemove);
        }

        protected override void NotifyInputError()
        {
            base.NotifyInputError();

            PlayFeedbackSample(FeedbackSampleType.TextInvalid);
        }

        protected override void OnTextCommitted(bool textChanged)
        {
            base.OnTextCommitted(textChanged);

            PlayFeedbackSample(FeedbackSampleType.TextConfirm);
        }

        protected override void OnCaretMoved(bool selecting)
        {
            base.OnCaretMoved(selecting);

            if (!selecting)
                PlayFeedbackSample(FeedbackSampleType.CaretMove);
        }

        protected override void OnTextSelectionChanged(TextSelectionType selectionType)
        {
            base.OnTextSelectionChanged(selectionType);

            switch (selectionType)
            {
                case TextSelectionType.Character:
                    PlayFeedbackSample(FeedbackSampleType.SelectCharacter);
                    break;

                case TextSelectionType.Word:
                    PlayFeedbackSample(selectionStarted ? FeedbackSampleType.SelectCharacter : FeedbackSampleType.SelectWord);
                    break;

                case TextSelectionType.All:
                    PlayFeedbackSample(FeedbackSampleType.SelectAll);
                    break;
            }

            selectionStarted = true;
        }

        protected override void OnTextDeselected()
        {
            base.OnTextDeselected();

            if (!selectionStarted) return;

            PlayFeedbackSample(FeedbackSampleType.Deselect);

            selectionStarted = false;
        }

        protected override void OnImeComposition(string newComposition, int removedTextLength, int addedTextLength, bool caretMoved)
        {
            base.OnImeComposition(newComposition, removedTextLength, addedTextLength, caretMoved);

            if (string.IsNullOrEmpty(newComposition))
            {
                switch (removedTextLength)
                {
                    case 0:
                        // empty composition event, composition wasn't changed, don't play anything.
                        return;

                    case 1:
                        // composition probably ended by pressing backspace, or was cancelled.
                        PlayFeedbackSample(FeedbackSampleType.TextRemove);
                        return;

                    default:
                        // longer text removed, composition ended because it was cancelled.
                        // could be a different sample if desired.
                        PlayFeedbackSample(FeedbackSampleType.TextRemove);
                        return;
                }
            }

            if (addedTextLength > 0)
            {
                // some text was added, probably due to typing new text or by changing the candidate.
                PlayFeedbackSample(FeedbackSampleType.TextAdd);
                return;
            }

            if (removedTextLength > 0)
            {
                // text was probably removed by backspacing.
                // it's also possible that a candidate that only removed text was changed to.
                PlayFeedbackSample(FeedbackSampleType.TextRemove);
                return;
            }

            if (caretMoved)
            {
                // only the caret/selection was moved.
                PlayFeedbackSample(FeedbackSampleType.CaretMove);
            }
        }

        protected override void OnImeResult(string result, bool successful)
        {
            base.OnImeResult(result, successful);

            if (successful)
            {
                // composition was successfully completed, usually by pressing the enter key.
                PlayFeedbackSample(FeedbackSampleType.TextConfirm);
            }
            else
            {
                // composition was prematurely ended, eg. by clicking inside the textbox.
                // could be a different sample if desired.
                PlayFeedbackSample(FeedbackSampleType.TextConfirm);
            }
        }

        protected override void OnFocus(FocusEvent e)
        {
            if (Masking)
                BorderThickness = 3;

            base.OnFocus(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            if (Masking)
                BorderThickness = 0;

            base.OnFocusLost(e);
        }

        protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
        {
            AutoSizeAxes = Axes.Both,
            Child = new OsuSpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: FontSize) },
        };

        protected override Caret CreateCaret() => caret = new OsuCaret
        {
            CaretWidth = CaretWidth,
            SelectionColour = SelectionColour,
        };

        private SampleChannel? getSampleChannel(FeedbackSampleType feedbackSampleType)
        {
            var samples = sampleMap[feedbackSampleType];

            if (samples.Length == 0)
                return null;

            return samples[RNG.Next(0, samples.Length)]?.GetChannel();
        }

        protected void PlayFeedbackSample(FeedbackSampleType feedbackSample) => Schedule(() =>
        {
            if (Time.Current < sampleLastPlaybackTime + 15) return;

            SampleChannel? channel = getSampleChannel(feedbackSample);

            if (channel == null) return;

            double pitch = 0.98 + RNG.NextDouble(0.04);

            if (feedbackSample == FeedbackSampleType.SelectCharacter)
                pitch += ((double)SelectedText.Length / Math.Max(1, Text.Length)) * 0.15f;

            channel.Frequency.Value = pitch;
            channel.Play();

            sampleLastPlaybackTime = Time.Current;
        });

        private partial class OsuCaret : Caret
        {
            private const float caret_move_time = 60;

            private readonly CaretBeatSyncedContainer beatSync;

            public OsuCaret()
            {
                Colour = Color4.Transparent;

                InternalChild = beatSync = new CaretBeatSyncedContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Masking = true,
                    CornerRadius = 1f,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.9f,
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

            private partial class CaretBeatSyncedContainer : BeatSyncedContainer
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
