namespace osu.Game.Screens.Mvis.UI.Objects.MusicVisualizers.Bars
{
    public class CircularBar : BasicBar
    {
        public CircularBar()
        {
            Masking = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CornerRadius = Width / 2;
        }

        protected override float ValueFormula(float amplitudeValue, float valueMultiplier) => Width + base.ValueFormula(amplitudeValue, valueMultiplier);
    }
}
