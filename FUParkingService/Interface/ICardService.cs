using FUParkingModel.RequestObject.Card;
using FUParkingModel.ResponseObject.Card;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ICardService
    {
        Task<Return<dynamic>> CreateNewCardAsync(CreateNewCardReqDto req);
        Task<Return<IEnumerable<GetCardResDto>>> GetListCardAsync(GetCardsWithFillerReqDto req);
        Task<Return<dynamic>> DeleteCardByIdAsync(Guid id);
        Task<Return<dynamic>> UpdatePlateNumberInCardAsync(string PlateNumber, Guid CardId);
    }
}
