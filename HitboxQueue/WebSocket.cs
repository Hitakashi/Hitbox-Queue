using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace HitboxQueue
{
    internal sealed class WebSocketData
    {
        private static WebSocket _websocket;
        private readonly StringBuilder _baseMessage = new StringBuilder("5:::{\"name\":\"message\",\"args\":");
        private static readonly String WSAddress = GetWsAddress();
        private readonly MainWindow hb;

        public WebSocketData(MainWindow hitbox)
        {
            _websocket = new WebSocket("ws://" + WSAddress + "/socket.io/1/websocket/" + GetWsConnId());

            _websocket.Opened += websocket_Opened;
            _websocket.MessageReceived += websocket_MessageReceived;
            _websocket.Error += websocket_Error;
            _websocket.Closed += websocket_Closed;

            hb = hitbox;
            _websocket.Open();
            _websocket.EnableAutoSendPing = false;
        }

        private static String GetWsConnId()
        {
            var fullConnId = API.Get("http://" + WSAddress + "/socket.io/1").ToString();
            return fullConnId.Substring(0, fullConnId.IndexOf(":", StringComparison.Ordinal));
        }

        private static String GetWsAddress()
        {
            using (var client = new WebClient())
            {
                return
                    JArray.Parse(client.DownloadString("http://www.hitbox.tv/api/chat/servers.json?redis=true"))
                        .First.SelectToken("server_ip")
                        .ToString();
            }
        }

        private void websocket_Closed(object sender, EventArgs e)
        {
            Trace.WriteLine("Connection has been closed.");
            _websocket.Open();
            _websocket.EnableAutoSendPing = false;
        }

        private void websocket_Error(object sender, ErrorEventArgs e)
        {
            Trace.WriteLine("Error: " + e.Exception.GetBaseException().Message);
            _websocket.Open();
            _websocket.EnableAutoSendPing = false;
        }

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Equals("1::")) return;
            if (e.Message.Equals("2::"))
            {
                _websocket.Send("2::");
                return;
            }
            if (e.Message.StartsWith("5:::"))
            {
                var jsonObj = JObject.Parse(e.Message.Substring(4));
                var args = JObject.Parse(jsonObj.GetValue("args").First.ToString());
                var method = args.GetValue("method");
                var paramsObject = args.GetValue("params");

                switch (method.ToString())
                {
                    case "loginMsg":
                        break;
                    case "chatMsg":
                        Console.WriteLine(paramsObject);
                        if (paramsObject.SelectToken("buffer") != null) return;

                        hb.Dispatcher.Invoke(delegate
                        {
                            if (paramsObject.SelectToken("text").ToString().Equals("!queue"))
                            {
                                if (hb.subToggle.IsChecked == true && hb.followerToggle.IsChecked == true)
                                {
                                    if (paramsObject.SelectToken("isSubscriber").ToObject<Boolean>() ||
                                        paramsObject.SelectToken("isFollower").ToObject<Boolean>())
                                        hb.Dispatcher.Invoke(
                                            delegate { hb.AddToQueue(paramsObject.SelectToken("name").ToString()); });
                                }
                                else if (hb.subToggle.IsChecked == true)
                                {
                                    // Must be sub
                                    if (paramsObject.SelectToken("isSubscriber").ToObject<Boolean>())
                                        hb.Dispatcher.Invoke(
                                            delegate { hb.AddToQueue(paramsObject.SelectToken("name").ToString()); });
                                }
                                else if (hb.followerToggle.IsChecked == true)
                                {
                                    if (paramsObject.SelectToken("isFollower").ToObject<Boolean>())
                                        hb.Dispatcher.Invoke(
                                            delegate { hb.AddToQueue(paramsObject.SelectToken("name").ToString()); });
                                }
                                else
                                {
                                    hb.Dispatcher.Invoke(
                                        delegate { hb.AddToQueue(paramsObject.SelectToken("name").ToString()); });
                                }
                            }
                        });

                        if (paramsObject.SelectToken("text").ToString().Equals("!leavequeue"))
                        {
                            hb.Dispatcher.Invoke(
                                delegate { hb.RemoveFromQueue(paramsObject.SelectToken("name").ToString()); });
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            Trace.WriteLine("Connected");

            SendJoinChannelMessage();
        }

        private void SendJoinChannelMessage()
        {
            var sb = new StringBuilder(_baseMessage.ToString());
            sb.Append("[{\"method\":\"joinChannel\",\"params\":{\"channel\":\"" + hb.Channel.ToLower() +
                      "\",\"name\":\"UnknownSoldier\"}}]}");
            _websocket.Send(sb.ToString());
        }

    }
}