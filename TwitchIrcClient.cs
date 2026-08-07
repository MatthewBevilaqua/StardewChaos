using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace StardewChaos
{
    public class TwitchIrcClient
    {
        private TcpClient _tcp;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Thread _thread;
        private volatile bool _running;
        private volatile bool _connected;
        private int _reconnectDelay = 2000;
        private const int MaxReconnectDelay = 30000;

        private readonly string _oauth;
        private readonly string _nick;
        private readonly string _channel;

        public bool IsConnected => _connected;
        public readonly ConcurrentQueue<(string user, int vote)> VoteQueue = new();

        private static readonly Regex PrivmsgRegex = new(
            @":(\w+)!\w+@\w+\.tmi\.twitch\.tv PRIVMSG #(\w+) :(.*)",
            RegexOptions.Compiled);

        public TwitchIrcClient(string oauth, string nick, string channel)
        {
            _oauth = oauth.StartsWith("oauth:") ? oauth.Substring(6) : oauth;
            _nick = nick.ToLower();
            _channel = channel.ToLower();
        }

        public void Connect()
        {
            _running = true;
            _thread = new Thread(RunLoop) { IsBackground = true, Name = "TwitchIRC" };
            _thread.Start();
        }

        public void Disconnect()
        {
            _running = false;
            _connected = false;
            try { _writer?.WriteLine("QUIT"); _writer?.Flush(); } catch { }
            try { _tcp?.Close(); } catch { }
        }

        private void RunLoop()
        {
            while (_running)
            {
                try
                {
                    _tcp = new TcpClient("irc.chat.twitch.tv", 6667);
                    var stream = _tcp.GetStream();
                    _reader = new StreamReader(stream);
                    _writer = new StreamWriter(stream) { NewLine = "\r\n", AutoFlush = true };

                    _writer.WriteLine($"PASS oauth:{_oauth}");
                    _writer.WriteLine($"NICK {_nick}");
                    _writer.WriteLine($"JOIN #{_channel}");

                    _connected = true;
                    _reconnectDelay = 2000;

                    string line;
                    while (_running && (line = _reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("PING"))
                        {
                            _writer.WriteLine("PONG " + line.Substring(4));
                        }
                        else if (line.Contains("PRIVMSG"))
                        {
                            var m = PrivmsgRegex.Match(line);
                            if (m.Success)
                            {
                                string user = m.Groups[1].Value;
                                string msg = m.Groups[3].Value.Trim();
                                if (int.TryParse(msg, out int vote) && vote >= 1 && vote <= 8)
                                {
                                    VoteQueue.Enqueue((user, vote));
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                _connected = false;
                if (!_running) break;

                Thread.Sleep(_reconnectDelay);
                _reconnectDelay = Math.Min(_reconnectDelay * 2, MaxReconnectDelay);
            }
        }
    }
}