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

            _notifyIcon = new NotifyIcon { Icon = Properties.Resources.ServicesIcon };
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            UpdateNotifyIcon();

            Application.ApplicationExit += (sender, e) => { _notifyIcon.Visible = false; };

            _notifyIcon.Visible = true;
        }

        #endregion

        #region Private Methods

        private void UpdateNotifyIcon()
        {
            UpdateNotifyIconText();
            UpdateNotifyIconMenu();
        }

        private void UpdateNotifyIconText()
        {
            _notifyIcon.Text = string.Format("{0} ({1})", _serviceController.DisplayName, _serviceController.Status);
        }

        private void UpdateNotifyIconMenu()
        {
            if (_notifyIcon.ContextMenuStrip == null)
                _notifyIcon.ContextMenuStrip = new ContextMenuStrip { Name = "TrayIconContextMenu", Size = new Size(153, 70) };

            _notifyIcon.ContextMenuStrip.SuspendLayout();

            var closeMenuItem = _notifyIcon.ContextMenuStrip.Items["CloseMenuItem"] as ToolStripMenuItem;
            if (closeMenuItem == null)
            {
                closeMenuItem = new ToolStripMenuItem
                {
                    Name = "CloseMenuItem",
                    Size = new Size(152, 22),
                    Text = "Exit"
                };

                closeMenuItem.Click += (sender, e) => Application.Exit();

                _notifyIcon.ContextMenuStrip.Items.Add(closeMenuItem);
            }

            _notifyIcon.ContextMenuStrip.ResumeLayout();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (_serviceController.Status == ServiceControllerStatus.Stopped || _serviceController.Status == ServiceControllerStatus.StopPending)
                {
                    _serviceController.Start();
                    _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
                }
                else
                {
                    _serviceController.Stop();
                    _serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1));
                }
            }
            catch (Exception ex)
            {
                _notifyIcon.ShowBalloonTip(1000, "Error", ex.Message, ToolTipIcon.Error);
                throw;
            }

            _serviceController.Refresh();
            UpdateNotifyIcon();
        }

        #endregion
    }
}