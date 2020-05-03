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
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;

namespace Ice_Age
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string noticeage = "S:\\Coding\\C#\\Hackaton\\noticeage.png";
        private string iceage = "S:\\Coding\\C#\\Hackaton\\iceage.jpg";
        public MainWindow()
        {
            InitializeComponent();
        }

        public void ShowResult(bool result)
        {
            Uri resourceUri = new Uri(result ? iceage : noticeage, UriKind.Absolute);
            Results.Source = new BitmapImage(resourceUri);
        }





    }
}
