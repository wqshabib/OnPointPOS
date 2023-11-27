using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(POSSUM.Api.Startup))]
namespace POSSUM.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
