// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public abstract partial class RoomSettingsOverlay : FocusedOverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        protected const float TRANSITION_DURATION = 350;
        protected const float FIELD_PADDING = 25;

        protected OnlinePlayComposite Settings { get; set; }

        protected override bool BlockScrollInput => false;

        protected abstract OsuButton SubmitButton { get; }

        protected abstract bool IsLoading { get; }

        private readonly Room room;

        protected RoomSettingsOverlay(Room room)
        {
            this.room = room;

            RelativeSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(Settings = CreateSettings(room));
        }

        protected abstract void SelectBeatmap();

        protected abstract OnlinePlayComposite CreateSettings(Room room);

        protected override void PopIn()
        {
            Settings.MoveToY(0, TRANSITION_DURATION, Easing.OutQuint);
            Settings.FadeIn(TRANSITION_DURATION / 2);
        }

        protected override void PopOut()
        {
            Settings.MoveToY(-1, TRANSITION_DURATION, Easing.InSine);
            Settings.Delay(TRANSITION_DURATION / 2).FadeOut(TRANSITION_DURATION / 2);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Select:
                    if (IsLoading)
                        return true;

                    if (SubmitButton.Enabled.Value)
                    {
                        SubmitButton.TriggerClick();
                        return true;
                    }
                    else
                    {
                        SelectBeatmap();
                        return true;
                    }
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        /// <remarks>
        /// <see cref="ReverseChildIDFillFlowContainer{T}"/> is used to ensure that if the nested <see cref="Section"/>s
        /// use expanded overhanging content (like an <see cref="OsuDropdown{T}"/>'s dropdown),
        /// then the overhanging content will be correctly Z-ordered.
        /// </remarks>
        protected partial class SectionContainer : ReverseChildIDFillFlowContainer<Section>
        {
            public SectionContainer()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Width = 0.5f;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(FIELD_PADDING);
            }
        }

        protected partial class Section : Container
        {
            private readonly ReverseChildIDFillFlowContainer<Drawable> content;

            protected override Container<Drawable> Content => content;

            public Section(string title)
            {
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 12),
                            Text = title.ToUpperInvariant(),
                        },
                        content = new ReverseChildIDFillFlowContainer<Drawable>
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FillDirection.Vertical
                        },
                    },
                };
            }
        }
    }
}
