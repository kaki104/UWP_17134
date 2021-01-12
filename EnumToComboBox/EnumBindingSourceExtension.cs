using System;
using Windows.UI.Xaml.Markup;

namespace EnumToComboBox
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        public Type EnumType { get; set; }

        public EnumBindingSourceExtension()
        {
        }

        protected override object ProvideValue()
        {
            if (EnumType is null || !EnumType.IsEnum)
                throw new Exception("EnumType must not be null and of type enum");
            return Enum.GetValues(EnumType);
        }
    }
}
