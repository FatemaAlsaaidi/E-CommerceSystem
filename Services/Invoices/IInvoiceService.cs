namespace E_CommerceSystem.Services
{
    public interface IInvoiceService
    {
        byte[] Generate(int orderId);
    }
}
