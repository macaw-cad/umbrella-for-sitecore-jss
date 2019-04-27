using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.LayoutService.Serialization;
using Sitecore.LayoutService.Serialization.FieldSerializers;
using Sitecore.Diagnostics;

namespace Umbrella.PanTau.Serialization.FieldSerializers
{
    public class ImageFieldSerializer : BaseFieldSerializer
    {
        private string _renderedValue;
        public ImageFieldSerializer(IFieldRenderer fieldRenderer) : base(fieldRenderer)
        {
        }

        protected override void WriteValue(Field field, JsonTextWriter writer)
        {
            Assert.ArgumentNotNull((object)field, nameof(field));
            var renderedImage = this.ParseRenderedImage(this.GetRenderedValue(field));
            var imageField = (ImageField) field;

            MediaItem mediaItem = imageField.MediaItem;
            var stream = mediaItem.GetMediaStream();
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            var base64 = $"data:{mediaItem.MimeType};base64,{Convert.ToBase64String(bytes)}";

            writer.WriteStartObject();
            foreach (var keyValuePair in renderedImage)
            {
                writer.WritePropertyName(keyValuePair.Key);
                writer.WriteValue(keyValuePair.Value);
            }

            writer.WritePropertyName("type");
            writer.WriteValue("media");
            writer.WritePropertyName("id");
            writer.WriteValue(mediaItem.ID.Guid);
            writer.WritePropertyName("path");
            writer.WriteValue(mediaItem.Path);
            writer.WritePropertyName("mediaPath");
            writer.WriteValue(mediaItem.MediaPath);
            writer.WritePropertyName("filePath");
            writer.WriteValue(mediaItem.FilePath);
            writer.WritePropertyName("fileName");
            writer.WriteValue($"{mediaItem.Name}.{mediaItem.Extension}");
            writer.WritePropertyName("displayName");
            writer.WriteValue(mediaItem.DisplayName);
            writer.WritePropertyName("description");
            writer.WriteValue(mediaItem.Description);
            writer.WritePropertyName("extension");
            writer.WriteValue(mediaItem.Extension);
            writer.WritePropertyName("filePath");
            writer.WriteValue(mediaItem.FilePath);
            writer.WritePropertyName("name");
            writer.WriteValue(mediaItem.Name);
            writer.WritePropertyName("base64");
            writer.WriteValue(base64);
            writer.WritePropertyName("mimeType");
            writer.WriteValue(mediaItem.MimeType);

            writer.WriteEndObject();
        }

        private string GetRenderedValue(Field field)
        {
            if (string.IsNullOrWhiteSpace(this._renderedValue))
                this._renderedValue = this.RenderField(field, false).ToString();
            return this._renderedValue;
        }
        private IDictionary<string, string> ParseRenderedImage(string renderedField)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(renderedField);
            if (htmlDocument.DocumentNode == null || !htmlDocument.DocumentNode.HasChildNodes)
                return (IDictionary<string, string>)dictionary;
            HtmlNode htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//img");
            if (htmlNode == null)
                return (IDictionary<string, string>)dictionary;
            foreach (HtmlAttribute attribute in (IEnumerable<HtmlAttribute>)htmlNode.Attributes)
                dictionary[attribute.Name] = HttpUtility.HtmlDecode(attribute.Value);
            return (IDictionary<string, string>)dictionary;
        }
    }
}