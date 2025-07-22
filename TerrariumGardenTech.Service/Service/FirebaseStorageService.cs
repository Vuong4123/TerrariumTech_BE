using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class FirebaseStorageService : IFirebaseStorageService
{
    private readonly FirestoreDb _firestore;
    private readonly StorageClient _storageClient;

    public FirebaseStorageService(IConfiguration configuration)
    {
        var credentialPath = configuration["Firebase:CredentialPath"];
        var projectId = configuration["Firebase:ProjectId"];

        if (string.IsNullOrEmpty(credentialPath) || string.IsNullOrEmpty(projectId))
            throw new InvalidOperationException("Firebase configuration is missing.");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
        // Khởi tạo Firestore
        _firestore = FirestoreDb.Create(projectId);

        // Khởi tạo StorageClient
        _storageClient = StorageClient.Create();
    }

    public async Task SaveTokenAsync(string userId, string fcmToken)
    {
        try
        {
            var tokensCollection = _firestore.Collection("user_tokens").Document(userId).Collection("tokens");

            var snapshot = await tokensCollection.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
                if (doc.ContainsField("Token") && doc.GetValue<string>("Token") == fcmToken)
                {
                    await doc.Reference.DeleteAsync();
                    break;
                }

            var deviceId = Guid.NewGuid().ToString();
            var docRef = tokensCollection.Document(deviceId);

            await docRef.SetAsync(new
            {
                Token = fcmToken,
                CreatedAt = DateTime.UtcNow
            });

            Console.WriteLine("FCMToken saved successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving fcmToken: {ex.Message}");
        }
    }


    public async Task<List<string>> GetUserFcmTokensAsync(string userId)
    {
        try
        {
            var tokensCollection = _firestore.Collection("user_tokens").Document(userId).Collection("tokens");
            var snapshot = await tokensCollection.GetSnapshotAsync();

            var tokens = new List<string>();
            foreach (var doc in snapshot.Documents)
            {
                var tokenData = doc.ToDictionary();
                if (tokenData.ContainsKey("Token")) tokens.Add(tokenData["Token"].ToString());
            }

            return tokens;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching FCM tokens: {ex.Message}");
            return new List<string>();
        }
    }


    public async Task<bool> DeleteTokenAsync(string userId, string fcmToken)
    {
        try
        {
            var tokensRef = _firestore.Collection("user_tokens").Document(userId).Collection("tokens");
            var snapshot = await tokensRef.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
                if (doc.Exists && doc.ContainsField("Token") && doc.GetValue<string>("Token") == fcmToken)
                {
                    await doc.Reference.DeleteAsync();
                    return true;
                }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting token: {ex.Message}");
            return false;
        }
    }

    //public async Task<string> UploadImageAsync(IFormFile file)
    //{
    //    var objectName = $"terrarium-images/{Guid.NewGuid()}_{file.FileName}";
    //    using var stream = file.OpenReadStream();

    //    await _storageClient.UploadObjectAsync(_bucketName, objectName, file.ContentType, stream, new UploadObjectOptions
    //    {
    //        PredefinedAcl = PredefinedObjectAcl.PublicRead
    //    });

    //    return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
    //}

    //public async Task<bool> DeleteImageAsync(string imageUrl)
    //{
    //    try
    //    {
    //        var uri = new Uri(imageUrl);
    //        var objectPath = Uri.UnescapeDataString(uri.AbsolutePath); // /bucket-name/path/to/file
    //        var bucketPrefix = $"/{_bucketName}/";
    //        var objectName = objectPath.StartsWith(bucketPrefix)
    //            ? objectPath.Substring(bucketPrefix.Length)
    //            : objectPath.TrimStart('/');

    //        await _storageClient.DeleteObjectAsync(_bucketName, objectName);
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        // Bạn có thể log nếu cần: Console.WriteLine(ex.Message);
    //        return false;
    //    }
    //}
}