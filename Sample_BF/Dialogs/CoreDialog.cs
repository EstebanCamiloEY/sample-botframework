using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Sample_BF.Dialogs
{
    public class CoreDialog : BaseDialog
    {
        private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;
        
        public CoreDialog(ConversationState conversationState) : base(nameof(CoreDialog), conversationState)
        {
            _conversationDataAccessor = conversationState.CreateProperty<ConversationData>("ConversationData");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                IntialStepAsync,
                CheckProfileStepAsync,
                AskUserStepAsync,
                OptionStepAsync,
                EndStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new SampleDialog(conversationState));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        #region Steps
        private async Task<DialogTurnResult> IntialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync();
        }
        private async Task<DialogTurnResult> CheckProfileStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _conversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData(), cancellationToken);

            if(userProfile != null && userProfile.Name != null)
            {
                return await stepContext.NextAsync();
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(SampleDialog));
            }
        }
        private async Task<DialogTurnResult> AskUserStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Seleccione una de las dos opciones"),
                    RetryPrompt = MessageFactory.Text("Opcion no valida."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Modificar Perfil", "Ver Perfil", "Borrar Perfil" }),
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> OptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var response = ((FoundChoice)stepContext.Result).Value;
            var userProfile = await _conversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData(), cancellationToken);

            if (response.Equals("Modificar Perfil"))
            {
                return await stepContext.BeginDialogAsync(nameof(SampleDialog));
            }
            else if (response.Equals("Ver Perfil"))
            {
                var msg = SampleDialog.GetProfile(userProfile);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            }
            else if (response.Equals("Borrar Perfil"))
            {
                await _conversationDataAccessor.DeleteAsync(stepContext.Context, cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Perfil eliminado."), cancellationToken);
            }

            return await stepContext.NextAsync();
        }
        private async Task<DialogTurnResult> EndStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(nameof(CoreDialog));
        }
        #endregion

        #region Validators
        #endregion
    }
}
