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
    /// Interaction logic for SlotRequest.xaml
    /// </summary>
    public partial class SlotRequest : UserControl
    {
        public SlotRequest()
        {
            InitializeComponent();
        }

        //Predefined colors for boxes
        public Brush OPEN = Brushes.AntiqueWhite;
        public Brush BUSY = Brushes.Gray;
        public Brush REQUEST = Brushes.Yellow;
        public Brush PHONE = Brushes.Green;
        private Brush previous = Brushes.AntiqueWhite;

        // Change to the appropriate status color and select if status is request
        public void setStatus(Brush b)
        {
            this.Background = b;
            if (b == REQUEST)
            {
                Selected.IsChecked = true;
            }
        }

        // Change back to open and deselect everything
        public void reset()
        {
            setStatus(OPEN);
            Selected.IsChecked = false;
        }

        // Hide the implementation
        public bool isSelected()
        {
            return (bool)Selected.IsChecked;
        }

        // Set the label value
        public void setDepth(int value)
        {
            Count.Content = value;
        }

        // update if clicked
        public void activate()
        {
            previous = this.Background;
            this.Background = this.REQUEST;
            int c = Convert.ToInt32(Count.Content.ToString());
            c++;
            Count.Content = c;
        }

        // update if unclicked
        public void deactivate()
        {
            this.Background = previous;
            int c = Convert.ToInt32(Count.Content.ToString());
            c--;
            Count.Content = c;
        }

        private void Selected_Checked(object sender, RoutedEventArgs e)
        {
            this.activate();
        }

        private void Selected_Unchecked(object sender, RoutedEventArgs e)
        {
            this.deactivate();
        }
    }
}
