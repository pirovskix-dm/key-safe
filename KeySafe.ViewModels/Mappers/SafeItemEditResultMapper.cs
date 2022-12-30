using KeySafe.ViewModels.Dependencies;
using KeySafe.ViewModels.Service;
using KeySafe.ViewModels.ViewModels;

namespace KeySafe.ViewModels.Mappers;

public static class SafeItemEditResultMapper
{
    public static SafeItemViewModel ToSafeItemViewModel(this SafeItemEditResult safeItemEditResult)
    {
        return new SafeItemViewModel
        {
            Name = safeItemEditResult.Name,
            Login = safeItemEditResult.Login,
            Password = safeItemEditResult.Password,
            Web = safeItemEditResult.Web,
            Note = safeItemEditResult.Note,
        };
    }
    
    public static StorageItem ToStorageItem(this SafeItemEditResult safeItemEditResult)
    {
        return new StorageItem
        {
            Name = safeItemEditResult.Name,
            Login = safeItemEditResult.Login,
            Password = safeItemEditResult.Password,
            Web = safeItemEditResult.Web,
            Note = safeItemEditResult.Note,
        };
    }
}