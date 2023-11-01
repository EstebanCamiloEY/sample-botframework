using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Sample_BF.Dialogs
{
    public class BaseDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;

        public BaseDialog(string id, ConversationState conversationState) : base(id)
        {
            _conversationDataAccessor = conversationState.CreateProperty<ConversationData>("ConversationData");

        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default) {
            if (CheckInterrupt(innerDc))
            {
                await _conversationDataAccessor.DeleteAsync(innerDc.Context, cancellationToken);
                await innerDc.Context.SendActivityAsync(MessageFactory.Text("Hasta luego!"), cancellationToken);
                return await innerDc.CancelAllDialogsAsync(cancellationToken);
            }
            else
                return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }


        private static bool CheckInterrupt(DialogContext innerDc)
            => innerDc.Context.Activity.Text != null && innerDc.Context.Activity.Text.ToLower().Equals("adios");
    }
}
