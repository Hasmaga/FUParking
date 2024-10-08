﻿using FUParkingModel.Object;
using FUParkingModel.ResponseObject.Payment;
using FUParkingModel.ResponseObject.Statistic;
using FUParkingModel.ReturnCommon;

namespace FUParkingRepository.Interface
{
    public interface IPaymentRepository
    {
        Task<Return<Payment>> CreatePaymentAsync(Payment payment);
        Task<Return<PaymentMethod>> CreatePaymentMethodAsync(PaymentMethod paymentMethod);
        Task<Return<PaymentMethod>> UpdatePaymentMethodAsync(PaymentMethod paymentMethod);
        Task<Return<PaymentMethod>> GetPaymentMethodByIdAsync(Guid paymentMethodId);
        Task<Return<IEnumerable<PaymentMethod>>> GetAllPaymentMethodAsync();
        Task<Return<PaymentMethod>> GetPaymentMethodByNameAsync(string name);
        Task<Return<Payment>> GetPaymentBySessionIdAsync(Guid sessionId);
        Task<Return<StatisticPaymentByCustomerResDto>> StatisticPaymentByCustomerAsync(Guid customerId);
        Task<Return<IEnumerable<StatisticSessionPaymentMethodResDto>>> StatisticSessionPaymentMethodByCustomerAsync(Guid customerId);
        Task<Return<IEnumerable<StatisticSessionPaymentMethodResDto>>> StatisticSessionPaymentMethodAsync();
        Task<Return<StatisticPaymentTodayResDto>> GetStatisticPaymentTodayForGateAsync(Guid gateId, DateTime? startDate, DateTime? endDate);
    }
}
