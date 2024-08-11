using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ReturnCommon;
using FUParkingRepository.Interface;
using Microsoft.EntityFrameworkCore;

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
                    Message = SuccessfullyEnumServer.CREATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Card>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<IEnumerable<Card>>> GetAllCardsAsync(GetCardsWithFillerReqDto req)
        {
            try
            {
                var query = _db.Cards
                    .Include(x => x.Sessions)
                    .Where(x => x.DeletedDate == null)
                    .AsQueryable();
                if (!string.IsNullOrEmpty(req.Attribute) && !string.IsNullOrEmpty(req.SearchInput))
                {
                    switch (req.Attribute.ToLower())
                    {
                        case "cardnumber":
                            query = query.Where(x => x.CardNumber.Contains(req.SearchInput));
                            break;
                        case "platenumber":
                            query = query.Where(x => (x.PlateNumber ?? "").Contains(req.SearchInput));
                            break;
                    }
                }
                var result = await query
                        .OrderByDescending(t => t.CreatedDate)
                        .Skip((req.PageIndex - 1) * req.PageSize)
                        .Take(req.PageSize)
                        .ToListAsync();
                return new Return<IEnumerable<Card>>
                {
                    Data = result,
                    TotalRecord = result.Count,
                    IsSuccess = true,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<Card>>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<Card>> GetCardByCardNumberAsync(string cardNumber)
        {
            try
            {
                var result = await _db.Cards.Where(x => x.DeletedDate == null).FirstOrDefaultAsync(x => x.CardNumber == cardNumber);
                return new Return<Card>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Card>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<Card>> GetCardByIdAsync(Guid cardId)
        {
            try
            {
                var result = await _db.Cards.Where(x => x.DeletedDate == null).FirstOrDefaultAsync(x => x.Id == cardId);
                return new Return<Card>
                {
                    Data = result,
                    IsSuccess = true,
                    Message = result == null ? ErrorEnumApplication.NOT_FOUND_OBJECT : SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<Card>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }

        public async Task<Return<Card>> UpdateCardAsync(Card card)
        {
            try
            {
                _db.Cards.Update(card);
                await _db.SaveChangesAsync();
                return new Return<Card>
                {
                    Data = card,
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.UPDATE_OBJECT_SUCCESSFULLY
                };
            }
            catch (Exception e)
            {
                return new Return<Card>
                {
                    InternalErrorMessage = e,
                    Message = ErrorEnumApplication.SERVER_ERROR
                };
            }
        }
    }
}
