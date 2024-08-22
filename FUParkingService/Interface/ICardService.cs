using FUParkingModel.RequestObject.Card;
using FUParkingModel.ResponseObject.Card;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ICardService
    {
        Task<Return<dynamic>> CreateNewCardAsync(CreateNewCardReqDto req);
        Task<Return<IEnumerable<GetCardResDto>>> GetListCardAsync(GetCardsWithFillerReqDto req);
        Task<Return<dynamic>> DeleteCardByIdAsync(Guid id);
        Task<Return<dynamic>> UpdatePlateNumberInCardAsync(string PlateNumber, Guid CardId);
        Task<Return<bool>> ChangeStatusCardToMissingAsync(Guid cardId);
        Task<Return<bool>> ChangeStatusCardAsync(Guid cardId, bool isActive);
        Task<Return<StatisticCardResDto>> GetStatisticCardAsync();
        Task<Return<GetCardByCardNumberResDto>> GetCardByCardNumberAsync(string cardNumber);
    }
}
