using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AkemiSelfBot
{
    public class GuildObj
    {
        public SocketGuild Guild;
        public List<ChannelObj> TextChannels = new List<ChannelObj>();

        public BitmapImage Image;

        public GuildObj(SocketGuild guild)
        {
            Guild = guild;
            Image = new BitmapImage(new Uri(@Guild.IconUrl));
        }

        public void LoadChannels()
        {
            foreach (SocketChannel channel in Guild.Channels)
            {
                if(channel.GetType() == typeof(SocketTextChannel)) TextChannels.Add(new ChannelObj((SocketTextChannel)channel));
            }
            TextChannels = TextChannels.OrderBy(x => x.Channel.Name).ToList();
        }
    }

    public class ChannelObj
    {
        public SocketTextChannel Channel;

        public bool SaveMessages;
        public bool SaveMedia;

        public List<MessageObj> Messages = new List<MessageObj>();

        public ChannelObj(SocketTextChannel channel)
        {
            Channel = channel;
        }

        public Task Client_MessageReceived(SocketMessage arg)
        {
            if(arg.Channel.Name == Channel.Name && SaveMessages)
            {
                Messages.Add(new MessageObj(arg));
                MainWindow.Instance.UpdateMessageList(this, false);
            }
            return null;
        }
    }

    public class MessageObj
    {
        public SocketMessage Message;

        public string Author;
        public string Nickname;
        public ulong Id;
        public string Time;
        public string Content;

        public MessageObj(SocketMessage msg)
        {
            Message = msg;
            Author = msg.Author.Username;
            SocketGuildUser user = (SocketGuildUser)Message.Author;
            Nickname = user.Nickname;
            Id = msg.Author.Id;
            Time = msg.Timestamp.ToString();
            Content = msg.Content;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} said: {2}", Time, Author, Content);
        }
    }
}
