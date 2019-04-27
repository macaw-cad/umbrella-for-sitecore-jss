using System.Collections.Generic;
using Sitecore.Services.Core.ComponentModel;

namespace Umbrella.PanTau
{
    public static class Templates
    {
        public static Dictionary<string, string> CommonFieldTypes
        {
            get
            {
                var fieldTypes = new Dictionary<string, string>();

                fieldTypes.Add("SingleLineText", "Single-Line Text");
                fieldTypes.Add("MultiLineText", "Multi-Line Text");
                fieldTypes.Add("RichText", "Rich Text");
                fieldTypes.Add("ContentList", "Treelist");
                fieldTypes.Add("ItemLink", "Droptree");
                fieldTypes.Add("GeneralLink", "General Link");
                fieldTypes.Add("Image", "Image");
                fieldTypes.Add("File", "File");
                fieldTypes.Add("Number", "Number");
                fieldTypes.Add("Checkbox", "Checkbox");
                fieldTypes.Add("Date", "Date");
                fieldTypes.Add("Datetime", "Datetime");

                return fieldTypes;
            }
        }
    }
}