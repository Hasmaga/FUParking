using FUParkingModel.Object;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ICardRepository
    {
        Task<Return<Card>> CreateCardAsync(Card card);
        Task<Return<Card>> GetCardByIdAsync(Guid cardId);
        Task<Return<IEnumerable<Card>>> GetAllCardsAsync(GetCardsWithFillerReqDto req);
        Task<Return<Card>> GetCardByCardNumberAsync(string cardNumber);
        Task<Return<Card>> UpdateCardAsync(Card card);
    }
}
