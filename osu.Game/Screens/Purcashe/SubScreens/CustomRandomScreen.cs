namespace osu.Game.Screens.Purcashe.SubScreens
{
    public class CustomRandomScreen : RandomScreen
    {
        public override string ScreenTitle => "";
        public override int ItemCount => RandomTimes;
        public int RandomTimes { get; set; }
    }
}
