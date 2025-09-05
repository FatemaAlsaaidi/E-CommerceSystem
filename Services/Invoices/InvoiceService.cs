using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using E_CommerceSystem.Repositories;
using System.Linq;

namespace E_CommerceSystem.Services
{
    public sealed class InvoiceService : IInvoiceService
    {
        private readonly IOrderRepo _orders;
        private readonly IOrderProductsRepo _items;
        private readonly IProductRepo _products;
        private readonly IUserRepo _users;

        public InvoiceService(IOrderRepo o, IOrderProductsRepo i, IProductRepo p, IUserRepo u)
            => (_orders, _items, _products, _users) = (o, i, p, u);

        public byte[] Generate(int orderId)
        {
            var order = _orders.GetOrderById(orderId) ?? throw new KeyNotFoundException("Order not found.");
            var user = _users.GetUserById(order.UID) ?? throw new KeyNotFoundException("User not found.");
            var lines = _items.GetAllOrders().Where(x => x.OID == orderId).ToList();
            var prods = _products.GetAllProducts().ToDictionary(p => p.PID);

            var model = lines.Select(l => new {
                Name = prods[l.PID].ProductName,
                Qty = l.Quantity,
                Price = prods[l.PID].Price,
                LineTotal = prods[l.PID].Price * l.Quantity
            }).ToList();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Text($"Invoice #{order.OID}").FontSize(18).SemiBold();
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Customer: {user.UName}  |  {user.Email}");
                        col.Item().Text($"Order Date: {order.OrderDate:yyyy-MM-dd HH:mm}");
                        col.Item().Text($"Status: {order.Status}");

                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(40);  // #
                                c.RelativeColumn(6);   // Name
                                c.ConstantColumn(60);  // Qty
                                c.ConstantColumn(80);  // Price
                                c.ConstantColumn(90);  // Total
                            });

                            t.Header(h =>
                            {
                                h.Cell().Text("#").SemiBold();
                                h.Cell().Text("Item").SemiBold();
                                h.Cell().Text("Qty").SemiBold();
                                h.Cell().Text("Price").SemiBold();
                                h.Cell().Text("Total").SemiBold();
                            });

                            for (int i = 0; i < model.Count; i++)
                            {
                                var m = model[i];
                                t.Cell().Text((i + 1).ToString());
                                t.Cell().Text(m.Name);
                                t.Cell().Text(m.Qty.ToString());
                                t.Cell().Text($"{m.Price:C}");
                                t.Cell().Text($"{m.LineTotal:C}");
                            }

                            t.Footer(f =>
                            {
                                f.Cell().ColumnSpan(4).AlignRight().Text("Grand Total:").SemiBold();
                                f.Cell().Text($"{order.TotalAmount:C}").SemiBold();
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text("Thank you for your purchase!");
                });
            }).GeneratePdf();
        }
    }
}
