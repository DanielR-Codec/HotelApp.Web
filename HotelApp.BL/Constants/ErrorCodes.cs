namespace HotelApp.BL.Constants
{
    /// <summary>
    /// Generic codes used in BusinessRuleException.Code.
    /// Business rule / use case codes (RN-xxx, CU-xxx) refer to the analysis and design document and are used as-is.
    /// </summary>
    public static class ErrorCodes
    {
        public const string Validation = "VALIDATION";
        public const string Auth = "AUTH";
        public const string Forbidden = "FORBIDDEN";
        public const string NotFound = "NOT_FOUND";
    }
}
