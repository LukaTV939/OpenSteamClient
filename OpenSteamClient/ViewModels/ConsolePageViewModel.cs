using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenSteamworks.Client.Apps;
using OpenSteamworks.Client.Managers;
using OpenSteamworks.ConCommands;

namespace OpenSteamClient.ViewModels;

public partial class ConsolePageViewModel : AvaloniaCommon.ViewModelBase
{
    [ObservableProperty]
    private string currentCommandText = "clientCommand";

    [ObservableProperty]
    private string outputText = "Some output here\nAnother line";

    public ObservableCollection<string> AutocompleteNames = new();
    public ConsolePageViewModel()
    {
        
    }

    public void SendCommand() {
        ConCommandHandler.ExecuteConsoleCommand(CurrentCommandText);
    }
}