﻿using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Peernet.Browser.WPF.Controls
{
    /// <summary>
    /// Interaction logic for PagerControl.xaml
    /// </summary>
    public partial class PagerControl : UserControl
    {
        public event EventHandler PageSizeChanged;

        public event EventHandler PageIndexChanged;

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register("PageIndex", typeof(int),
                typeof(PagerControl), new PropertyMetadata(OnPageIndexChangedCallBack));

        public static readonly DependencyProperty PageSizeProperty =
                    DependencyProperty.Register("PageSize", typeof(int),
                typeof(PagerControl), new PropertyMetadata(OnPageSizeChangedCallBack));

        public static readonly DependencyProperty PagesCountProperty =
                    DependencyProperty.Register("PagesCount", typeof(int),
                typeof(PagerControl), null);

        public static readonly DependencyProperty FirstPageCommandProperty =
                    DependencyProperty.Register("FirstPageCommand", typeof(ICommand),
                typeof(PagerControl), null);

        public static readonly DependencyProperty LastPageCommandProperty =
                    DependencyProperty.Register("LastPageCommand", typeof(ICommand),
                typeof(PagerControl), null);

        public static readonly DependencyProperty PreviousPageCommandProperty =
                    DependencyProperty.Register("PreviousPageCommand", typeof(ICommand),
                typeof(PagerControl), null);

        public static readonly DependencyProperty NextPageCommandProperty =
                    DependencyProperty.Register("NextPageCommand", typeof(ICommand),
                typeof(PagerControl), null);

        public static readonly DependencyProperty RefreshPageCommandProperty =
                    DependencyProperty.Register("RefreshPageCommand", typeof(ICommand),
                typeof(PagerControl), null);

        public PagerControl()
        {
            InitializeComponent();
        }

        public ICommand FirstPageCommand
        {
            get => (ICommand)GetValue(FirstPageCommandProperty);
            set => SetValue(FirstPageCommandProperty, value);
        }

        public ICommand NextPageCommand
        {
            get => (ICommand)GetValue(NextPageCommandProperty);
            set => SetValue(NextPageCommandProperty, value);
        }

        public ICommand LastPageCommand
        {
            get => (ICommand)GetValue(LastPageCommandProperty);
            set => SetValue(LastPageCommandProperty, value);
        }

        public ICommand PreviousPageCommand
        {
            get => (ICommand)GetValue(PreviousPageCommandProperty);
            set => SetValue(PreviousPageCommandProperty, value);
        }

        public ICommand RefreshPageCommand
        {
            get => (ICommand)GetValue(RefreshPageCommandProperty);
            set => SetValue(RefreshPageCommandProperty, value);
        }

        public int PageIndex
        {
            get => (int)GetValue(PageIndexProperty);
            set => SetValue(PageIndexProperty, value);
        }

        public int PageSize
        {
            get => (int)GetValue(PageSizeProperty);
            set => SetValue(PageSizeProperty, value);
        }

        public int PagesCount
        {
            get => (int)GetValue(PagesCountProperty);
            set => SetValue(PagesCountProperty, value);
        }

        private static void OnPageSizeChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PagerControl pc = sender as PagerControl;
            if (pc != null)
            {
                pc?.PageSizeChanged?.Invoke(sender, new());
            }
        }

        private static void OnPageIndexChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PagerControl pc = sender as PagerControl;
            if (pc != null)
            {
                pc?.PageIndexChanged?.Invoke(sender, new());
            }
        }

        private void PageSizeComboItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var value = (int)((FrameworkElement)e.OriginalSource).DataContext;
            PageSize = value;
        }

        private void ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}