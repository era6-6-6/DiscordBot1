﻿using System;
using MetroFramework;
using MetroFramework.Components;
using MetroFramework.Forms;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DiscordBot.TasksClient;
using DiscordBot.SafeThreading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace DiscordBot
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;


        public Form1()
        {
            InitializeComponent();
            StatusLabel.Text = "Status: Offline";
            StatusLabel.BackColor = Color.Red;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void ConnectBotToken_Click(object sender, EventArgs e)
        {
            await Task.Run(() => RunBotAsync().GetAwaiter().GetResult());
            
            
        }
        
      
        public async Task RunBotAsync()
        {
            await Task.Run(async () =>
            {
                _client = new DiscordSocketClient();
                _commands = new CommandService();
                _services = new ServiceCollection()
                    .AddSingleton(_client)
                    .AddSingleton(_commands)
                    .BuildServiceProvider();


                _client.Log += _client_Log;
                
                await RegisterCommandAsync();
                await _client.LoginAsync(TokenType.Bot, TokenTextBox.Text);
                await _client.StartAsync();
               
                await Task.Delay(-1);
            });
        }

        private async Task RegisterCommandAsync()
        {
            _client.Ready += LoadGuilds;
            _client.Ready += ReadyStatus;
            _client.Disconnected += OffStatus;
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageReceived += AllChangesChange;
            _client.MessageReceived += MsgCouter;
            _client.MessageReceived += MessLog;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OffStatus(Exception arg)
        {
            safe = new SafeThreadingForm();
            safe.StatusLAbel("Status: Offline");
            await Task.Delay(1);
        }

        private async Task ReadyStatus()
        {
            safe = new SafeThreadingForm();
            safe.StatusLAbel("Status: Online");
            await Task.Delay(1);
        }

        SafeThreading.SafeThreadingForm safe;
        private async Task MessLog(SocketMessage arg)
        {
            safe = new SafeThreading.SafeThreadingForm();
            safe.MessageLog(arg.Channel.Name + " >> " + arg.Author.Username +  " : " + arg.Content + Environment.NewLine);
            await Task.Delay(1);
            
        }

        private int msgCount = 0;
        private async Task MsgCouter(SocketMessage arg)
        {
            msgCount++;
            safeChange(msgCount.ToString());
            //if (arg.Author.IsBot) return;
            //await _client.GetGuild(759424063130304592).GetTextChannel(arg.Channel.Id).SendMessageAsync(arg.Content);
        }
      
        private async Task AllChangesChange(SocketMessage arg)
        {
            var m = new Random();
            int next = m.Next(0, 13);
            Style = (MetroColorStyle)next;
            
            
        }


        private async Task HandleCommandAsync(SocketMessage arg)
        {
            try
            {
                var message = arg as SocketUserMessage;
                var context = new SocketCommandContext(_client, message);
                if (message.Author.IsBot) return;

                int argPos = 0;
                if (message.HasStringPrefix("+", ref argPos))
                {
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
                    if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
                }

            }
            catch { };

        }

        private Task _client_Log(LogMessage arg)
        {
            LogText(arg.ToString().Replace(DateTime.Now.ToString() , " "));
            return Task.CompletedTask;

        }
        public void LogText(string text)
        {
            if (logTextBox.InvokeRequired)
            {
               
                Action safeWrite = delegate { LogText(DateTime.Now + " " + text + " " + Environment.NewLine); };
                logTextBox.Invoke(safeWrite);
            }
            else
                logTextBox.Text += text;
        }
        public void safeChange(string text)
        {
            if(serverMessagesButton.InvokeRequired)
            {
                Action safeWrite = delegate { safeChange(text); };
                serverMessagesButton.Invoke(safeWrite);
            }
            else
                serverMessagesButton.Text = text;
        }
        Messages mess;
  
        Dictionary<string , ulong> Channel;
        Dictionary<string, ulong> Server;
        private async Task LoadGuilds()
        {
            safe = new SafeThreadingForm();
            Server = new Dictionary<string, ulong>();
            foreach (var server in _client.Guilds)
            {
                safe.ComboBoxServers(server.Name);
                Server.Add(server.Name, server.Id);
                Console.WriteLine(server.Name);
            }
            await Task.Delay(1);

        }
        private void ChannelsTExtBox_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            
        }
        

        private void SendBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var myKey = Channel.FirstOrDefault(x => x.Key == ChannelsTExtBox.Text).Value;
                _client.GetGuild(ulong.Parse(ServerIdTextBox.Text)).GetTextChannel(myKey).SendMessageAsync(MessageBox.Text);
            }catch
            {

            }
        }

        private void LoadBtn_Click_1(object sender, EventArgs e)
        {
            Channel = new Dictionary<string, ulong>();
            ChannelsTExtBox.Items.Clear();


            foreach (var channel in _client.GetGuild(ulong.Parse(ServerIdTextBox.Text)).TextChannels)
            {
                try
                {
                    Channel.Add(channel.ToString(), channel.Id);
                    ChannelsTExtBox.Items.Add(channel.ToString());
                }
                catch (Exception ex)
                {
                    ChannelsTExtBox.Items.Add("Not Able To Add this Channel");
                }
            }
        }

        private void GuildsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                
                loadInfo();
            }
            catch
            {

            }
        }
        public void loadInfo()
        {
            try
            {
                safe = new SafeThreadingForm();
                var myKey = Server.FirstOrDefault(x => x.Key == GuildsComboBox.Text).Value;
                ServerIdTextBox.Text = myKey.ToString();
                safe.ServerNameText(_client.GetGuild(ulong.Parse(ServerIdTextBox.Text)).Name);
                safe.ServerOwner(_client.GetGuild(ulong.Parse(ServerIdTextBox.Text)).Owner.Username);
                string Created = _client.GetGuild(ulong.Parse(ServerIdTextBox.Text)).CreatedAt.ToString();
                Created = Created.Substring(0, Created.LastIndexOf("+"));
                safe.CreatedAt(Created);
                safe.RolesNumber(_client.GetGuild(ulong.Parse(ServerIdTextBox.Text)).Roles.Count.ToString());
                safe.TotalMembers(_client.GetGuild(ulong.Parse(ServerIdTextBox.Text)).Users.Count.ToString());
                safe.TotalChannels(_client.GetGuild(ulong.Parse(ServerIdTextBox.Text)).Channels.Count.ToString());


            } catch
            {
                
            }
        }
    }
}
