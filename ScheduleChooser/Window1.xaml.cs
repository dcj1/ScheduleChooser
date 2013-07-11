﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        /// <summary>
        ///Constructor
        /// </summary>
        public LoginDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// what to do whaen we click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submit_Click(object sender, RoutedEventArgs e)
        {
            if ((userNameBox.Text.Length < 1) || (passwordBox1.Password.Length < 1)) return;
            this.DialogResult = true;
        }

        /// <summary>
        /// what to do whaen we click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
