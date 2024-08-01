namespace FUParkingModel.ReturnCommon
{
    public class Return<T>
    {
        public T? Data { get; set; }
        public required string Message { get; set; }
        public int? TotalRecord { get; set; }
        public Exception? InternalErrorMessage { get; set; }
        public bool IsSuccess { get; set; } = false;
    }
}
