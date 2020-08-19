
namespace ARPEGOS.Services.Interfaces
{
    using System.Threading.Tasks;

    using Xamarin.Forms;

    public interface IDialogService
    {
        Task DisplayAlert(string title, string msg);

        Task<bool> DisplayAcceptableAlert(string title, string msg, string acceptText = "Si", string cancelText = "No");

        Task<string> DisplayActionSheet(string title, string cancelText, string deleteText = null, params string[] options);

        Task<string> DisplayTextPrompt(string title, string msg, string acceptText = "OK", string cancelText = "Cancelar", string placeholderText = "", int max = -1, Keyboard keyboard = null);
    }
}
