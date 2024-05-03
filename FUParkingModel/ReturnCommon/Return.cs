namespace FUParkingModel.ReturnCommon
{
    public class Return<T>
    {
        public T? Data { get; set; }
        public string? SuccessfullyMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public string? InternalErrorMessage { get; set; }
        public bool IsSuccess { get; set; } = false;
    }
}
