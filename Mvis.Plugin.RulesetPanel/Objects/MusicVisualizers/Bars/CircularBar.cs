namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers.Bars
{
    public class CircularBar : BasicBar
    {
        public new float Width
        {
            get => base.Width;
            set
            {
                base.Width = value;
                CornerRadius = value / 2;
            }
        }

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
