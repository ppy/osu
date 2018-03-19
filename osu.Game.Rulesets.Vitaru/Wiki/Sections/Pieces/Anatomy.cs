using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using osu.Game.Rulesets.Vitaru.Settings;
using Symcol.Core.GameObjects;

namespace osu.Game.Rulesets.Vitaru.Wiki.Sections.Pieces
{
    /// <summary>
    /// The insides of the player, only used by the wiki
    /// </summary>
    public class Anatomy : Container
    {
        private readonly Bindable<Characters> selectedCharacter = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.Characters);

        private readonly Sprite characterSprite;
        private readonly CircularContainer hitbox;
        private EdgeEffectParameters edgeEffectParameters;

        public Anatomy()
        {
            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreRight;
            AutoSizeAxes = Axes.Both;

            edgeEffectParameters = new EdgeEffectParameters
            {
                Radius = 4,
                Type = EdgeEffectType.Shadow
            };

            Children = new Drawable[]
            {
                characterSprite = new Sprite
                {
                    Scale = new Vector2(2),
                    Position = new Vector2(-10, 0),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight
                },
                hitbox = new CircularContainer
                {
                    Position = new Vector2(-4, 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(2),
                    Size = new Vector2(4),
                    BorderThickness = 4 / 3,
                    Masking = true,

                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    EdgeEffect = edgeEffectParameters
                }
            };

            selectedCharacter.ValueChanged += character =>
            {
                restart:

                characterSprite.Texture = VitaruRuleset.VitaruTextures.Get("playerKiai");
                characterSprite.Colour = Color4.White;
                hitbox.BorderColour = Color4.White;
                edgeEffectParameters.Colour = Color4.White.Opacity(0.5f);

                switch (character)
                {
                    /*
                    case Characters.Alex:
                        characterSprite.Colour = Color4.Gold;
                        hitbox.BorderColour = Color4.Gold;
                        edgeEffectParameters.Colour = Color4.Gold.Opacity(0.5f);
                        break;
                        */
                    case Characters.ReimuHakurei:
                        characterSprite.Texture = VitaruRuleset.VitaruTextures.Get("reimuKiai");
                        hitbox.BorderColour = Color4.Red;
                        edgeEffectParameters.Colour = Color4.Red.Opacity(0.5f);
                        break;
                    case Characters.MarisaKirisame:
                        characterSprite.Texture = VitaruRuleset.VitaruTextures.Get("marisaKiai");
                        hitbox.BorderColour = Color4.Black;
                        edgeEffectParameters.Colour = Color4.Black.Opacity(0.5f);
                        break;
                    case Characters.SakuyaIzayoi:
                        characterSprite.Texture = VitaruRuleset.VitaruTextures.Get("sakuyaKiai");
                        hitbox.BorderColour = Color4.Navy;
                        edgeEffectParameters.Colour = Color4.Navy.Opacity(0.5f);
                        break;
                    case Characters.FlandreScarlet:
                        characterSprite.Colour = Color4.Red;
                        hitbox.BorderColour = Color4.Red;
                        edgeEffectParameters.Colour = Color4.Red.Opacity(0.5f);
                        break;
                    case Characters.RemiliaScarlet:
                        characterSprite.Colour = Color4.Pink;
                        hitbox.BorderColour = Color4.Pink;
                        edgeEffectParameters.Colour = Color4.Pink.Opacity(0.5f);
                        break;
                    case Characters.Cirno:
                        characterSprite.Colour = Color4.Blue;
                        hitbox.BorderColour = Color4.Blue;
                        edgeEffectParameters.Colour = Color4.Blue.Opacity(0.5f);
                        break;
                    case Characters.TenshiHinanai:
                        characterSprite.Colour = Color4.DarkBlue;
                        hitbox.BorderColour = Color4.DarkBlue;
                        edgeEffectParameters.Colour = Color4.DarkBlue.Opacity(0.5f);
                        break;
                    case Characters.YukariYakumo:
                        characterSprite.Colour = Color4.LightBlue;
                        hitbox.BorderColour = Color4.LightBlue;
                        edgeEffectParameters.Colour = Color4.LightBlue.Opacity(0.5f);
                        break;
                    case Characters.Chen:
                        characterSprite.Texture = VitaruRuleset.VitaruTextures.Get("chenKiai");
                        hitbox.BorderColour = Color4.Green;
                        edgeEffectParameters.Colour = Color4.Green.Opacity(0.5f);
                        break;
                    case Characters.Kaguya:
                        characterSprite.Texture = VitaruRuleset.VitaruTextures.Get("kaguyaKiai");
                        hitbox.BorderColour = Color4.DarkRed;
                        edgeEffectParameters.Colour = Color4.DarkRed.Opacity(0.5f);
                        break;
                    case Characters.IbarakiKasen:
                        characterSprite.Colour = Color4.YellowGreen;
                        hitbox.BorderColour = Color4.YellowGreen;
                        edgeEffectParameters.Colour = Color4.YellowGreen.Opacity(0.5f);
                        break;
                    case Characters.NueHoujuu:
                        characterSprite.Texture = VitaruRuleset.VitaruTextures.Get("nueKiai");
                        hitbox.BorderColour = Color4.DarkGray;
                        edgeEffectParameters.Colour = Color4.DarkGray.Opacity(0.5f);
                        break;
                    case Characters.AliceMuyart:
                        if (!VitaruAPIContainer.Shawdooow)
                        {
                            selectedCharacter.Value = Characters.ReimuHakurei;
                            character = Characters.ReimuHakurei;
                            goto restart;
                        }
                        characterSprite.Colour = Color4.SkyBlue;
                        hitbox.BorderColour = Color4.SkyBlue;
                        edgeEffectParameters.Colour = Color4.SkyBlue.Opacity(0.5f);
                        break;
                    case Characters.ArysaMuyart:
                        if (!VitaruAPIContainer.Shawdooow)
                        {
                            selectedCharacter.Value = Characters.ReimuHakurei;

                            character = Characters.ReimuHakurei;
                            goto restart;
                        }
                        characterSprite.Colour = Color4.LightGreen;
                        hitbox.BorderColour = Color4.LightGreen;
                        edgeEffectParameters.Colour = Color4.LightGreen.Opacity(0.5f);
                        break;
                }
            };
            selectedCharacter.TriggerChange();
        }
    }
}
