using System;
using System.Diagnostics;
using System.Linq;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.LayoutService.Serialization;
using Sitecore.LayoutService.Serialization.FieldSerializers;
using Sitecore.LayoutService.Serialization.Pipelines.GetFieldSerializer;

namespace Umbrella.PanTau.Serialization.Pipelines.GetFieldSerializer
{
    public class GetImageFieldSerializer : BaseGetFieldSerializer
    {
        public GetImageFieldSerializer(IFieldRenderer fieldRenderer) : base(fieldRenderer)
        {
            
        }

        protected override void SetResult(GetFieldSerializerPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));

            var path = ((System.Web.HttpRequestWrapper) ((System.Web.HttpContextWrapper) Context.HttpContext)
                .Request).Path;
            if (path != null && 
                path.Split('/').Last().Equals("umbrella", StringComparison.OrdinalIgnoreCase))
            {
                args.Result = new FieldSerializers.ImageFieldSerializer(this.FieldRenderer);
            }
            else
            {
                args.Result = new ImageFieldSerializer(this.FieldRenderer);
            }                      
        }
    }
}