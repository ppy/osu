namespace M.Resources.Fonts
{
    public abstract class Font
    {
        #region 基础信息

        public string Name = "未知字体";
        public string Homepage = "主页";
        public string Author = "未知作者";
        public string License = "未知许可证";

        protected int Version => 1;

        #endregion

        #region 字体信息

        public bool LightAvaliable;
        public bool MediumAvaliable;
        public bool SemiBoldAvaliable;
        public bool BoldAvaliable;
        public bool BlackAvaliable;

        public string FamilyName = "UnknownFamilyName";

        #endregion
    }
}
