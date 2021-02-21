namespace M.Resources.Fonts
{
    public abstract class Font
    {
        #region 基础信息

        public string Name = "字体";
        public string Homepage = "主页";
        public string Author = "作者";
        public string License = "协议";

        protected int Version => 1;

        #endregion

        #region 字体信息

        public bool HaveBold;
        public bool HaveBlack;
        public bool HaveMedium;
        public string FamilyName = "Custom";

        #endregion
    }
}
