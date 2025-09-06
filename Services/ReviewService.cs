using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Cryptography;
using AutoMapper;

using AutoMapper;
using AutoMapper.QueryableExtensions; // <-- needed for ProjectTo
using Microsoft.EntityFrameworkCore;  // <-- for ToListAsync


namespace E_CommerceSystem.Services
{
    public class ReviewService : IReviewService
    {
        public IReviewRepo _reviewRepo;
        public IProductService _productService;
        public IOrderService _orderService;
        public IOrderProductsService _orderProductsService;
        private readonly IMapper _mapper;

        public ReviewService(IReviewRepo reviewRepo, IProductService productService, IOrderProductsService orderProductsService, IOrderService orderService, IMapper mapper)
        {
            _reviewRepo = reviewRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _orderService = orderService;
            _mapper = mapper;
        }
        public IEnumerable<Review> GetAllReviews(int pageNumber, int pageSize,int pid)
        {
            // Base query
            var query = _reviewRepo.GetReviewByProductId(pid);

            // Pagination
            var pagedProducts = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedProducts;
        }

        public Review GetReviewsByProductIdAndUserId(int pid, int uid)
        {
            return _reviewRepo.GetReviewsByProductIdAndUserId(pid,uid);
        }
        public Review GetReviewById(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {rid} not found.");
            return review;
        }

        public IEnumerable<Review> GetReviewByProductId(int pid)
        {
            return _reviewRepo.GetReviewByProductId(pid);
        }
        public void AddReview(int uid, int pid, ReviewDTO reviewDTO)
        {
            // 1) Must have purchased the product
            var userOrders = _orderService.GetOrderByUserId(uid);  // already available in your service:contentReference[oaicite:2]{index=2}
            var purchased = userOrders
                .SelectMany(o => _orderProductsService.GetOrdersByOrderId(o.OID))
                .Any(op => op.PID == pid);
            if (!purchased)
                throw new UnauthorizedAccessException("You can only review products you purchased.");

            // 2) Must not already have a review
            var existing = _reviewRepo.GetReviewsByProductIdAndUserId(pid, uid);
            if (existing != null)
                throw new InvalidOperationException("You have already reviewed this product.");

            // 3) Create & save review
            var review = _mapper.Map<Review>(reviewDTO, opt =>
            {
                opt.Items["pid"] = pid;
                opt.Items["uid"] = uid;
            });
            _reviewRepo.AddReview(review);

            // 4) Recalculate rating for THIS product only
            RecalculateProductRating(pid);
        }

        public void UpdateReview(int rid, ReviewDTO reviewDTO)
        {
            var review = GetReviewById(rid);

            review.ReviewDate = DateTime.Now;
            review.Rating = reviewDTO.Rating;
            review.Comment = reviewDTO.Comment;

            _reviewRepo.UpdateReview(review);
            RecalculateProductRating(review.Rating);
        }
        public void DeleteReview(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {rid} not found.");

            _reviewRepo.DeleteReview(rid);
            RecalculateProductRating(review.PID);
        }

        private void RecalculateProductRating(int pid)
        {
            var reviews = _reviewRepo.GetReviewByProductId(pid);  // filter by product exists in repo:contentReference[oaicite:4]{index=4}
            var product = _productService.GetProductById(pid);     // product model contains OverallRating:contentReference[oaicite:5]{index=5}

            var average = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            product.OverallRating = Convert.ToDecimal(average);
            _productService.UpdateProduct(product);
        }

    }
}
