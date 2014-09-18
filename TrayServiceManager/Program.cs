using System;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;

namespace TrayServiceManager
{
    /// <summary>
    /// Program class.
    /// </summary>
    static class Program
    {
        #region Static Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length == 1 && !string.IsNullOrWhiteSpace(args[0]))
            {
                var serviceController = ServiceController.GetServices().SingleOrDefault(service => service.ServiceName == args[0]);
                if (serviceController != null)
                {
                    using (serviceController)
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new TrayServiceControlerContext(serviceController));
                    }
                }
            }
        }

        #endregion
    }
}
