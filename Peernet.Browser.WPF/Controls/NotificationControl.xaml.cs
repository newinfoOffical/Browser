﻿using System.Windows;
using System.Windows.Controls;
using Peernet.Browser.Application.Contexts;
using Peernet.Browser.Models;
using Peernet.Browser.Models.Presentation;

namespace Peernet.Browser.WPF.Controls
{
    /// <summary>
    /// Interaction logic for NotificationControl.xaml
    /// </summary>
    public partial class NotificationControl : UserControl
    {
        public static readonly DependencyProperty NotificationProperty =
            DependencyProperty.Register("Notification", typeof(string),
                typeof(NotificationControl), null);

        public NotificationControl()
        {
            InitializeComponent();
        }

        public string Notification
        {
            get => (string)GetValue(NotificationProperty);
            set => SetValue(NotificationProperty, value);
        }

        private void RemoveNotification(object sender, RoutedEventArgs e)
        {
            var notification = (Notification)((FrameworkElement)e.OriginalSource).DataContext;
            GlobalContext.Notifications.Remove(notification);
        }
    }
}