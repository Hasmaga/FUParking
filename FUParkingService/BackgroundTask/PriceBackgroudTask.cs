using Coravel.Invocable;
using FUParkingRepository.Interface;
using Microsoft.Extensions.Logging;

namespace FUParkingService.BackgroundTask
{
    public class PriceBackgroudTask : IInvocable
    {
        // Create background task to minus all the money in waller extra if ExpiredDate <= DateTime.Now
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger<PriceBackgroudTask> _logger;

        public PriceBackgroudTask(IWalletRepository walletRepository, ILogger<PriceBackgroudTask> logger)
        {
            _walletRepository = walletRepository;
            _logger = logger;
        }

        public async Task Invoke()
        {
            try
            {
                var result = await _walletRepository.UpdateAllWalletExtraAndMinusBalanceBackgroundTaskAsync();
                if (!result.IsSuccess)
                {
                    _logger.LogError("Update all wallet extra and minus balance failed: {ex}", result.InternalErrorMessage);
                }
                _logger.LogInformation("Update all wallet extra and minus balance successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when update all wallet extra and minus balance: {ex}", ex);
            }
        }
    }
}
