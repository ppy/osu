// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public abstract class MatchSettingsOverlay : FocusedOverlayContainer
    {
        protected const float TRANSITION_DURATION = 350;
        protected const float FIELD_PADDING = 45;

        protected OnlinePlayComposite Settings { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
        }

        protected override void PopIn()
        {
            Settings.MoveToY(0, TRANSITION_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            Settings.MoveToY(-1, TRANSITION_DURATION, Easing.InSine);
        }

        protected class SettingsTextBox : OsuTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundUnfocused = Color4.Black;
                BackgroundFocused = Color4.Black;
            }
        }

        protected class SettingsNumberTextBox : SettingsTextBox
        {
            protected override bool CanAddCharacter(char character) => char.IsNumber(character);
        }

        protected class SettingsPasswordTextBox : OsuPasswordTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundUnfocused = Color4.Black;
                BackgroundFocused = Color4.Black;
            }
        }

        protected class SectionContainer : FillFlowContainer<Section>
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

        protected class Section : Container
        {
            private readonly Container content;

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
                            Text = title.ToUpper(),
                        },
                        content = new Container
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                };
            }
        }
    }
}
