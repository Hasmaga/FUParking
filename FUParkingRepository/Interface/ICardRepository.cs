using FUParkingModel.Object;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ResponseObject.Card;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ICardRepository
    {
        Task<Return<Card>> CreateCardAsync(Card card);
        Task<Return<Card>> GetCardByIdAsync(Guid cardId);
        Task<Return<IEnumerable<GetCardResDto>>> GetAllCardsAsync(GetCardsWithFillerReqDto req);
        Task<Return<Card>> GetCardByCardNumberAsync(string cardNumber);
        Task<Return<Card>> UpdateCardAsync(Card card);
        Task<Return<Card>> GetCardByPlateNumberAsync(string plateNumber);
    }
}
