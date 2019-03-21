using Sitecore.LayoutService.Configuration;
using Sitecore.Mvc.Presentation;
using System.Web.Mvc;

namespace Umbrella.PanTau.Resolvers
{
    public class RenderingContentsResolver : Sitecore.LayoutService.ItemRendering.ContentsResolvers.RenderingContentsResolver
    {
        public override object ResolveContents(Rendering rendering, IRenderingConfiguration renderingConfig)
        {           
            // return empty because we don't want to output data from the route for the configuration
            return new { };
        }
    }
}