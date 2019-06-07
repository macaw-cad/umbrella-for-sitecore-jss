using Newtonsoft.Json.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Umbrella.PanTau.Extensions
{
  public static class ItemExtensions
  {
    public static JObject GetItemFields(this JObject obj, Item item)
    {
      obj["id"] = item.ID.Guid;
      obj["name"] = item.Name;
      obj["displayName"] = item.DisplayName;
      obj["type"] = item.Template.Name;

      foreach (Field field in item.Fields)
      {
        if (field.Name.StartsWith("__")) continue;
        obj[field.Name] = field.Value;
      }

      return obj;
    }
  }
}
