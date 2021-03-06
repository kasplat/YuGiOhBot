﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects;
using YuGiOhV2.Services;

namespace YuGiOhV2.Core
{
    public class Events
    {

        private DiscordSocketClient _client;
        private CommandService _commands;
        private Database _database;
        private Stats _stats;
        private Chat _chat;
        private Cache _cache;
        private Web _web;
        private Config _config;
        private InteractiveService _interactive;
        private IServiceProvider _services;

        private bool _isInitialized = false;

        private static DiscordSocketConfig _clientConfigBacking;

        private DiscordSocketConfig _clientConfig
        {

            get
            {

                if(_clientConfigBacking == null)
                {

                    _clientConfigBacking = new DiscordSocketConfig()
                    {

                        //AlwaysDownloadUsers = true,
                        LogLevel = LogSeverity.Verbose,
                        MessageCacheSize = 1000

                    };

                    if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                        _clientConfigBacking.WebSocketProvider = WS4NetProvider.Instance;

                }

                return _clientConfigBacking;

            }

        }

        private static readonly CommandServiceConfig _commandConfig = new CommandServiceConfig()
        {

            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Verbose

        };

        public Events()
        {

            Print("Initializing events...");

            _client = new DiscordSocketClient(_clientConfig);
            _commands = new CommandService(_commandConfig);
            _web = new Web();
            _cache = new Cache();
            _interactive = new InteractiveService(_client);
            _config = Config.Instance;

            RegisterLogging();

            Print("Finished initializing events.");

        }

        public async Task GetReadyForBlastOff()
        {

            await RevEngines();

            if (!_isInitialized)
                _client.Ready += YouAintDoneYet;

        }

        private async Task RevEngines()
        {

            var isTest = Environment.GetCommandLineArgs().ElementAtOrDefault(1);
            string token;

            if (!string.IsNullOrEmpty(isTest) && isTest.Equals("true", StringComparison.OrdinalIgnoreCase))
                token = File.ReadAllText("Files/Bot Tokens/Test.txt");
            else
                token = File.ReadAllText("Files/Bot Tokens/Legit.txt");

            Print($"Test: {isTest ?? "false"}");

            Print("Logging in...");
            await _client.LoginAsync(TokenType.Bot, token);
            Print("Logged in.");
            Print("Starting client...");
            await _client.StartAsync();
            Print("ITS UP AND RUNNING BOIIIIIIIIIIIIIIS");

        }

        private async Task YouAintDoneYet()
        {

            //await _cache.GetAWESOMECARDART(_web);
            await LoadDatabase();
            LoadStats();
            BuildServices();
            await RegisterCommands();
            await _client.SetGameAsync($"Support guild/server: {_config.GuildInvite}");

            _isInitialized = true;

        }

        private async Task LoadDatabase()
        {

            Print("Loading database...");
            _database = new Database();
            await _database.Initialize(_client.Guilds);
            Print("Finished loading database.");

            _client.JoinedGuild += _database.AddGuild;

        }

        private void LoadStats()
        {

            _stats = new Stats(_client, _web);

        }

        private void BuildServices()
        {

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_cache)
                .AddSingleton(_database)
                .AddSingleton(_interactive)
                .AddSingleton(_web)
                .AddSingleton(_stats)
                .AddSingleton(_config)
                .AddSingleton<Random>()
                .BuildServiceProvider();

        }

        private async Task RegisterCommands()
        {

            Print("Registering commands...");

            _chat = new Chat(_cache, _database, new Web());

            _client.MessageReceived += HandleCommand;
            _client.MessageReceived += _chat.SOMEONEGETTINGACARDBOIS;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            Print("Commands registered.");

            _client.Ready -= YouAintDoneYet;

        }

        private async Task HandleCommand(SocketMessage message)
        {            

            if (!(message is SocketUserMessage)
                || message.Author.IsBot
                || string.IsNullOrEmpty(message.Content))
                return;

            ulong id = 1;
            string prefix = "y!";

            if (!(message.Channel is SocketDMChannel))
            {

                id = (message.Channel as SocketTextChannel).Guild.Id;
                prefix = _database.Settings[id].Prefix;

            }

            var possibleCmd = message as SocketUserMessage;
            var argPos = 0;

            if ((possibleCmd.HasStringPrefix(prefix, ref argPos) || possibleCmd.HasMentionPrefix(_client.CurrentUser, ref argPos))
                && possibleCmd.Content.Trim() != prefix)
            {

                var context = new SocketCommandContext(_client, possibleCmd);

                if (message.Channel is SocketDMChannel)
                    AltConsole.Print("Info", "Command", $"{possibleCmd.Author.Username} in DM's");
                else
                    AltConsole.Print("Info", "Command", $"{possibleCmd.Author.Username} from {(possibleCmd.Channel as SocketTextChannel).Guild.Name}");


                AltConsole.Print("Info", "Command", $"{possibleCmd.Content}");

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {

                    if (result.ErrorReason.ToLower().Contains("unknown command"))
                        return;
                    else if (result.ErrorReason.ToLower().Contains("you are currently in timeout"))
                        await context.Channel.SendMessageAsync("Please wait 5 seconds between each type of paginator command!");

                    //await context.Channel.SendMessageAsync("https://goo.gl/JieFJM");

                    AltConsole.Print("Error", "Error", result.ErrorReason);
                    //debug purposes
                    //await context.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");

                }

            }

        }

        private void RegisterLogging()
        {

            _client.Log += (message)
                => Task.Run(()
                => AltConsole.Print(message.Severity.ToString(), message.Source, message.Message, message.Exception));
            _commands.Log += (message)
                => Task.Run(()
                => AltConsole.Print(message.Severity.ToString(), message.Source, message.Message, message.Exception));

        }

        private void Print(string message)
            => AltConsole.Print("Info", "Events", message);

    }
}
