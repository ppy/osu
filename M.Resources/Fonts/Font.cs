namespace M.Resources.Fonts
{
    public abstract class Font
    {
        #region 基础信息

        public string Name = "未知字体";
        public string Description = "未知描述";
        public string Homepage = "未知主页";
        public string Author = "未知作者";
        public string License = "未知许可证";

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
