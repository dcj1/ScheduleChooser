using System;
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
    /// Interaction logic for UserWarning.xaml
    /// </summary>
    public partial class UserWarning : Window
    {
        /// <summary>
        /// constructor
        /// </summary>
        public UserWarning()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the warning message
        /// </summary>
        /// <param name="message">String to display</param>
        public void setMessage(string message)
        {
            WarningTextBlock.Text = message;
        }

        /// <summary>
        /// Send it all away.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void acknowledge_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
