using FUParkingModel.DatabaseContext;
using FUParkingModel.Enum;
using FUParkingModel.Object;
using FUParkingModel.RequestObject.Card;
using FUParkingModel.ResponseObject.Card;
using FUParkingModel.ResponseObject.Statistic;
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

        public async Task<Return<IEnumerable<GetCardResDto>>> GetAllCardsAsync(GetCardsWithFillerReqDto req)
        {
            try
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                var query = _db.Cards
                    .Include(x => x.Sessions)
                    .Where(x => x.DeletedDate == null)
                    .Select(x => new GetCardResDto
                    {
                        Id = x.Id,
                        CardNumber = x.CardNumber,
                        PlateNumber = x.PlateNumber,
                        CreatedDate = x.CreatedDate,
                        Status = x.Status,
                        SessionId = _db.Sessions
                            .Where(s => s.CardId == x.Id && s.Status.Equals(SessionEnum.PARKED))
                            .OrderByDescending(s => s.CreatedDate)
                            .Select(s => s.Id.ToString())
                            .FirstOrDefault(),
                        PlateNumberSession = _db.Sessions
                            .Where(s => s.CardId == x.Id && s.Status.Equals(SessionEnum.PARKED))
                            .OrderByDescending(s => s.CreatedDate)
                            .Select(s => s.PlateNumber)
                            .FirstOrDefault()
                    });
#pragma warning restore CS8601 // Possible null reference assignment.
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
                        case "platenumbersession":
                            query = query.Where(x => (x.PlateNumberSession ?? "").Contains(req.SearchInput));
                            break;
                        case "status":
                            query = query.Where(x => x.Status.Equals(req.SearchInput));
                            break;
                    }
                }
                var result = await query
                        .OrderByDescending(t => t.CreatedDate)
                        .Skip((req.PageIndex - 1) * req.PageSize)
                        .Take(req.PageSize)
                        .ToListAsync();
                return new Return<IEnumerable<GetCardResDto>>
                {
                    Data = result,
                    TotalRecord = result.Count,
                    IsSuccess = true,
                    Message = result.Count > 0 ? SuccessfullyEnumServer.FOUND_OBJECT : ErrorEnumApplication.NOT_FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<IEnumerable<GetCardResDto>>
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

        public async Task<Return<Card>> GetCardByPlateNumberAsync(string plateNumber)
        {
            try
            {
                var result = await _db.Cards
                    .Where(x => x.DeletedDate == null)
                    .FirstOrDefaultAsync(x => x.PlateNumber == plateNumber);
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

        public async Task<Return<StatisticCardResDto>> GetStatisticCardAsync()
        {
            try
            {
                var totalCard = await _db.Cards.CountAsync(x => x.DeletedDate == null);
                var totalCardInUse = await _db.Sessions.CountAsync(x => x.Status.Equals(SessionEnum.PARKED));
                return new Return<StatisticCardResDto>
                {
                    Data = new StatisticCardResDto
                    {
                        TotalCard = totalCard,
                        TotalCardInUse = totalCardInUse
                    },
                    IsSuccess = true,
                    Message = SuccessfullyEnumServer.FOUND_OBJECT
                };
            }
            catch (Exception e)
            {
                return new Return<StatisticCardResDto>
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
