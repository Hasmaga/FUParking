using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface ICardRepository
    {
        Task<Return<Card>> CreateCardAsync(Card card);
    }
}
