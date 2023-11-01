// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;

namespace Sample_BF.Bot
{
    public class EchoBot : ActivityHandler
    {
        string patter = "(?:6[0-9]|7[1-9])[0-9]{7}$";

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Funcion que te devuelve la fecha actual en formato DateTime
            var actualDate = DateTime.Now;
            // Funcion que te divide un string en un array de string apartir de un caracter.
            var textSplittedByPlus = "2+2".Split('+');
            // Funcion que te devuelve el numero de telefono movil en españa de un string.
            var phoneNumber = Regex.Match("este es mi numero 612345678", patter);

            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}