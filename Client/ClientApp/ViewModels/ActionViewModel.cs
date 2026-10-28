using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp.Models;

namespace ClientApp.ViewModels;

public partial class ActionViewModel : ObservableObject
{
    [ObservableProperty]
    private ItemModel? _targetItem;

    [ObservableProperty]
    private string _opponentChampion = string.Empty;

    [ObservableProperty]
    private string _actionDetails = string.Empty;

    [ObservableProperty]
    private string _selectedStatus = "Draft";

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    public List<string> StatusOptions { get; } = new() { "Draft", "Approved", "Archived" };

    public event Func<Task>? OnActionSaved;

    public void SetItem(ItemModel item)
    {
        TargetItem = item;
        OpponentChampion = string.Empty;
        ActionDetails = string.Empty;
        SelectedStatus = "Draft";
        Message = string.Empty;
    }

    [RelayCommand]
    private async Task SaveActionAsync()
    {
        Message = string.Empty;

        if (string.IsNullOrWhiteSpace(OpponentChampion) || string.IsNullOrWhiteSpace(ActionDetails))
        {
            Message = "Будь ласка, заповніть усі обов'язкові поля!";
            IsSuccess = false;
            return;
        }

        // Емуляція збереження для Завдання 8 (виклик API буде в Завданні 9)
        Message = $"Гайд проти {OpponentChampion} успішно створено!";
        IsSuccess = true;

        await Task.Delay(600);

        if (OnActionSaved != null)
        {
            await OnActionSaved.Invoke();
        }
    }
}