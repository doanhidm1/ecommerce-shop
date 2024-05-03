using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders
{
    public interface IOrderService
    {
        Task<GenericData<OrderViewModel>> GetOrders(OrderPage model);
        Task<OrderDetailViewModel> GetOrderDetail(Guid orderId);
        Task UpdateOrder(OrderUpdateViewModel model);
    }

    public class OrderService : IOrderService
    {
        private readonly IRepository<Bill, Guid> _orderRepository;
        private readonly IRepository<OrderDetail, Guid> _orderDetailRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IRepository<Bill, Guid> orderRepository, IRepository<OrderDetail, Guid> orderDetailRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderDetailViewModel> GetOrderDetail(Guid billId)
        {
            var order = await _orderRepository.FindById(billId) ?? throw new Exception("Order not found");
            var allOrderDetails = _orderDetailRepository.GetAll();
            var result = new OrderDetailViewModel
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                OrderDate = order.CreatedDate,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status,
                TotalPrice = order.TotalAmount,
                OrderDetailItems = await allOrderDetails.Where(x => x.BillId == billId).ToListAsync()
            };
            return result;
        }

        public async Task<GenericData<OrderViewModel>> GetOrders(OrderPage filter)
        {
            var data = new GenericData<OrderViewModel>();
            var allOrders = _orderRepository.GetAll();

            var result = allOrders.Select(x => new OrderViewModel
            {
                OrderId = x.Id,
                CustomerName = x.CustomerName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Address = $"{x.ExactAddress}, {x.WardCommune}, {x.DistrictTown}, {x.CityProvince}",
                OrderDate = x.CreatedDate,
                PaymentMethod = x.PaymentMethod,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                Note = x.Note
            });

            if (!string.IsNullOrEmpty(filter.CustomerName))
            {
                result = result.Where(x => x.CustomerName.Contains(filter.CustomerName));
            }

            if (!string.IsNullOrEmpty(filter.Email))
            {
                result = result.Where(x => x.Email.Contains(filter.Email));
            }

            if (!string.IsNullOrEmpty(filter.Phone))
            {
                result = result.Where(x => x.PhoneNumber.Contains(filter.Phone));
            }

            if (filter.Status != null)
            {
                result = result.Where(x => x.Status == filter.Status);
            }

            switch (filter.OrderBy)
            {
                case SortEnum.Name:
                    result = result.OrderBy(x => x.CustomerName);
                    break;
                case SortEnum.Date:
                    result = result.OrderBy(x => x.OrderDate);
                    break;
                case SortEnum.Price:
                    result = result.OrderBy(x => x.TotalAmount);
                    break;
                default:
                    break;
            }

            var x = await result.Skip(filter.SkipNumber).Take(filter.PageSize).ToListAsync();

            if (!string.IsNullOrEmpty(filter.CityProvince))
            {
                x = x.Where(x => x.Address.Contains(filter.CityProvince)).ToList();
            }

            if (!string.IsNullOrEmpty(filter.DistrictTown))
            {
                x = x.Where(x => x.Address.Contains(filter.DistrictTown)).ToList();
            }

            if (!string.IsNullOrEmpty(filter.WardCommune))
            {
                x = x.Where(x => x.Address.Contains(filter.WardCommune)).ToList();
            }

            data.Count = x.Count;
            data.Data = x;
            return data;
        }

        public async Task UpdateOrder(OrderUpdateViewModel model)
        {
            var order = await _orderRepository.FindById(model.OrderId) ?? throw new Exception("Order not found");
            order.Status = model.Status;
            await _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
