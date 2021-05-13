using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI_WPF.UCTrackBar
{
    /// <summary>
    /// Interaction logic for UCThreshold.xaml
    /// </summary>
    public partial class UCThreshold : UserControl
    {
        public delegate void delegateValueChange(double value);
        public event delegateValueChange ApplyValueChanged;

        public delegate void delegateDoneEdit();
        public event delegateDoneEdit DoneEdit;
        public UCThreshold()
        {
            InitializeComponent();
        }

        private void sldThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(ApplyValueChanged!=null)
            {
                this.Dispatcher.Invoke(new Action(() => { ApplyValueChanged(sldThreshold.Value); }));
            }
        }

        private void btnApplyImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (ApplyValueChanged != null)
            //{
            //    this.Dispatcher.Invoke(new Action(() => { ApplyValueChanged(100); }));
            //    //ApplyValueChanged(sldThreshold.Value);
            //}
        }

        private void sldThreshold_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DoneEdit != null)
            {
                this.Dispatcher.Invoke(new Action(() => { DoneEdit(); }));
            }
        }
    }
}
