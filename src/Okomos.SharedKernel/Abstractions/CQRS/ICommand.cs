namespace Okomos.SharedKernel.Abstractions.CQRS;

public interface ICommand;

public interface ICommand<out TResult> : ICommand;
