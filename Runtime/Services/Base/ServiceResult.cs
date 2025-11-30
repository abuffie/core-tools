namespace Aarware.Services {
    /// <summary>
    /// Represents the result of a service operation.
    /// </summary>
    public class ServiceResult {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }

        public ServiceResult(bool success, string errorMessage = null) {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public static ServiceResult Successful() {
            return new ServiceResult(true);
        }

        public static ServiceResult Failed(string errorMessage) {
            return new ServiceResult(false, errorMessage);
        }
    }

    /// <summary>
    /// Represents the result of a service operation with a return value.
    /// </summary>
    public class ServiceResult<T> {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }
        public T Data { get; private set; }

        public ServiceResult(bool success, T data = default, string errorMessage = null) {
            Success = success;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static ServiceResult<T> Successful(T data) {
            return new ServiceResult<T>(true, data);
        }

        public static ServiceResult<T> Failed(string errorMessage) {
            return new ServiceResult<T>(false, default, errorMessage);
        }
    }
}
