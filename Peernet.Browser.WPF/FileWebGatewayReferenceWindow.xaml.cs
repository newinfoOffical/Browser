﻿using DevExpress.Mvvm.Native;
using Microsoft.Extensions.DependencyInjection;
using Peernet.Browser.Application.Managers;
using Peernet.SDK.Common;
using Peernet.SDK.Models.Domain.Common;
using Peernet.SDK.Models.Presentation.Footer;
using Peernet.SDK.WPF;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Input;

namespace Peernet.Browser.WPF
{
    /// <summary>
    /// Interaction logic for FileWebGatewayReferenceWindow.xaml
    /// </summary>
    public partial class FileWebGatewayReferenceWindow : PeernetWindow
    {
        public ApiFile File { get; set; }

        public string[] WebGatewayResourceUris => GetWebGatewayResourceUris();

        public FileWebGatewayReferenceWindow(ApiFile file)
        {
            Initialized += Window_Initialized;
            ContentRendered += Window_ContentRendered;
            Owner = App.Current.MainWindow;
            WindowStartupLocation = App.Current.MainWindow.WindowStartupLocation;

            InitializeComponent();
            MouseDown += Window_MouseDown;
            DataContext = this;
            File = file;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.Topmost = false;
            this.Focus();
            Owner = null;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.Activate();
            this.Topmost = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Maximize_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CopyLinkToClipboard_OnClick(object sender, RoutedEventArgs e)
        {
            var webGatewayUri = ((FrameworkElement)e.OriginalSource)?.DataContext as string;
            Clipboard.SetText(webGatewayUri);
            App.ServiceProvider.GetRequiredService<INotificationsManager>().Notifications.Add(new Notification("Copied to clipboard!"));
        }

        private string[] GetWebGatewayResourceUris()
        {
            var domains = App.ServiceProvider.GetRequiredService<ISettingsManager>().WebGatewayDomains;

            return domains?.Select(domain =>
            HttpUtility.UrlDecode(
                new UriBuilder
                {
                    Scheme = Uri.UriSchemeHttps,
                    Host = domain,
                    Path = $"{Convert.ToHexString(File?.NodeId)}/{Convert.ToHexString(File?.Hash)}?fileName={File.Name}"
                }.ToString())).ToArray();
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var uri = (string)((FrameworkElement)e.OriginalSource).DataContext;
            var processInfo = new ProcessStartInfo(uri);
            processInfo.UseShellExecute = true;
            Process.Start(processInfo);
            e.Handled = true;
        }
    }
}