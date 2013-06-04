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
    /// Interaction logic for SlotStatus.xaml
    /// </summary>
    public partial class SlotStatus : UserControl
    {
        public SlotStatus()
        {
            InitializeComponent();
            this.reset();
        }

        //Values to set up the color scheme
        public Brush OPEN = Brushes.AntiqueWhite;
        public Brush BUSY = Brushes.Gray;
        public Brush REQUEST = Brushes.Yellow;
        public Brush PHONE = Brushes.Green;

        //Set the value of the widget
        public void setStatus(Brush b)
        {
            this.Background = b;
        }

        //Set everything back to default
        public void reset()
        {
            setStatus(OPEN);
        }

    }
}
