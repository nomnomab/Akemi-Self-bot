using Discord.WebSocket;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AkemiSelfBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Instance;
        public static DiscordSocketClient Client;
        public static JsonConfig Config;

        private List<GuildObj> Guilds;

        private string selectedChannel;
        private Timer Update;

        public Queue<Action> Actions = new Queue<Action>();

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            OnStartup();
        }



        #region Connecting

        public void OnStartup()
        {
            Client = new DiscordSocketClient();
            Config = new JsonConfig();
            Config.Load();
            Guilds = new List<GuildObj>();
            Update = new Timer(1000);
            Update.Elapsed += Update_Elapsed;
            Update.Start();
            SaveMessagesCheckbox.Visibility = Visibility.Hidden;

            if (!string.IsNullOrEmpty(Config.Token)) ConnectToken();
            else
            {
                DisplayLogin();
            }
        }

        private void Update_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Actions.Count <= 0) return;
            Action a = Actions.Dequeue();
            a.Invoke();
        }

        private async Task DisplayLogin()
        {
            string token = await this.ShowInputAsync("Login", "Give me your User Token");
            Config.Token = token;
            ConnectToken();
        }

        private async Task ConnectToken()
        {
            Config.Token = Config.Token.Replace("\"", "").TrimStart().TrimEnd();
            try
            {
                await Client.LoginAsync(Discord.TokenType.User, Config.Token);
                await Client.StartAsync();
                await this.ShowMessageAsync("I am Connected to Discord", "");
                LoadServers();
            }
            catch (Exception e)
            {
                await this.ShowMessageAsync("I Ran into a Problem", e.Message);
                DisplayLogin();
                return;
            }
        }

        #endregion
        private void LoadServers()
        {
            Guilds.Clear();

            foreach(SocketGuild guild in Client.Guilds)
            {
                Guilds.Add(new GuildObj(guild));
            }

            SetSidebarImages();

            foreach (GuildObj guildObj in Guilds)
            {
                guildObj.LoadChannels();
            }
        }

        #region Downloader

        // mfa.acCPHEFQEePgTygn_PFl--oPv3poo7V9m9h6FPKb_2tPN_7htUnMFKFMY650X0KUWnazcdFM1jw7tt4VNMs1

        public void SetSidebarImages()
        {
            ListBox servers = ServerImages;
            ChannelNames.SelectionChanged += ChannelNames_SelectionChanged;
            servers.Items.Clear();
            servers.SelectionChanged += Servers_SelectionChanged;
            foreach(GuildObj guild in Guilds.OrderBy(x=>x.Guild.Name))
            {
                Image img = new Image();
                img.Source = guild.Image;
                img.Width = 50;
                img.Height = 50;
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => servers.Items.Add(img)));
            }
        }

        private void Servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // set channels to second list view
            MessagesTab.Items.Clear();
            SaveMessagesCheckbox.Visibility = Visibility.Hidden;
            Console.WriteLine(sender.GetType());
            ListBox box = (ListBox)sender;
            Image img = (Image)box.SelectedItem;
            GuildObj guild = Guilds.FirstOrDefault(x => x.Image == img.Source);
            ListBox channels = ChannelNames;
            channels.Items.Clear();
            foreach (ChannelObj obj in guild.TextChannels)
            {
                //Application.Current.Dispatcher.BeginInvoke(
                //  DispatcherPriority.Background,
                //  new Action(() => channels.Items.Add(obj.Channel.Name)));
                channels.Items.Add(obj.Channel.Name);
            }
        }

        private void ChannelNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessagesTab.Items.Clear();
            ListBox box = (ListBox)sender;
            string content = (string)box.SelectedItem;
            Console.WriteLine("Content: " + content);
            selectedChannel = content;
            GuildObj guild = Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(y=>y.Channel.Name == content) != null);
            if (guild == null) return;
            SaveMessagesCheckbox.Visibility = Visibility.Visible;
            ChannelObj channel = guild.TextChannels.FirstOrDefault(x => x.Channel.Name == content);
            UpdateMessageList(channel, true);
            SaveMessagesCheckbox.IsChecked = channel.SaveMessages;
        }

        #endregion

        private void SaveMessagesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            GuildObj guild = Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(y => y.Channel.Name == selectedChannel) != null);
            if (guild == null) return;
            ChannelObj channel = guild.TextChannels.FirstOrDefault(x => x.Channel.Name == selectedChannel);
            channel.SaveMessages = (bool)SaveMessagesCheckbox.IsChecked;
            Client.MessageReceived += channel.Client_MessageReceived;
        }

        private void SaveMessagesCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            GuildObj guild = Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(y => y.Channel.Name == selectedChannel) != null);
            if (guild == null) return;
            ChannelObj channel = guild.TextChannels.FirstOrDefault(x => x.Channel.Name == selectedChannel);
            channel.SaveMessages = (bool)SaveMessagesCheckbox.IsChecked;
            Client.MessageReceived -= channel.Client_MessageReceived;
        }

        public void UpdateMessageList(ChannelObj obj, bool reset = false)
        {
            if (reset) MessagesTab.Items.Clear();

            foreach (MessageObj msg in obj.Messages)
            {
                //foreach (object o in MessagesTab.Items) strings.Add((string)o);
                if (MessagesTab.Items.Contains(msg.ToString())) continue;
                if (selectedChannel != obj.Channel.Name) return;
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => MessagesTab.Items.Add(msg.ToString())));
            }
        }
    }
}
