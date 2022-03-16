# Node Verification Bot

## Bot Commands
### !Ping

The bot will reply "Hello!". Mostly used for making sure the bot is reachable.

### !RegisterMe hornbrod

If you don't have the Role specified in the appsettings.json file, and are not already in the process of registering. The Bot will find a property owned by the user in the Neighborhood specified in the appsettings.json file. It will then ask them to place it for sale for some large number.

### !ClearMe

This will clear the user after they have used !RegisterMe. Its particularly useful if they can't place the specified property on sale for some reason.

### !VerifyMe

Once the property returned by !RegisterMe is on sale for the specified amount. Run !VerifyMe. If the bot confirms everything it will grant the user the role specified in the appsettings.json file.

## Setup

### Appsettings

1. UplandAuthToken - The Upland Authentication Token. This can be retrieved by intercepting request made in upland through a program like Fiddler.
2. DiscordBotToken - The Token For the Discord Bot. This is retrieved from the Discord Developer Portal Under OAuth2 --> General --> Client Secret
3. CommandPrefix - The Character the bot will look for to identify what is a command. ex. !
4. ChannelIdsToListenTo - A list of ChannelIds seperated by commas. The bot will only respond to commands in those channels.
5. RolesToGrant - A List of RoleIds sepearted by commas. The bot will grant these roles on successful validation.
6. NeighborhoodToValidate - The Name of the neighborhood you want to verify. This is case insensitive.

### Bot Permissions

The Bot will need the below permissions when set up in the Discord Developer Portal:

1. Manage Roles
2. Send Messages
3. Read Message History

After you invite the bot to your Discord Server, you will need to make sure that the Bots Role is listed above the Roles you plan to grant in Server Settings --> Roles. This is due to how role heirarchys work in discord. Roles can only grant access to roles lower than themselves.

