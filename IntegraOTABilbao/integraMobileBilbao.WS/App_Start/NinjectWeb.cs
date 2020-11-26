[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(integraMobile.WS.App_Start.NinjectWeb), "Start")]

namespace integraMobile.WS.App_Start
{
//    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject.Web;

    public static class NinjectWeb 
    {
        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
        }
    }
}
