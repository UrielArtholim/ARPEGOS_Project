
namespace ARPEGOS.Services
{
    using System.Threading.Tasks;

    using ARPEGOS.Services.Interfaces;

    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class DialogService : IDialogService
    {
        public async Task DisplayAlert(string title, string msg)
        {
            await Device.InvokeOnMainThreadAsync(
                async () => { await Application.Current.MainPage.DisplayAlert(title, msg, "OK"); });
        }

        public async Task<bool> DisplayAcceptableAlert(string title, string msg, string acceptText = "Si", string cancelText = "No")
        {
            return await Device.InvokeOnMainThreadAsync(
                async () => await Application.Current.MainPage.DisplayAlert(title, msg, acceptText, cancelText));
        }

        public async Task<string> DisplayActionSheet(string title, string cancelText, string deleteText = null, params string[] options)
        {
            return await Device.InvokeOnMainThreadAsync(
                async () => await Application.Current.MainPage.DisplayActionSheet(title, cancelText, deleteText, options));
        }

        public async Task<string> DisplayTextPrompt(string title, string msg, string acceptText = "OK", string cancelText = "Cancelar", string placeholderText = "", int max = -1, Keyboard keyboard = null)
        {
            return await Device.InvokeOnMainThreadAsync(
                async () => await Application.Current.MainPage.DisplayPromptAsync(title, msg, acceptText, cancelText, placeholderText, max, keyboard, string.Empty));
        }
    }
}
