// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.MedalSplash
{
    public class DrawableMedal : Container, IStateful<DisplayState>
    {
        private const float scale_when_unlocked = 0.76f;
        private const float scale_when_full = 0.6f;

        public event Action<DisplayState> StateChanged;

        private readonly Medal medal;
        private readonly Container medalContainer;
        private readonly Sprite medalSprite, medalGlow;
        private readonly OsuSpriteText unlocked, name;
        private readonly TextFlowContainer description;
        private DisplayState state;
        public DrawableMedal(Medal medal)
        {
            this.medal = medal;
            Position = new Vector2(0f, MedalOverlay.DISC_SIZE / 2);

            FillFlowContainer infoFlow;
            Children = new Drawable[]
            {
                medalContainer = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Alpha = 0f,
                    Children = new Drawable[]
                    {
                        medalSprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(0.81f),
                        },
                        medalGlow = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    },
                },
                unlocked = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = "Medal Unlocked".ToUpper(),
                    TextSize = 24,
                    Font = @"Exo2.0-Light",
                    Alpha = 0f,
                    Scale = new Vector2(1f / scale_when_unlocked),
                },
                infoFlow = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.6f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 5f),
                    Children = new Drawable[]
                    {
                        name = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = medal.Name,
                            TextSize = 20,
                            Font = @"Exo2.0-Bold",
                            Alpha = 0f,
                            Scale = new Vector2(1f / scale_when_full),
                        },
                        description = new OsuTextFlowContainer
                        {
                            TextAnchor = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0f,
                            Scale = new Vector2(1f / scale_when_full),
                        },
                    },
                },
            };

            description.AddText(medal.Description, s =>
            {
                s.Anchor = Anchor.TopCentre;
                s.Origin = Anchor.TopCentre;
                s.TextSize = 16;
            });

            medalContainer.OnLoadComplete = d =>
            {
                unlocked.Position = new Vector2(0f, medalContainer.DrawSize.Y / 2 + 10);
                infoFlow.Position = new Vector2(0f, unlocked.Position.Y + 90);
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures)
        {
            medalSprite.Texture = textures.Get(medal.ImageUrl);
            medalGlow.Texture = textures.Get(@"MedalSplash/medal-glow");
            description.Colour = colours.BlueLight;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
        }

        public DisplayState State
        {
            get { return state; }
            set
            {
                if (state == value) return;

                state = value;
                updateState();

                StateChanged?.Invoke(State);
            }
        }

        private void updateState()
        {
            if (!IsLoaded) return;

            const double duration = 900;

            switch (state)
            {
                case DisplayState.None:
                    medalContainer.ScaleTo(0);
                    break;
                case DisplayState.Icon:
                    medalContainer
                        .FadeIn(duration)
                        .ScaleTo(1, duration, Easing.OutElastic);
                    break;
                case DisplayState.MedalUnlocked:
                    medalContainer
                        .FadeTo(1)
                        .ScaleTo(1);

                    this.ScaleTo(scale_when_unlocked, duration, Easing.OutExpo);
                    this.MoveToY(MedalOverlay.DISC_SIZE / 2 - 30, duration, Easing.OutExpo);
                    unlocked.FadeInFromZero(duration);
                    break;
                case DisplayState.Full:
                    medalContainer
                        .FadeTo(1)
                        .ScaleTo(1);

                    this.ScaleTo(scale_when_full, duration, Easing.OutExpo);
                    this.MoveToY(MedalOverlay.DISC_SIZE / 2 - 60, duration, Easing.OutExpo);
                    unlocked.Show();
                    name.FadeInFromZero(duration + 100);
                    description.FadeInFromZero(duration * 2);
                    break;
            }


        }
    }

    public enum DisplayState
    {
        None,
        Icon,
        MedalUnlocked,
        Full,
    }
}
