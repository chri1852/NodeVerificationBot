using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NodeVerificationBot.Interfaces;
using NodeVerificationBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NodeVerificationBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly IUplandApiRepository _uplandApiRepository;
        private readonly IRegisterRepository _registerRepository;
        private readonly IConfiguration _configuration;

        private readonly Random _random;

        private readonly List<ulong> _rolesToGrant;
        private readonly List<long> _validPropertyIds;

        public Commands(IUplandApiRepository uplandApiRepository, IRegisterRepository registerRepository, IConfiguration configuration)
        {
            _uplandApiRepository = uplandApiRepository;
            _registerRepository = registerRepository;
            _configuration = configuration;

            _random = new Random();

            _rolesToGrant = this._configuration["AppSettings:RolesToGrant"].Split(",").Select(s => ulong.Parse(s)).ToList();
            _validPropertyIds = this._configuration["AppSettings:ValidPropIds"].Split(",").Select(s => long.Parse(s)).ToList();
        }

        [Command("Ping")]
        public async Task Ping()
        {
            await ReplyAsync("Hello!");
        }

        [Command("RegisterMe")]
        public async Task RegisterMe(string uplandUserName)
        {
            List<UplandAuthProperty> properties;
            RegisterData registeredUser = _registerRepository.GetRegisteringUser(Context.User.Id);

            if (registeredUser != null)
            {
                await ReplyAsync(string.Format("You are already registering. Run ClearMe or VerifyMe"));
                return;
            }

            properties = await _uplandApiRepository.GetPropertysByUsername(uplandUserName.ToLower());

            if (properties == null || properties.Count == 0)
            {
                await ReplyAsync(string.Format("Looks like {0} is not a player or has no properties.", uplandUserName));
                return;
            }

            properties = properties.Where(p => _validPropertyIds.Contains(p.Prop_Id)).ToList();

            if (properties.Count == 0)
            {
                await ReplyAsync(string.Format("{0} has no valid properties.", uplandUserName));
                return;
            }

            UplandProperty verifyProp = null;
            int verifyPrice = _random.Next(80000000, 90000000);

            try
            {
                verifyProp = await _uplandApiRepository.GetPropertyById(properties[_random.Next(0, properties.Count)].Prop_Id);
            }
            catch (Exception ex)
            {
                await ReplyAsync(string.Format("Error: {0}", ex.Message));
                return;
            }

            // The registered user is null
            RegisterData newUser = new RegisterData();
            newUser.DiscordId = Context.User.Id;
            newUser.Price = verifyPrice;
            newUser.PropId = verifyProp.Prop_Id;
            newUser.Address = string.Format("{0}, {1}", verifyProp.Full_Address, verifyProp.City.name);
            newUser.UplandUsername = uplandUserName;

            _registerRepository.CreateRegisteringUser(newUser);

            await ReplyAsync(string.Format("You are registered. To Verify place {0} up for sale for {1:N2} UPX, and use VerifyMe. If you cannot place the propery for sale, run ClearMe.", newUser.Address, verifyPrice));
        }

        [Command("ClearMe")]
        public async Task ClearMe(string uplandUsername = "")
        {
            RegisterData registeredUser = _registerRepository.GetRegisteringUser(Context.User.Id);

            if (registeredUser != null)
            {
                try
                {
                    _registerRepository.ClearRegisteringUser(Context.User.Id);
                    await ReplyAsync(string.Format("I have cleared your registration. Try again with RegisterMe and your Upland username."));
                }
                catch (Exception ex)
                {
                    await ReplyAsync(string.Format("Error: {0}", ex.Message));
                }

                return;
            }

            await ReplyAsync(string.Format("Register first with RegisterMe command with your Upland username."));
        }

        [Command("VerifyMe")]
        public async Task VerifyMe(string uplandUsername = "")
        {
            RegisterData registeredUser = _registerRepository.GetRegisteringUser(Context.User.Id);
            if (registeredUser == null)
            {
                await ReplyAsync(string.Format("Please register first with RegisterMe and your Upland username."));
                return;
            }
            else
            {
                UplandProperty property = await _uplandApiRepository.GetPropertyById(registeredUser.PropId);

                if (property.on_market == null)
                {
                    await ReplyAsync(string.Format("Please place {0} on sale for {1:N2}.", registeredUser.Address, registeredUser.Price));
                    return;
                }

                if (property.on_market.token != string.Format("{0}.00 UPX", registeredUser.Price))
                {
                    await ReplyAsync(string.Format("{0} is on sale, but it not for {1:N2}.", registeredUser.Address, registeredUser.Price));
                    return;
                }
                else
                {
                    try
                    {
                        List<ulong> userRoles = (Context.User as SocketGuildUser).Guild.Roles.Where(r => r.Members.Select(m => m.Id).Contains(Context.User.Id)).Select(r => r.Id).ToList();
                        foreach (ulong roleId in _rolesToGrant)
                        {
                            if (!userRoles.Contains(roleId))
                            {
                                await (Context.User as IGuildUser).AddRoleAsync(roleId);
                            }
                        }
                        _registerRepository.ClearRegisteringUser(Context.User.Id);
                        await ReplyAsync(string.Format("You are now Verified."));
                    }
                    catch
                    {
                        await ReplyAsync(string.Format("Error adding roles."));
                    }
                }
            }
            return;
        }
    }
}
