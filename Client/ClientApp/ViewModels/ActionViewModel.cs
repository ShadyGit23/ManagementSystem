using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp.Models;
using ClientApp.Services;

namespace ClientApp.ViewModels;

public partial class ActionViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();

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

    [ObservableProperty]
    private bool _isBusy;

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

        if (TargetItem == null)
        {
            Message = "Помилка: не обрано чемпіона!";
            IsSuccess = false;
            return;
        }

        try
        {
            IsBusy = true;

            // Зчитуємо ID авторизованого користувача зі збереженого стану сесії
            int currentUserId = Preferences.Default.Get("user_id", 1);

            var actionRecord = new ActionModel
            {
                ItemID = TargetItem.ID,
                UserID = currentUserId,
                OpponentChampion = OpponentChampion.Trim(),
                ActionDetails = ActionDetails.Trim(),
                Status = SelectedStatus
            };

            var (success, resultMsg) = await _apiService.CreateActionAsync(actionRecord);

            IsSuccess = success;
            Message = resultMsg;

            if (success)
            {
                await Task.Delay(800);
                if (OnActionSaved != null)
                {
                    await OnActionSaved.Invoke();
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}