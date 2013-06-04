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
    /// Interaction logic for SlotApproval.xaml
    /// </summary>
    public partial class SlotApproval : UserControl
    {
        public SlotApproval()
        {
            InitializeComponent();
            this.reset();
        }

        //Predefined colors for boxes
        public Brush OPEN = Brushes.AntiqueWhite;
        public Brush BUSY = Brushes.Gray;
        public Brush REQUEST = Brushes.Yellow;
        public Brush PHONE = Brushes.Green;

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
    }
}
