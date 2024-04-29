using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Orders
{
    public interface IOrderService
    {
        Task<GenericData<OrderViewModel>> GetOrders(OrderPage model);
        Task<OrderDetailViewModel> GetOrderDetail(Guid orderId);
        Task UpdateOrder(EntityStatus status);
        Task DeleteOrder(Guid orderId);
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

        public Task DeleteOrder(Guid productId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetailViewModel> GetOrderDetail(Guid billId)
        {
            throw new NotImplementedException();
        }

        public Task<GenericData<OrderViewModel>> GetOrders(OrderPage model)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOrder(EntityStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
