using FUParkingModel.RequestObject.Firebase;
using FUParkingModel.ReturnCommon;

namespace FirebaseService
{
    public interface IFirebaseService
    {
        Task<Return<dynamic>> SendNotificationAsync(FirebaseReqDto firebaseReq);
    }
}
