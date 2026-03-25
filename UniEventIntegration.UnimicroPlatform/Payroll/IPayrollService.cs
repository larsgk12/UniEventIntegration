namespace UniEventIntegration.UnimicroPlatform.Payroll;

public interface IPayrollService
{
    Task SendDialogportenUpdate(string source, CancellationToken cancellationToken = default);
}