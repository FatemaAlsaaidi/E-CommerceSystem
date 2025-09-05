using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using E_CommerceSystem.Services.Email;

using AutoMapper;
using AutoMapper.QueryableExtensions; // <-- needed for ProjectTo
using Microsoft.EntityFrameworkCore;  // <-- for ToListAsync

using System.Transactions;


namespace E_CommerceSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductService _productService;
        private readonly IOrderProductsService _orderProductsService;
        private readonly IMapper _mapper;
        private readonly IUserRepo _userRepo;
        private readonly IEmailSender _email;            
        private readonly IInvoiceService _invoice;       

        public OrderService(
            IOrderRepo orderRepo,
            IProductService productService,
            IOrderProductsService orderProductsService,
            IMapper mapper,
            IUserRepo userRepo,
            IEmailSender email,           
            IInvoiceService invoice          
        )
        {
            _orderRepo = orderRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _mapper = mapper;
            _userRepo = userRepo;
            _email = email;                  // <-- add
            _invoice = invoice;              // <-- add
        }

        //get all orders for login user
        public List<OrderProducts> GetAllOrders(int uid)
        {
            var orders = _orderRepo.GetOrderByUserId(uid);
            if (orders == null || !orders.Any())
                throw new InvalidOperationException($"No orders found for user ID {uid}.");

            // Collect all OrderProducts for all orders
            var allOrderProducts = new List<OrderProducts>();

            foreach (var order in orders)
            {
                var orderProducts = _orderProductsService.GetOrdersByOrderId(order.OID);
                if (orderProducts != null)
                    allOrderProducts.AddRange(orderProducts);
            }

            return allOrderProducts;

        }

        //get order by order id for the login user
        public IEnumerable<OrdersOutputOTD> GetOrderById(int oid, int uid)
        {
            //list of items in the order 
            List<OrdersOutputOTD> items = new List<OrdersOutputOTD>();
            OrdersOutputOTD ordersOutputOTD = null;


            List<OrderProducts> products = null;
            Product product = null;
            string productName = string.Empty;

            //get order 
            var order = _orderRepo.GetOrderById(oid);

            if (order == null)
                throw new InvalidOperationException($"No orders found .");

            //execute the products data in existing Product
            if (order.UID == uid)
            {
                products = _orderProductsService.GetOrdersByOrderId(oid);
                foreach (var p in products)
                {
                    product = _productService.GetProductById(p.PID);
                    productName = product.ProductName;
                    ordersOutputOTD = new OrdersOutputOTD
                    {
                        ProductName = productName,
                        Quantity = p.Quantity,
                        OrderDate = order.OrderDate,
                        TotalAmount = p.Quantity * product.Price,
                    };
                    items.Add(ordersOutputOTD);
                }
            }

            return items;

        }

        public IEnumerable<Order> GetOrderByUserId(int uid)
        {
            var order = _orderRepo.GetOrderByUserId(uid);
            if (order == null)
                throw new KeyNotFoundException($"order with user ID {uid} not found.");

            return order;
        }

        public void DeleteOrder(int oid)
        {
            var order = _orderRepo.GetOrderById(oid);
            if (order == null)
                throw new KeyNotFoundException($"order with ID {oid} not found.");

            _orderRepo.DeleteOrder(oid);
            throw new Exception($"order with ID {oid} is deleted");
        }
        public void AddOrder(Order order)
        {
            _orderRepo.AddOrder(order);
        }
        public void UpdateOrder(Order order)
        {
            _orderRepo.UpdateOrder(order);
        }

        //Places an order for the given list of items and user ID.
        public void PlaceOrder(List<OrderItemDTO> items, int uid)
        {
            // Temporary variable to hold the currently processed product
            Product existingProduct = null;

            decimal TotalPrice, totalOrderPrice = 0; // Variables to hold the total price of each item and the overall order

            OrderProducts orderProducts = null;

            // Validate all items in the order
            for (int i = 0; i < items.Count; i++)
            {
                TotalPrice = 0;
                existingProduct = _productService.GetProductByName(items[i].ProductName);
                if (existingProduct == null)
                    throw new Exception($"{items[i].ProductName} not Found");

                if (existingProduct.Stock < items[i].Quantity)
                    throw new Exception($"{items[i].ProductName} is out of stock");

            }
            // Create a new order for the user using auto mapping 
            var order = new Order { UID = uid, OrderDate = DateTime.Now, TotalAmount = 0 };
            AddOrder(order); // Save the order to the database

            // Process each item in the order
            foreach (var item in items)
            {
                // Retrieve the product by its name
                existingProduct = _productService.GetProductByName(item.ProductName);

                // Calculate the total price for the current item
                TotalPrice = item.Quantity * existingProduct.Price;

                // Deduct the ordered quantity from the product's stock
                existingProduct.Stock -= item.Quantity;

                // Update the overall total order price
                totalOrderPrice += TotalPrice;

                // Create a relationship record between the order and product
                orderProducts = new OrderProducts { OID = order.OID, PID = existingProduct.PID, Quantity = item.Quantity };
                _orderProductsService.AddOrderProducts(orderProducts);

                // Update the product's stock in the database
                _productService.UpdateProduct(existingProduct);

            }

            // Update the total amount of the order
            order.TotalAmount = totalOrderPrice;
            UpdateOrder(order);

            // Fetch user to get email
            var user = _userRepo.GetUserById(uid);
            var subject = $"Your order #{order.OID} has been placed";
            var body = $@"
            <h2>Thanks for your order, {user.UName}!</h2>
            <p>Order ID: <b>{order.OID}</b></p>
            <p>Total: <b>{order.TotalAmount:C}</b></p>
            <p>Status: <b>{order.Status}</b></p>
        ";

            // Optional: generate invoice PDF and attach
            byte[]? pdf = _invoice.Generate(order.OID);

            _ = _email.SendAsync(user.Email, subject, body, pdf, $"Invoice_{order.OID}.pdf");
        }

        public void Cancel(int oid, int uid)
        {
            var order = _orderRepo.GetOrderById(oid);
            if (order == null)
                throw new KeyNotFoundException($"order with ID {oid} not found.");
            if (order.UID != uid)
                throw new UnauthorizedAccessException("You are not authorized to cancel this order.");
            // Retrieve all products associated with the order
            var orderProducts = _orderProductsService.GetOrdersByOrderId(oid);
            if (orderProducts == null || !orderProducts.Any())
                throw new InvalidOperationException("No products found for this order.");
            // Restore stock for each product in the order
            foreach (var op in orderProducts)
            {
                var product = _productService.GetProductById(op.PID);
                if (product != null)
                {
                    product.Stock += op.Quantity; // Restore the stock
                    _productService.UpdateProduct(product); // Update the product in the database
                }
            }
            // Delete the order
            _orderRepo.DeleteOrder(oid);
            var user = _userRepo.GetUserById(uid);
            var subject = $"Your order #{oid} has been cancelled";
            var body = $@"
            <h2>Order Cancelled</h2>
            <p>Order ID: <b>{oid}</b> was cancelled successfully.</p>
        ";

            _ = _email.SendAsync(user.Email, subject, body);




        }


        public void UpdateOrderStatus(int oid, OrderStatus newStatus, int requesterUid)
        {
            // 1) fetch the order
            var order = _orderRepo.GetOrderById(oid) ??
                        throw new KeyNotFoundException("Order not found.");

            // 2) اfetch the use data
            var requester = _userRepo.GetUserById(requesterUid); // تأكد أن لديك _userRepo
            var role = requester?.Role ?? "";
            bool isAdminOrManager = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                                 || role.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            // 3) statuse cancelled dose not able to change 
            if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
                throw new InvalidOperationException($"Order is already {order.Status}; status can no longer be changed.");

            // 4) change that allow to do 
            bool okNormal = order.Status switch
            {
                OrderStatus.Pending => newStatus is OrderStatus.Paid or OrderStatus.Cancelled or OrderStatus.Shipped,
                OrderStatus.Paid => newStatus is OrderStatus.Shipped or OrderStatus.Cancelled or OrderStatus.Delivered,
                OrderStatus.Shipped => newStatus is OrderStatus.Delivered or OrderStatus.Cancelled,
                _ => false
            };

            // 5) For the administrator/manager: Allow status changes to proceed "forward only" (e.g., from "Processing" to "Delivered"), while preventing reverting to a previous status and preventing changing the status to "Cancelled" outside of this framework.
            bool forwardOnlyForAdmins = false;
            if (isAdminOrManager)
            {
                //Arrange the cases numerically in the same order as your enum definition:
                static int Rank(OrderStatus s) => s switch
                {
                    OrderStatus.Pending => 0,
                    OrderStatus.Paid => 1,
                    OrderStatus.Shipped => 2,
                    OrderStatus.Delivered => 3,
                    OrderStatus.Cancelled => 99, 
                    _ => 100
                };

                //It allows progression to a later status (e.g., Pending->Delivered or Paid->Delivered), but it does not allow moving to the 'Cancelled' status.
                forwardOnlyForAdmins = newStatus is not OrderStatus.Cancelled
                                       && Rank(newStatus) > Rank(order.Status);
            }

            if (!(okNormal || forwardOnlyForAdmins))
                throw new InvalidOperationException($"Cannot change {order.Status} -> {newStatus}");

            // 6) Apply the change and save.
            order.Status = newStatus;
            _orderRepo.UpdateOrder(order);

            // 7) Notifications/emails based on status
            //if (newStatus == OrderStatus.Paid)
            //    _email.SendOrderPlaced(order.OID);
        }




    }
}

