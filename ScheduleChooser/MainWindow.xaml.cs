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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public CommunicationClass commObj;



        private void ScheduleRequest_Click(object sender, RoutedEventArgs e)
        {
            {
                try
                {
                    commObj = new CommunicationClass(this);
                    Page1 page = new Page1(commObj);
                    mainFrame.NavigationService.Navigate(page);
                }
                catch
                {
                    Console.WriteLine("ByeBye");
                    Environment.Exit(0);
                }
            }
        }

        private void manageButton_Click(object sender, RoutedEventArgs e)
        {
            QueueManager qm = new QueueManager(new CommunicationClass(this));

        }
    }
}
