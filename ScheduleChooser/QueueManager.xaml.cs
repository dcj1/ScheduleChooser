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
    /// Interaction logic for QueueManager.xaml
    /// </summary>
    public partial class QueueManager : Window
    {
        CommunicationClass commObj;

        public QueueManager(CommunicationClass c)
        {
            InitializeComponent();
            commObj = c;        
            QueueList.ItemsSource = commObj.fetchQueueNames();
            this.Show();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            commObj.saveQueueList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddToPrimaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in TASList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                commObj.putInQueue(s, QueueList.SelectedItem.ToString(), true);
                moveListItem(s, TASList, PrimaryMemberList);
            }
        }

        private void RemoveFromPrimaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in PrimaryMemberList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                commObj.removeFromQueue(s, QueueList.SelectedItem.ToString());
                moveListItem(s, PrimaryMemberList, TASList);
            }
        }

        private void AddToSecondaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in TASList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                commObj.putInQueue(s, QueueList.SelectedItem.ToString(), false);
                moveListItem(s, TASList, SecondaryMemberList);
            }
        }

        private void RemoveFromSecondaryQueue_Click(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            foreach (string s in SecondaryMemberList.SelectedItems)
            {
                names.Add(s);
            }
            foreach (string s in names)
            {
                commObj.removeFromQueue(s, QueueList.SelectedItem.ToString()); 
                moveListItem(s, SecondaryMemberList, TASList);
            }
        }

        private void QueueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrimaryMemberList.Items.Clear();
            SecondaryMemberList.Items.Clear();

            //Get primary queue names
            List<string> names = commObj.getTASNames(QueueList.SelectedValue.ToString(), true);
            foreach (string s in names)
            {
                PrimaryMemberList.Items.Add(s);
            }

            //Get secondary queue names
            names = commObj.getTASNames(QueueList.SelectedValue.ToString(), false);
            foreach (string s in names)
            {
                SecondaryMemberList.Items.Add(s);
            }

            startTimeTextBox.Text = commObj.getQueueStartTime(e.ToString()).ToShortTimeString();
            endTimeTextBox.Text = commObj.getQueueEndTime(e.ToString()).ToShortTimeString();
            durationTextBox.Text = commObj.getQueueSlotDuration(e.ToString()).ToString();
        }

        private void moveListItem(string s, ListView source, ListView dest)
        {
            string s2 = s.Clone() as string;
            source.Items.Remove(s);
            dest.Items.Add(s2);
        }

        private void EditTASList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewQueueButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
