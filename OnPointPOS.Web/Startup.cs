using Microsoft.Owin;
using Owin;


[assembly: OwinStartupAttribute(typeof(POSSUM.Web.Startup))]
namespace POSSUM.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
          //  ConfigureData();
        }

        private static void ConfigureData()
        {
           // var storage = new WebSessionStorage(System.Web.HttpContext.Current.ApplicationInstance);
          //  DataConfig.Configure(storage);
        }
    }
}
