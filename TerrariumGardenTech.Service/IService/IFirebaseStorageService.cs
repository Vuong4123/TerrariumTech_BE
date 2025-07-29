namespace TerrariumGardenTech.Service.IService;

public interface IFirebaseStorageService
{
    Task SaveTokenAsync(string userId, string fcmToken);

    Task<List<string>> GetUserFcmTokensAsync(string userId);
    Task<bool> DeleteTokenAsync(string userId, string fcmToken);
    //Task<string> UploadImageAsync(IFormFile file);
}