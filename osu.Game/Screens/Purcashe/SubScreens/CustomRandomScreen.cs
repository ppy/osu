namespace osu.Game.Screens.Purcashe.SubScreens
{
    public class CustomRandomScreen : RandomScreen
    {
        public override string ScreenTitle => $"{RandomTimes}{(RandomTimes <= 1 ? "次" : "连")}";
        public override int ItemCount => RandomTimes;
        public int RandomTimes { get; set; }
    }
}
