using System;
using System.Threading.Tasks;

namespace ServiceRequest
{
    public static class RequestResultSettingsExtensions
    {
        public static R OnSuccessComplete<R>(this R request, Action onSuccesCallback) where R : Request
        {
            if (request == null) { return request; }

            request.OnSuccess += onSuccesCallback;

            return request;
        }

        public static R OnFailureComplete<R>(this R request, Action<string> onFailureCallback) where R : Request
        {
            if (request == null) { return request; }

            request.OnFailure += onFailureCallback;

            return request;
        }

        public static R OnFinalyComplete<R>(this R request, Action onFinalyCallback) where R : Request
        {
            if (request == null) { return request; }

            request.OnFinaly += onFinalyCallback;

            return request;
        }

        public static R OnSuccessComplete<R, T>(this R request, Action<T> onSuccesCallback) where R : Request<T>
        {
            if (request == null) { return request; }

            request.OnSuccess += onSuccesCallback;

            return request;
        }

        public static R OnFailureComplete<R, T>(this R request, Action<string> onFailureCallback) where R : Request<T>
        {
            if (request == null) { return request; }

            request.OnFailure += onFailureCallback;

            return request;
        }

        public static R OnFinalyComplete<R, T>(this R request, Action onFinalyCallback) where R : Request<T>
        {
            if (request == null) { return request; }

            request.OnFinaly += onFinalyCallback;

            return request;
        }
    }

    public class Request
    {
        public Action OnSuccess { get; protected internal set; }
        public Action<string> OnFailure { get; protected internal set; }
        public Action OnFinaly { get; protected internal set; }

        public bool IsSuccess { get; protected set; }
        public string ErrorMessage { get; protected set; }

        protected Func<Task> mainAction;
        protected bool isStarted = false;

        public Request(Func<Task> playAction)
        {
            mainAction = playAction;
        }

        public void Start() => _ = StartAsync();

        public async Task StartAndWait() => await StartAsync();

        protected virtual async Task StartAsync()
        {
            if (isStarted) { return; }
            isStarted = true;

            if (mainAction != null)
            {
                try
                {
                    await mainAction.Invoke();
                    Success();
                }
                catch (Exception e)
                {
                    Failure(e.Message);
                }
                finally
                {
                    OnFinaly?.Invoke();
                }
            }
        }

        public Request Success()
        {
            IsSuccess = true;
            ErrorMessage = string.Empty;
            OnSuccess?.Invoke();

            return this;
        }

        public Request Failure(string errorMessage)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
            OnFailure?.Invoke(errorMessage);

            return this;
        }
    }

    public class Request<T> : Request
    {
        public new Action<T> OnSuccess { get; protected internal set; }

        public T Result { get; protected set; }

        protected new Func<Task<T>> mainAction;

        public Request(Func<Task<T>> playAction) : base(null)
        {
            mainAction = playAction;
        }

        protected override async Task StartAsync()
        {
            if (isStarted) { return; }
            isStarted = true;

            if (mainAction != null)
            {
                try
                {
                    Success(await mainAction.Invoke());
                }
                catch (Exception e)
                {
                    Failure(e.Message);
                }
                finally
                {
                    OnFinaly?.Invoke();
                }
            }
        }

        public Request<T> Success(T resultValue)
        {
            IsSuccess = true;
            ErrorMessage = string.Empty;
            Result = resultValue;
            OnSuccess?.Invoke(Result);

            return this;
        }

        public new Request<T> Failure(string errorMessage)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
            Result = default;
            OnFailure.Invoke(ErrorMessage);

            return this;
        }
    }
}
