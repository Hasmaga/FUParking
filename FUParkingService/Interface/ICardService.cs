using FUParkingModel.RequestObject.Card;
using FUParkingModel.ResponseObject.Card;
using FUParkingModel.ReturnCommon;

namespace FUParkingService.Interface
{
    public interface ICardService
    {
        Task<Return<bool>> CreateNewCardAsync(CreateNewCardReqDto req);
        Task<Return<List<GetCardResDto>>> GetListCardAsync(GetCardsWithFillerReqDto req);
        Task<Return<bool>> DeleteCardByIdAsync(Guid id);
        Task<Return<bool>> UpdatePlateNumberCard(string PlateNumber, Guid CardId);
    }
}
