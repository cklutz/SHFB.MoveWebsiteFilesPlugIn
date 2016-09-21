using System.ComponentModel;
using MoveWebsiteFiles.Properties;

namespace MoveWebsiteFiles
{
    public sealed class LocalizableCategoryAttribute : CategoryAttribute
    {
        public LocalizableCategoryAttribute(string category)
            : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return Resources.ResourceManager.GetString(value);
        }
    }
}