using osu.Game.Graphics.Backgrounds;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Configuration;
using osu.Game.Configuration;
using osu.Framework.Allocation;

namespace osu.Game.Modes.Osu.UI
{
    class OsuComboFire : Triangles
    {

        public int Combo { get; set; } = 0;
        private Bindable<bool> comboFireEnabled;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            comboFireEnabled = config.GetBindable<bool>(OsuConfig.ComboFire);
        }

        protected override void Update()
        {
            base.Update();

            if(Children.Count() > Combo || !comboFireEnabled)
                Children.Where((Triangle tri) => Children.ToList().IndexOf(tri) > Combo).ToList().ForEach((Triangle tri) => shrink(tri));
        }

        protected override Triangle CreateTriangle()
        {
            Triangle tri = base.CreateTriangle();
            shrink(tri,RNG.Next(3000) + 3000);
            return tri;
        }

        private void shrink(Triangle tri, int delay = 100)
        {
            tri.Schedule(() => tri.ScaleTo(0.01f, delay, EasingTypes.InExpo));
            tri.Schedule(() => tri.Expire());
        }

        protected override float SpawnRatio
        {
            get
            {
                return comboFireEnabled ? 500 * Combo * (TriangleScale * TriangleScale) / (DrawHeight * DrawWidth) : 0;
            }
        }
    }
}
