using System;
using System.Drawing;
using System.ServiceProcess;
using System.Windows.Forms;

namespace TrayServiceManager
{
    /// <summary>
    /// Tray process controler context.
    /// </summary>
    public class TrayServiceControlerContext : ApplicationContext
    {
        #region Fields

        private readonly ServiceController _serviceController;
        private readonly NotifyIcon _notifyIcon;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the instance of the TrayServiceControlerContext class.
        /// </summary>
        /// <param name="serviceController">The service controller.</param>
        public TrayServiceControlerContext(ServiceController serviceController)
        {
            _serviceController = serviceController;

            _notifyIcon = new NotifyIcon
            {
                Text = GetNotifyIconText(serviceController),
                Icon = Properties.Resources.ServicesIcon,
                ContextMenuStrip = GenerateContextMenu()
            };

            _notifyIcon.DoubleClick += _notifyIcon_DoubleClick;

            Application.ApplicationExit += (sender, e) => { _notifyIcon.Visible = false; };

            _notifyIcon.Visible = true;
        }

        #endregion

        #region Private Methods

        private string GetNotifyIconText(ServiceController serviceController)
        {
            return string.Format("{0} ({1})", serviceController.DisplayName, serviceController.Status);
        }

        private ContextMenuStrip GenerateContextMenu()
        {
            var contextMenu = new ContextMenuStrip { Name = "TrayIconContextMenu", Size = new Size(153, 70) };

            contextMenu.SuspendLayout();

            var closeMenuItem = new ToolStripMenuItem
            {
                Name = "CloseMenuItem",
                Size = new Size(152, 22),
                Text = "Exit"
            };

            closeMenuItem.Click += (sender, e) => Application.Exit();

            contextMenu.Items.AddRange(new ToolStripItem[] { closeMenuItem });

            contextMenu.ResumeLayout();

            return contextMenu;
        }

        private void _notifyIcon_DoubleClick(object sender, System.EventArgs e)
        {
            switch (_serviceController.Status)
            {
                case ServiceControllerStatus.Stopped:
                    {
                        _serviceController.Start();
                        _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
                        break;
                    }
                case ServiceControllerStatus.StartPending:
                    {
                        _serviceController.Refresh();
                        _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
                        break;
                    }
                case ServiceControllerStatus.StopPending:
                    {
                        _serviceController.Refresh();
                        _serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1));
                        break;
                    }
                case ServiceControllerStatus.Running:
                    {
                        if (_serviceController.CanStop)
                        {
                            _serviceController.Stop();
                            _serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1));
                        }
                        break;
                    }
                case ServiceControllerStatus.ContinuePending:
                    {
                        _serviceController.Refresh();
                        _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
                        break;
                    }
                case ServiceControllerStatus.PausePending:
                    {
                        _serviceController.Refresh();
                        _serviceController.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromMinutes(1));
                        break;
                    }
                case ServiceControllerStatus.Paused:
                    {
                        if (_serviceController.CanPauseAndContinue)
                        {
                            _serviceController.Continue();
                            _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
                        }
                        break;
                    }
            }

            _notifyIcon.Text = GetNotifyIconText(_serviceController);
        }

        #endregion
    }
}