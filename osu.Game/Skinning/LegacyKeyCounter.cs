// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning
{
    public partial class LegacyKeyCounter : KeyCounter
    {
        private const float transition_duration = 160;

        public Colour4 ActiveColour { get; set; }

        private Colour4 textColour;

        public Colour4 TextColour
        {
            get => textColour;
            set
            {
                textColour = value;
                initialNameText.Colour = value;
                overlayKeyText.Colour = value;
            }
        }

        private readonly Container keyContainer;
        private readonly LegacySpriteText overlayKeyText;
        private readonly OsuSpriteText initialNameText;

        private readonly Sprite keySprite;

        private bool activatedOnce;

        public LegacyKeyCounter(InputTrigger trigger)
            : base(trigger)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Child = keyContainer = new Container
            {
                AutoSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    keySprite = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new UprightAspectMaintainingContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            // The legacy font doesn't contain all the characters necessary to display placeholders.
                            // Keep things simple by using a normal font for this case.
                            initialNameText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = trigger.Name,
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                Colour = textColour,
                            },
                            overlayKeyText = new LegacySpriteText(LegacyFont.ScoreEntry)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Alpha = 0,
                                Colour = textColour,
                            }
                        }
                    },
                }
            };

            // matches longest dimension of default skin asset
            Height = Width = 46;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource source)
        {
            Texture? keyTexture = source.GetTexture(@"inputoverlay-key");

            if (keyTexture != null)
                keySprite.Texture = keyTexture;
        }

        protected override void Activate(bool forwardPlayback = true)
        {
            base.Activate(forwardPlayback);

            keyContainer.ScaleTo(0.75f, transition_duration, Easing.Out);
            keySprite.Colour = ActiveColour;

            overlayKeyText.Text = CountPresses.Value.ToString();

            if (forwardPlayback && !activatedOnce)
            {
                activatedOnce = true;
                initialNameText.FadeOut(transition_duration, Easing.Out);
                overlayKeyText.FadeIn(transition_duration, Easing.Out);
            }
        }

        protected override void Deactivate(bool forwardPlayback = true)
        {
            base.Deactivate(forwardPlayback);
            keyContainer.ScaleTo(1f, transition_duration, Easing.Out);
            keySprite.Colour = Colour4.White;

            if (!forwardPlayback && activatedOnce && CountPresses.Value == 0)
            {
                activatedOnce = false;
                initialNameText.FadeIn(transition_duration, Easing.Out);
                overlayKeyText.FadeOut(transition_duration, Easing.Out);
            }
        }
    }
}
