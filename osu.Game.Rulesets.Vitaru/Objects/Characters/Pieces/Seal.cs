using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Extensions.Color4Extensions;
using OpenTK;

namespace osu.Game.Rulesets.Vitaru.Objects.Characters.Pieces
{
    public class Seal : Container
    {
        private readonly Container characterSigil;

        private readonly VitaruPlayer vitaruPlayer;

        private readonly Sprite gear1;
        private readonly Sprite gear2;
        private readonly Sprite gear3;
        private readonly Sprite gear4;
        private readonly Sprite gear5;

        public Seal(VitaruPlayer character)
        {
            vitaruPlayer = character;

            Color4 lightColor = vitaruPlayer.CharacterColor.Lighten(0.5f);
            Color4 darkColor = vitaruPlayer.CharacterColor.Darken(0.5f);

            Scale = new Vector2(0.6f);

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AlwaysPresent = true;
            Alpha = 0.5f;

            Children = new Drawable[]
            {
                characterSigil = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    Size = new Vector2(90),
                    CornerRadius = 45
                },
                new Sprite
                {
                    Colour = vitaruPlayer.CharacterColor,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = VitaruRuleset.VitaruTextures.Get("seal"),
                },
            };

            switch (vitaruPlayer.CurrentCharacter)
            {
                default:
                    break;
                case Characters.SakuyaIzayoi:
                    characterSigil.Children = new Drawable[]
                    {
                        gear1 = new Sprite
                        {
                            Colour = lightColor,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = VitaruRuleset.VitaruTextures.Get("gearSmall"),
                            Position = new Vector2(-41, 10),
                        },
                        gear2 = new Sprite
                        {
                            Colour = vitaruPlayer.CharacterColor,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = VitaruRuleset.VitaruTextures.Get("gearMedium"),
                            Position = new Vector2(-4, 16),
                        },
                        gear3 = new Sprite
                        {
                            Colour = darkColor,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = VitaruRuleset.VitaruTextures.Get("gearLarge"),
                            Position = new Vector2(-16, -34),
                        },
                        gear4 = new Sprite
                        {
                            Colour = vitaruPlayer.CharacterColor,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = VitaruRuleset.VitaruTextures.Get("gearMedium"),
                            Position = new Vector2(35, -40),
                        },
                        gear5 = new Sprite
                        {
                            Colour = lightColor,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = VitaruRuleset.VitaruTextures.Get("gearSmall"),
                            Position = new Vector2(33, 8),
                        },
                    };
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            this.RotateTo((float)(-Clock.CurrentTime / 1000 * 90) * 0.1f);

            switch (vitaruPlayer.CurrentCharacter)
            {
                default:
                    break;
                case Characters.SakuyaIzayoi:
                    float speed = 0.25f;
                    gear1.RotateTo((float)(Clock.CurrentTime / 1000 * 90) * 1.25f * speed);
                    gear2.RotateTo((float)(-Clock.CurrentTime / 1000 * 90) * 1.1f * speed);
                    gear3.RotateTo((float)(Clock.CurrentTime / 1000 * 90) * speed);
                    gear4.RotateTo((float)(-Clock.CurrentTime / 1000 * 90) * 1.1f * speed);
                    gear5.RotateTo((float)(Clock.CurrentTime / 1000 * 90) * 1.25f * speed);
                    break;
            }
        }
    }
}
