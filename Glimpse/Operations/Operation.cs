using MediatR;

namespace Glimpse.Operations
{
    public abstract class Operation
    {
        public interface IRequest<TResult> : MediatR.IRequest<Response<TResult>>
        {
        }

        public interface IResponse
        {
            public IEnumerable<string> Errors { get; }
            public bool IsValid { get; }
        }

        public class Response<T> : IResponse
        {
            public Response(T result, IEnumerable<string> errors)
            {
                Result = result;
                Errors = errors;
            }

            public T Result { get; }
            public IEnumerable<string> Errors { get; } = Enumerable.Empty<string>();
            public bool IsValid => !Errors.Any();
        }

        public abstract class Handler<TRequest, TResult> : IRequestHandler<TRequest, Response<TResult>>
            where TRequest : Operation.IRequest<TResult>
        {
            private readonly List<string> _errors = new();

            protected bool HasErrors => _errors.Any();

            public async Task<Response<TResult>> Handle(TRequest request, CancellationToken cancellationToken)
            {
                var result = await DoOperation(request, cancellationToken);

                return new Response<TResult>(result, _errors);
            }

            protected abstract Task<TResult> DoOperation(TRequest request, CancellationToken cancellationToken);

            protected TResult Error(string error)
            {
                _errors.Add(error);
                return default;
            }
        }
    }
}
