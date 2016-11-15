using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IntelligentEditing.Startup))]
namespace IntelligentEditing
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
