using System;
using System.Threading;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace HitboxQueue
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public  String Channel;

        public MainWindow()
        {
            // Int the Window
            InitializeComponent();
        }

        private async void SetChannel_OnClick(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowInputAsync("Hello!", "What is your username?");

            if (result == null) //user pressed cancel
                return;

            Channel = result;
            Thread thread = new Thread(() =>
            {
                new WebSocketData(this);
            }); 
           thread.Start();
        }
    }
}
