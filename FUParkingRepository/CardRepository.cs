using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;

namespace FUParkingRepository
{
    public class CardRepository : ICardRepository
    {
        private readonly FUParkingDatabaseContext _db;

        public CardRepository(FUParkingDatabaseContext db)
        {
            _db = db;
        }

        public async Task<Return<Card>> CreateCardAsync(Card card)
        {
            try
            {
                await _db.Cards.AddAsync(card);
                await _db.SaveChangesAsync();
                return new Return<Card>
                {
                    Data = card,
                    IsSuccess = true,
                    SuccessfullyMessage = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY                    
                };
            }
            catch (Exception e)
            {
                return new Return<Card>
                {
                    IsSuccess = false,
                    InternalErrorMessage = e.Message,
                    ErrorMessage = ErrorEnumApplication.ADD_OBJECT_ERROR
                };
            }
        }
    }
}
