namespace Utils
{
    using ParrelSync;
    
    public static class LocalProfileTool
    {
        private static string _localProfileSuffix;

        public static string LocalProfileSuffix => _localProfileSuffix ??= GetCloneNameEnd();

        private static string GetCloneNameEnd()
        {
            #if UNITY_EDITOR
            
            if(ClonesManager.IsClone())
            {
                var cloneName = ClonesManager.GetCurrentProject().name;
                var lastUnderscoreIndex = cloneName.LastIndexOf("_");
                var numberStr = cloneName.Substring(lastUnderscoreIndex + 1);
                return numberStr;
            }
            
            #endif
            
            return "";
        }
    }
}