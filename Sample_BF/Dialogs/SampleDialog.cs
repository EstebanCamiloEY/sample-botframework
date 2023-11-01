using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Sample_BF.Dialogs
{
    public class SampleDialog : BaseDialog
    {
        private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;

        public SampleDialog(ConversationState conversationState) : base(nameof(SampleDialog), conversationState)
        {
            _conversationDataAccessor = conversationState.CreateProperty<ConversationData>("ConversationData");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                TransportStepAsync,
                NameStepAsync,
                NameConfirmStepAsync,
                AgeStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        #region Steps
        private async Task<DialogTurnResult> TransportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the user's response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Introduce un metodo de transporte:"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Coche", "Bus", "Bicicleta" }),
                }, cancellationToken);
        }
        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["transport"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Introduce tu nombre") }, cancellationToken);
        }
        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Gracias {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("¿Quieres añadir tu edad?") }, cancellationToken);
        }
        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // User said "yes" so we will be prompting for the age.
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Introduce tu edad"),
                    RetryPrompt = MessageFactory.Text("El valor no puede ser mayor de 150 ni menor de 0."),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
            }
            else
            {
                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;

            var msg = (int)stepContext.Values["age"] == -1 ? "Edad no registrada" : $"Registro tu edad como {stepContext.Values["age"]}.";

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.NextAsync();
        }
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var userProfile = await _conversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData(), cancellationToken);

            userProfile.Transport = (string)stepContext.Values["transport"];
            userProfile.Name = (string)stepContext.Values["name"];
            userProfile.Age = (int)stepContext.Values["age"];

            var msg = GetProfile(userProfile);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        #endregion

        #region Validators
        private Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 150);
        }
        #endregion

        #region Functions
        public static string GetProfile(ConversationData userProfile)
        {
            var msg = $"Has seleccionado como modo de transporte {userProfile.Transport} y tu nombre es {userProfile.Name}";

            if (userProfile.Age != -1)
            {
                msg += $" y tu edad son {userProfile.Age} años";
            }

            msg += ".";

            return msg;
        }
        #endregion
    }
}