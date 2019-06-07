using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.LayoutService.ItemRendering;
using Sitecore.JavaScriptServices.ViewEngine.LayoutService.Serialization;
using Newtonsoft.Json.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Services.Core.ComponentModel;
using Umbrella.PanTau.Extensions;


namespace Umbrella.PanTau.Transformers
{
  public class LayoutTransformer : Sitecore.JavaScriptServices.ViewEngine.LayoutService.Serialization.LayoutTransformer
  {
    private Dictionary<string, List<string>> _placeholders = new Dictionary<string, List<string>>();

    public LayoutTransformer(IPlaceholderTransformer placeholderTransformer) : base(placeholderTransformer)
    {
    }

    public override dynamic Transform(RenderedItem rendered)
    {
      if (Context.Request.QueryString["form"] != null)
      {
        
        rendered.Placeholders.Clear();
        rendered.Elements.Clear();
        rendered.Context.Clear();
        rendered.Fields = null;

        rendered.Context.Add("forms", GetForms());
        
      }
      else
      {
        rendered.Context.Add("placeholders", GetPlaceholders());
        rendered.Context.Add("templates", GetTemplates());
        rendered.Context.Add("renderings", GetRenderings());
      }

      return base.Transform(rendered);
    }

    #region Forms

    private JArray GetForms()
    {
      var forms = new JArray();
      var formsPath = $"/sitecore/Forms";
      var formsFolder = Sitecore.Context.Database.GetItem(formsPath);
      if (formsFolder == null) return forms;

      foreach (Item form in formsFolder.Children)
      {
        var formData = new JObject()
        {
          new JProperty("id", form.ID.Guid),
          new JProperty("name", form.Name),
          new JProperty("displayName", form.DisplayName),
          new JProperty("isAjax", form.Fields["Is Ajax"].Value),
          new JProperty("cssClass", form.Fields["Css Class"].Value),
          new JProperty("scripts", form.Fields["Scripts"].Value),
          new JProperty("styles", form.Fields["Styles"].Value),
          new JProperty("isTemplate", form.Fields["Is Template"].Value)
        };
        var pages = new JArray();

        foreach (Item page in form.Children)
        {
          pages.Add(ProcessPage(page));
        }

        formData["pages"] = pages;
        forms.Add(formData);
      }

      return forms;
    }

    private static JObject ProcessPage(Item page)
    {
      var pageObject = new JObject().GetItemFields(page);
      var sections = new JArray();

      foreach (Item section in page.Children)
      {
        sections.Add(ProcessSection(section));
      }

      pageObject["sections"] = sections;
      return pageObject;
    }

    private static JObject ProcessSection(Item section, string name = null)
    {
      if (string.IsNullOrEmpty(name))
        name = "fields";
      var sectionObject = new JObject().GetItemFields(section);
      var processed = ProcessFormFields(section);

      var fields = processed.Where(o => o["type"].Value<string>() != "Section");
      var sections = processed.Where(o => o["type"].Value<string>()
          .Equals("Section", StringComparison.OrdinalIgnoreCase));

      sectionObject[name] = new JArray(fields);
      sectionObject["sections"] = new JArray(sections);
      return sectionObject;
    }

    private static JArray ProcessFormFields(Item section)
    {
      var formFields = new JArray();
      foreach (Item sectionChild in section.Children)
      {
        if (sectionChild.Template.Name.Equals("section", StringComparison.OrdinalIgnoreCase))
        {
          formFields.Add(ProcessSection(sectionChild, sectionChild.Name));
        }
        else
        {
          var field = new JObject().GetItemFields(sectionChild);
          field.Merge(GetTemplateFields(sectionChild), new JsonMergeSettings
          {
            // union array values together to avoid duplicates
            MergeArrayHandling = MergeArrayHandling.Union
          });

          if (sectionChild.Children.Count > 0)
          {
            var item = sectionChild;
            if (item.Template.Name.Equals("Folder"))
            {
              item = Context.Database.GetItem(new Sitecore.Data.ID(item.ID.Guid));
              var list = new JArray();
              foreach (Item child in item.Children)
              {
                var listItem = new JObject().GetItemFields(child);
                listItem.Merge(GetTemplateFields(child), new JsonMergeSettings
                {
                  // union array values together to avoid duplicates
                  MergeArrayHandling = MergeArrayHandling.Union
                });
                list.Add(listItem);
              }

              field["items"] = list;
            }
            else
            {
              foreach (Item child in item.Children)
              {
                field[child.Name] = ProcessFormFields(child);
              }
            }
          }
          formFields.Add(field);
          
        }
      }
      return formFields;
    }

    #endregion



    private JArray GetTemplates()
    {
      var templates = new JArray();
      var siteName = Context.Site.Name;
      var templatePath = $"/sitecore/templates/Project/{siteName}";
      var templateFolderItem = Context.Database.GetItem(templatePath);
      if (templateFolderItem == null) return templates;

      foreach (Item item in templateFolderItem.Children)
      {
        var template = TemplateManager.GetTemplate(item.ID, Context.Database);
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
      var renderings = new JArray();
      var siteName = Sitecore.Context.Site.Name;
      var renderingPath = $"/sitecore/layout/Renderings/Project/{siteName}";
      var renderingFolderItem = Sitecore.Context.Database.GetItem(renderingPath);
      if (renderingFolderItem == null) return renderings;

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

          if (string.IsNullOrEmpty(typeKey)) continue;

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
      var placeholders = new JArray();
      var siteName = Sitecore.Context.Site.Name;
      var placeholderPath = $"/sitecore/layout/Placeholder Settings/Project/{siteName}";
      var placeholderFolderItem = Sitecore.Context.Database.GetItem(placeholderPath);
      if (placeholderFolderItem == null) return placeholders;

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

  
    private static JObject GetTemplateFields(Item item)
    {
      var template = TemplateManager.GetTemplate(item.TemplateID, Sitecore.Context.Database);
      if(template == null) return new JObject();

      var jsonTemplate = JObject.FromObject(template);
      var fields = template.GetFields(true);

      foreach (var field in fields)
      {
        if (field.Name.StartsWith("__")) continue;
        jsonTemplate[field.Name] = item[field.ID];
      }

      return jsonTemplate;
    }
  }
}