// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A <see cref="Container{T}"/> which mimics behavior of a vertical <see cref="FillFlowContainer"/> full of clickable items in an efficient way.
    /// </summary>
    /// <remarks>
    /// This component assumes that clickable items have transparent background while in idle state.
    /// </remarks>
    public partial class EditorTableBackground : Container<EditorTableBackgroundRow>
    {
        public Action<int>? Selected;

        public const float ROW_HEIGHT = 25;

        private int rowCount;

        public int RowCount
        {
            get => rowCount;
            set
            {
                rowCount = value;
                Height = rowCount * ROW_HEIGHT;
                Deselect();
            }
        }

        private readonly Bindable<int> hoveredIndexBindable = new Bindable<int>(-1);

        private readonly HoverSampleSet sampleSet;
        private Bindable<double?> lastHoverPlaybackTime = null!;

        public EditorTableBackground(HoverSampleSet sampleSet = HoverSampleSet.Default)
        {
            this.sampleSet = sampleSet;
        }

        private Sample? sampleHover;
        private Sample? sampleClick;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, SessionStatics statics)
        {
            // see HoverSampleDebounceComponent
            lastHoverPlaybackTime = statics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime);

            // see HoverSounds
            sampleHover = audio.Samples.Get($@"UI/{sampleSet.GetDescription()}-hover")
                          ?? audio.Samples.Get($@"UI/{HoverSampleSet.Default.GetDescription()}-hover");

            // see HoverClickSounds
            sampleClick = audio.Samples.Get($@"UI/{sampleSet.GetDescription()}-select")
                          ?? audio.Samples.Get($@"UI/{HoverSampleSet.Default.GetDescription()}-select");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            hoveredIndexBindable.BindValueChanged(i => onHoveredChanged(i.NewValue));
        }

        protected override void Update()
        {
            base.Update();

            if (IsHovered)
                hoveredIndexBindable.Value = getItemIndexAtMousePosition();
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            hoveredIndexBindable.Value = -1;
        }

        private void onHoveredChanged(int hoveredIndex)
        {
            EditorTableBackgroundRow? newHovered = null;

            foreach (var child in this)
            {
                if (child.Index == hoveredIndex)
                {
                    newHovered = child;
                }
                else
                {
                    if (child.State == RowState.Hovered)
                        child.State = RowState.None;
                }
            }

            if (hoveredIndex == -1)
                return;

            if (newHovered != null)
            {
                if (newHovered.State == RowState.None)
                    newHovered.State = RowState.Hovered;
            }
            else
                Add(new EditorTableBackgroundRow(hoveredIndex));

            playHoverSample();
        }

        public EditorTableBackgroundRow Select(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= rowCount)
                throw new ArgumentOutOfRangeException(@$"Can't select item at index {itemIndex} when item count is {rowCount}.");

            EditorTableBackgroundRow? existingAtIndex = getItemAtIndex(itemIndex);

            if (existingAtIndex == null)
            {
                Deselect();

                var toAdd = new EditorTableBackgroundRow(itemIndex)
                {
                    State = RowState.Selected
                };

                Add(toAdd);
                return toAdd;
            }

            foreach (var child in this)
            {
                if (child.Index == itemIndex)
                    child.State = RowState.Selected;
                else
                    child.State = child.Index == hoveredIndexBindable.Value ? RowState.Hovered : RowState.None;
            }

            return existingAtIndex;
        }

        public void Deselect()
        {
            foreach (var child in this)
            {
                if (child.State == RowState.Selected)
                    child.State = child.Index == hoveredIndexBindable.Value ? RowState.Hovered : RowState.None;
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            base.OnClick(e);
            int index = getItemIndexAtMousePosition();

            if (index != -1)
            {
                Select(index);
                playClickSample();
                Selected?.Invoke(index);
            }

            return true;
        }

        private int getItemIndexAtMousePosition()
        {
            float y = ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position).Y;

            int index = (int)(y / ROW_HEIGHT);
            if (index >= 0 && index < RowCount)
                return index;

            return -1;
        }

        private EditorTableBackgroundRow? getItemAtIndex(int index)
        {
            foreach (var child in this)
            {
                if (child.Index == index)
                    return child;
            }

            return null;
        }

        private void playHoverSample()
        {
            if (sampleHover == null)
                return;

            bool enoughTimePassedSinceLastPlayback = !lastHoverPlaybackTime.Value.HasValue || Time.Current - lastHoverPlaybackTime.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;

            if (!enoughTimePassedSinceLastPlayback)
                return;

            sampleHover.Frequency.Value = 0.98 + RNG.NextDouble(0.04);
            sampleHover.Play();

            lastHoverPlaybackTime.Value = Time.Current;
        }

        private void playClickSample()
        {
            var channel = sampleClick?.GetChannel();

            if (channel == null)
                return;

            channel.Frequency.Value = 0.99 + RNG.NextDouble(0.02);
            channel.Play();
        }
    }

    public partial class EditorTableBackgroundRow : CompositeDrawable
    {
        private const int fade_duration = 100;
        private const int colour_duration = 450;

        private RowState state = RowState.Hovered;

        public RowState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                state = value;

                if (!IsLoaded)
                    return;

                updateState(state);
            }
        }

        public override bool RemoveWhenNotAlive => true;

        protected override bool ShouldBeAlive => base.ShouldBeAlive && Alpha > 0;

        public int Index { get; }

        public EditorTableBackgroundRow(int index)
        {
            Index = index;
        }

        private Color4 colourHover;
        private Color4 colourSelected;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            RelativeSizeAxes = Axes.X;
            Height = EditorTableBackground.ROW_HEIGHT;
            Y = Index * Height;
            Masking = true;
            CornerRadius = 3;
            Alpha = 0.0001f; // Make alpha slightly bigger to allow playing FadeIn transforms without being immediately removed from the tree.
            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both
            };

            colourHover = colours.Background1;
            colourSelected = colours.Colour3;

            Colour = state == RowState.Hovered ? colourHover : colourSelected;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState(state);
        }

        private void updateState(RowState newState)
        {
            switch (newState)
            {
                case RowState.None:
                    this.FadeOut(fade_duration, Easing.OutQuint);
                    this.FadeColour(colourHover, colour_duration, Easing.OutQuint);
                    break;

                case RowState.Hovered:
                    this.FadeIn(fade_duration, Easing.OutQuint);
                    this.FadeColour(colourHover, colour_duration, Easing.OutQuint);
                    break;

                case RowState.Selected:
                    this.FadeIn(fade_duration, Easing.OutQuint);
                    this.FadeColour(colourSelected, colour_duration, Easing.OutQuint);
                    break;
            }
        }
    }

    public enum RowState
    {
        None,
        Hovered,
        Selected
    }
}
