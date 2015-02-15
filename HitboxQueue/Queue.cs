using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace HitboxQueue
{
    public partial class MainWindow
    {
        private static List<String> queueList = new List<String>();

        public void AddToQueue(String user)
        {
            if (!queueList.Contains(user, StringComparer.CurrentCultureIgnoreCase))
                queueList.Add(user);
            UserQueueNumber.Content = queueList.Count;
            
            GetPicture();
        }

        public void RemoveFromQueue(String user)
        {
            user = user.Replace("@", "");
            if (queueList.Contains(user, StringComparer.CurrentCultureIgnoreCase))
                queueList.Remove(queueList.Find(n => n.Equals(user, StringComparison.CurrentCultureIgnoreCase)));
            UserQueueNumber.Content = queueList.Count;

            GetPicture();
        }

        public void ClearQueue()
        {
            queueList.Clear();
            UserQueueNumber.Content = queueList.Count;

            GetPicture();
        }

        public void GetPicture()
        {
            foreach (var VARIABLE in ImageGrid.Children.OfType<Image>())
            {
                if (VARIABLE.Tag != null)
                {
                    VARIABLE.Source = null;
                }
            }
            foreach (var VARIABLE2 in ImageGrid.Children.OfType<Label>())
            {
                if (VARIABLE2.Content != null)
                    VARIABLE2.Content = "";
            }
            
            List<String> first5 = queueList.Take(5).ToList();
            int index = 1;
            foreach (var username in first5)
            {
                String userApi = API.Get(String.Format("https://www.hitbox.tv/api/user/{0}.json", username)).ToString();
                String avatar2 = JObject.Parse(userApi).SelectToken("user_logo").ToString();

                BitmapImage bt = new BitmapImage();

                bt.BeginInit();
                bt.UriSource = new Uri("http://edge.sf.hitbox.tv" + avatar2);
                bt.EndInit();
                switch (index)
                {
                    case 1:
                        User1.Source = bt;
                        User1.Tag = username;
                        Label1.Content = username;
                        break;
                    case 2:
                        User2.Source = bt;
                        User2.Tag = username;
                        Label2.Content = username;
                        break;
                    case 3:
                        User3.Source = bt;
                        User3.Tag = username;
                        Label3.Content = username;
                        break;
                    case 4:
                        User4.Source = bt;
                        User4.Tag = username;
                        Label4.Content = username;
                        break;
                    case 5:
                        User5.Source = bt;
                        User5.Tag = username;
                        Label5.Content = username;
                        break;
                }
                index++;
            }
        }
    }
}