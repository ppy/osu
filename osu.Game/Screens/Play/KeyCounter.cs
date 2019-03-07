// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public abstract class KeyCounter : Container
    {
        private Sprite buttonSprite;
        private Sprite glowSprite;
        private Container textLayer;
        private SpriteText countSpriteText;

        private readonly List<KeyCounterState> states = new List<KeyCounterState>();
        private KeyCounterState currentState;

        public bool IsCounting { get; set; } = true;
        private int countPresses;

        public int CountPresses
        {
            get => countPresses;
            private set
            {
                if (countPresses != value)
                {
                    countPresses = value;
                    countSpriteText.Text = value.ToString(@"#,0");
                }
            }
        }

        private bool isLit;

        public bool IsLit
        {
            get => isLit;
            protected set
            {
                if (isLit != value)
                {
                    isLit = value;
                    updateGlowSprite(value);
                    if (value && IsCounting)
                    {
                        CountPresses++;
                        saveState();
                    }
                }
            }
        }

        //further: change default values here and in KeyCounterCollection if needed, instead of passing them in every constructor
        public Color4 KeyDownTextColor { get; set; } = Color4.DarkGray;
        public Color4 KeyUpTextColor { get; set; } = Color4.White;
        public int FadeTime { get; set; }

        protected KeyCounter(string name)
        {
            Name = name;
        }

        [BackgroundDependencyLoader(true)]
        private void load(TextureStore textures, GameplayClock clock)
        {
            if (clock != null)
                Clock = clock;

            Children = new Drawable[]
            {
                buttonSprite = new Sprite
                {
                    Texture = textures.Get(@"KeyCounter/key-up"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                glowSprite = new Sprite
                {
                    Texture = textures.Get(@"KeyCounter/key-glow"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0
                },
                textLayer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = Name,
                            Font = OsuFont.Numeric.With(size: 12),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Both,
                            Position = new Vector2(0, -0.25f),
                            Colour = KeyUpTextColor
                        },
                        countSpriteText = new OsuSpriteText
                        {
                            Text = CountPresses.ToString(@"#,0"),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Both,
                            Position = new Vector2(0, 0.25f),
                            Colour = KeyUpTextColor
                        }
                    }
                }
            };
            //Set this manually because an element with Alpha=0 won't take it size to AutoSizeContainer,
            //so the size can be changing between buttonSprite and glowSprite.
            Height = buttonSprite.DrawHeight;
            Width = buttonSprite.DrawWidth;
        }

        private void updateGlowSprite(bool show)
        {
            if (show)
            {
                glowSprite.FadeIn(FadeTime);
                textLayer.FadeColour(KeyDownTextColor, FadeTime);
            }
            else
            {
                glowSprite.FadeOut(FadeTime);
                textLayer.FadeColour(KeyUpTextColor, FadeTime);
            }
        }

        public void ResetCount()
        {
            CountPresses = 0;
            states.Clear();
        }

        protected override void Update()
        {
            base.Update();

            if (currentState?.Time > Clock.CurrentTime)
                restoreStateTo(Clock.CurrentTime);
        }

        private void saveState()
        {
            if (currentState == null || currentState.Time < Clock.CurrentTime)
                states.Add(currentState = new KeyCounterState(Clock.CurrentTime, CountPresses));
        }

        private void restoreStateTo(double time)
        {
            states.RemoveAll(state => state.Time > time);

            currentState = states.LastOrDefault();
            CountPresses = currentState?.Count ?? 0;
        }
    }
}
