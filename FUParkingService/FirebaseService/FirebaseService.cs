using FirebaseAdmin.Messaging;
using FUParkingModel.RequestObject.Firebase;
using FUParkingModel.ReturnCommon;

namespace FirebaseService
{
    public class FirebaseService : IFirebaseService
    {
        public async Task<Return<dynamic>> SendNotificationAsync(FirebaseReqDto firebaseReq)
        {
            try
            {
                var message = new MulticastMessage()
                {
                    Tokens = firebaseReq.ClientTokens,
                    Notification = new Notification()
                    {
                        Title = firebaseReq.Title,
                        Body = firebaseReq.Body
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

                return new Return<dynamic>
                {
                    Data = response,
                    IsSuccess = true,
                    Message = "Notification sent successfully"
                };
            } catch (Exception ex)
            {
                return new Return<dynamic>
                {
                    IsSuccess = false,
                    Message = "Failed to send notification",
                    InternalErrorMessage = ex
                };
            }
        }
    }
}
