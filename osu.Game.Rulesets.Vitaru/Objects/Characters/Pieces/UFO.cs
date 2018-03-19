using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Vitaru.Objects.Characters.Pieces
{
    public class UFO : BeatSyncedContainer
    {
        public readonly VitaruPlayer ParentNue;
        public readonly UFOType UFOType;
        private readonly Color4 color;

        public VitaruPlayer AttachedPlayer;

        public UFO(VitaruPlayer player, UFOType type)
        {
            ParentNue = player;
            UFOType = type;
            AttachedPlayer = player;

            switch (type)
            {
                case UFOType.Mark:
                    color = Color4.Purple;
                    break;
                case UFOType.Health:
                    color = Color4.Green;
                    break;
                case UFOType.Energy:
                    color = Color4.Blue;
                    break;
                case UFOType.Damage:
                    color = Color4.Red;
                    break;
            }

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Masking = true;
            Colour = color;
            Size = new Vector2(10);
            CornerRadius = Size.X / 3;
            Alpha = 0.5f;

            Child = new Box
            {
                RelativeSizeAxes = Axes.Both
            };

            EdgeEffect = new EdgeEffectParameters
            {
                Radius = Width,
                Colour = color.Opacity(0.5f)
            };
        }
    }

    public enum UFOType
    {
        Mark,
        Health,
        Energy,
        Damage
    }
}
