using System.ComponentModel;
using MoveWebsiteFiles.Properties;

namespace MoveWebsiteFiles
{
    public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
    {
        public LocalizableDescriptionAttribute(string description)
            : base(description)
        {
        }

        public override string Description
        {
            get { return Resources.ResourceManager.GetString(DescriptionValue); }
        }
    }
}