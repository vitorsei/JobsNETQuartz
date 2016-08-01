namespace Quartz.Presentation.Modules
{
    public static class ExtensionMethods
    {
        public static string BooleanDisplayValuesAsYesNo(this bool value)
        {
            if (value)
                return "Yes";

            return "No";
        }
    }
}