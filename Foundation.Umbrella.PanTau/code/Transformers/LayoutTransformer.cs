using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.LayoutService.ItemRendering;
using Sitecore.JavaScriptServices.ViewEngine.LayoutService.Serialization;
using Newtonsoft.Json.Linq;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Services.Core.ComponentModel;


namespace Umbrella.PanTau.Transformers
{
    public class LayoutTransformer: Sitecore.JavaScriptServices.ViewEngine.LayoutService.Serialization.LayoutTransformer
    {
        private Dictionary<string, List<string>> _placeholders = new Dictionary<string, List<string>>();

        public LayoutTransformer(IPlaceholderTransformer placeholderTransformer) : base(placeholderTransformer)
        {
        }

        public override dynamic Transform(RenderedItem rendered)
        {
            //rendered.Fields.RemoveAll(); // remove all fields
            //rendered.Placeholders.Clear();
            //rendered.Elements.Clear();
            //rendered.Context.Clear();

            rendered.Context.Add("placeholderManifest", GetPlaceholders());
            rendered.Context.Add("templates", GetTemplates());
            rendered.Context.Add("renderings", GetRenderings());
            return base.Transform(rendered);
        }

        private JArray GetTemplates()
        {
            var siteName = Sitecore.Context.Site.Name;
            var templatePath = $"/sitecore/templates/Project/{siteName}";
            var templateFolderItem = Sitecore.Context.Database.GetItem(templatePath);
            var templates = new JArray();

            foreach (Item item in templateFolderItem.Children)
            {
                var template = TemplateManager.GetTemplate(item.ID, Sitecore.Context.Database);
                var jsonTemplate = JObject.FromObject(template);
                var fields = template.GetFields(true);
                var jsonFields = new JArray();
                
                foreach (var field in fields)
                {
                    if (field.Name.StartsWith("__")) continue;
                    var jsonField = new JObject()
                    {
                        new JProperty("id", field.ID.Guid),
                        new JProperty("name", field.Name),
                        new JProperty("defaultValue", field.DefaultValue),
                        new JProperty("type", field.Type),
                        new JProperty("typeKey", field.TypeKey),
                        new JProperty("icon", field.Icon),
                        new JProperty("isShared", field.IsShared),
                        new JProperty("inherited", field.Template.Name.StartsWith("_")),
                        new JProperty("templateId", field.Template?.ID.Guid),
                        new JProperty("templateName", field.Template.Name)                                             
                    };
                    jsonFields.Add(jsonField);
                }

                jsonTemplate["fields"] = jsonFields;
                templates.Add(jsonTemplate);
            }
            return templates;
        }

        private JArray GetRenderings()
        {
            var siteName = Sitecore.Context.Site.Name;
            var renderingPath = $"/sitecore/layout/Renderings/Project/{siteName}";
            var renderingFolderItem = Sitecore.Context.Database.GetItem(renderingPath);
            var renderings = new JArray();

            foreach (Item item in renderingFolderItem.Children)
            {
                var jsonFields = new JArray();
                var jsonPlaceholders = new JArray();

                var jsonRendering = new JObject()
                {
                    new JProperty("id", item.ID.Guid),
                    new JProperty("name", item.Name),
                    new JProperty("displayName", item.DisplayName),
                    new JProperty("icon", item["__Icon"]),
                };

                foreach (Sitecore.Data.Fields.Field field in item.Fields)
                {
                    if (
                        field.Name.StartsWith("__") ||
                        field.Section.Equals("Layout Service", StringComparison.OrdinalIgnoreCase) ||
                        field.Section.Equals("Editor Options", StringComparison.OrdinalIgnoreCase)
                    ) continue;

                    var typeKey = Templates.CommonFieldTypes
                        .FirstOrDefault(x => x.Value.Equals(field.Type, StringComparison.OrdinalIgnoreCase)).Key;

                    if(string.IsNullOrEmpty(typeKey)) continue;
                    
                    var jsonField = new JObject()
                    {
                        new JProperty("id", field.ID.Guid),
                        new JProperty("name", field.Name),
                        new JProperty("type", $"CommonFieldTypes.{typeKey}"),
                    };
                    jsonFields.Add(jsonField);
                }

                var id = item.ID.Guid.ToString("B").ToUpper();
                if (_placeholders.ContainsKey(id))
                {
                    foreach (var placeholderId in _placeholders[id])
                    {
                        jsonPlaceholders.Add(placeholderId);
                    }
                }

                jsonRendering["fields"] = jsonFields;
                jsonRendering["placeholders"] = jsonPlaceholders;
                renderings.Add(jsonRendering);
            }
            return renderings;
        }

        private JArray GetPlaceholders()
        {
            var siteName = Sitecore.Context.Site.Name;
            var placeholderPath = $"/sitecore/layout/Placeholder Settings/Project/{siteName}";
            var placeholderFolderItem = Sitecore.Context.Database.GetItem(placeholderPath);
            var placeholders = new JArray();

            foreach (Item item in placeholderFolderItem.Children)
            {
               var allowed = item.Fields.FirstOrDefault(f => f.Name.Equals("Allowed Controls", StringComparison.OrdinalIgnoreCase));
               if (allowed != null)
               {
                   var allowedControls = allowed.Value.Split('|');

                   foreach (var controlId in allowedControls)
                   {
                       AddPlaceholderRef(ref _placeholders, $"{item.ID.Guid.ToString("B").ToUpper()}|{item.Name}", controlId);
                   }
               }

               var jsonPlaceholder = new JObject()
                {
                    new JProperty("id", item.ID.Guid),
                    new JProperty("name", item.Name),
                    new JProperty("displayName", item.DisplayName),
                };
                placeholders.Add(jsonPlaceholder);
            }
            return placeholders;
        }

        private void AddPlaceholderRef(
            ref Dictionary<string, List<string>> placeholders, 
            string placeholderId,
            string conponentId)
        {
            if (!placeholders.ContainsKey(conponentId))
                placeholders.Add(conponentId, new List<string>());
            if (!placeholders[conponentId].Contains(placeholderId))
                placeholders[conponentId].Add(placeholderId);
        }
    }
}