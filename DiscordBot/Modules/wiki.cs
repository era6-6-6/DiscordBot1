using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Genbox.Wikipedia;
using Genbox.Wikipedia.Objects;
using Genbox.Wikipedia.Enums;

namespace DiscordBot.Modules
{
    public class wiki : ModuleBase<SocketCommandContext>
    {
        [Command("wiki")]
        public async Task Wiki([Remainder]string search)
        {
            WikipediaClient client = new WikipediaClient();
            client.Limit = 1;
            client.Language = Language.English;
            QueryResult results = client.Search(search);
            foreach(var item in results.Search)
            {
                await ReplyAsync(item.Url.ToString().Replace(" ", "_"));
            }
        }
    }
}
