using MassTransit;
using OneOf;

namespace WebApplication1;

public class PersonCommandHandler : CommandHandler<PersonCommand, OneOf<int>>
{
    protected override async Task<OneOf<int>> Handle(PersonCommand request, CancellationToken cancellationToken)
    {
        return request.Id;
    }
}

public class PersonCommand : ICommand<int>
{
    public PersonCommand(int id)
    {
        this.Id = id;
    }
    public int Id { get; set; }
}

public interface ICommand<TResponse>
{
}

public class ResponseWrapper<TResponse>
{
    public ResponseWrapper(TResponse response)
    {
        this.Response = response;
    }


    public TResponse Response { get; set; }
}

public abstract class CommandHandler<TRequest, TResponse> : IConsumer<TRequest>
    where TRequest : class
{
    public async Task Consume(ConsumeContext<TRequest> context)
    {
        var result = await this.Handle(context.Message, context.CancellationToken);

        await context.RespondAsync(new ResponseWrapper<TResponse>(result));
        
        // This way it works
        
        /*await context.RespondAsync(new ResponseWrapper<int>(123));*/
    }

    protected abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public class CommandSender
{
    private readonly IServiceProvider serviceProvider;
    public CommandSender(IServiceProvider serviceProvider) 
        => this.serviceProvider = serviceProvider;

    public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, ICommand<TResponse>
    {
        var requestClient = this.serviceProvider.GetRequiredService<IRequestClient<TRequest>>();
        
        var response = await requestClient.GetResponse<ResponseWrapper<TResponse>>(request, cancellationToken);
        
        return response.Message.Response;
    }
}