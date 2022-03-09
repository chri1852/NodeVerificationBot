using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodeVerificationBot.Interfaces;
using NodeVerificationBot.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NodeVerificationBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private IConfiguration _configuration;
        private string _commandPrefix;
        private List<ulong> _channelIdsToListenTo;
        private List<ulong> _rolesToGrant;

        static void Main(string[] args)
            => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton<IUplandApiRepository, UplandApiRepository>()
                .AddSingleton<IRegisterRepository, RegisterRepository>()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_configuration)
                .BuildServiceProvider();

            _client.Log += clientLog;

            _commandPrefix = _configuration["AppSettings:CommandPrefix"];
            _channelIdsToListenTo = _configuration["AppSettings:ChannelIdsToListenTo"].Split(",").Select(s => ulong.Parse(s)).ToList();
            _rolesToGrant = _configuration["AppSettings:RolesToGrant"].Split(",").Select(s => ulong.Parse(s)).ToList();

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, _configuration["AppSettings:DiscordBotToken"]);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task clientLog(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage message = arg as SocketUserMessage;

            if (message == null)
            {
                return;
            }

            SocketCommandContext context = new SocketCommandContext(_client, message);

            if (message.Author.IsBot)
            {
                return;
            }

            if (!_channelIdsToListenTo.Contains(context.Channel.Id))
            {
                return;
            }

            int argPos = 0;
            if (message.HasStringPrefix(_commandPrefix, ref argPos))
            {
                List<ulong> userRoles = (context.User as SocketGuildUser).Guild.Roles.Where(r => r.Members.Select(m => m.Id).Contains(context.User.Id)).Select(r => r.Id).ToList();
                if (_rolesToGrant.All(r => userRoles.Contains(r)))
                {
                    await context.Channel.SendMessageAsync("You have already registered and verified.");
                    return;
                }

                Task child = Task.Factory.StartNew(async () =>
                {
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess)
                    {
                        switch (result.ErrorReason)
                        {
                            case "Unknown command.":
                                await context.Channel.SendMessageAsync(string.Format("Unknown Command, Try {0}RegisterMe", _commandPrefix));
                                break;
                            case "The input text has too few parameters.":
                                await context.Channel.SendMessageAsync(string.Format("Put your Upland Username after RegisterMe"));
                                break;
                            case "Object reference not set to an instance of an object.":
                            case "The server responded with error 503: ServiceUnavailable":
                                await context.Channel.SendMessageAsync(string.Format("ERROR: Upland may be in Maintenance right now, try again later."));
                                break;
                            default:
                                await context.Channel.SendMessageAsync(string.Format("ERROR: Contact Admin"));
                                break;
                        }
                        Console.WriteLine(string.Format("{0}: {1}", string.Format("{0:MM/dd/yy H:mm:ss}", DateTime.Now), result.ErrorReason));
                    }

                    Console.WriteLine(string.Format("{0}: {1} - {2}", string.Format("{0:MM/dd/yy H:mm:ss}", DateTime.Now), message.Author.Username, message.Content));
                });
            }
        }

    }
}
