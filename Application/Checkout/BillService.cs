using Domain.Abstractions;
using Domain.Entities;

namespace Application.Checkout
{
    public interface IBillService
    {
        Task CreateBill(BillCreateViewModel model);
    }
    public class BillService : IBillService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Bill, Guid> _billRepository;
        private readonly IRepository<OrderDetail, Guid> _billDetailRepository;

        public BillService(IUnitOfWork unitOfWork, IRepository<Bill, Guid> billRepository, IRepository<OrderDetail, Guid> billDetailRepository)
        {
            _unitOfWork = unitOfWork;
            _billRepository = billRepository;
            _billDetailRepository = billDetailRepository;
        }
        public async Task CreateBill(BillCreateViewModel model)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            if (transaction == null) return;
            try
            {
                var bill = new Bill
                {
                    CustomerName = model.CustomerName,
                    CityProvince = model.CityProvince,
                    DistrictTown = model.DistrictTown,
                    WardCommune = model.WardCommune,
                    ExactAddress = model.ExactAddress,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Note = model.Note,
                    TotalAmount = model.BillDetails.Sum(s => s.Quantity * s.Price),
                    PaymentMethod = model.PaymentMethod,
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    Status = Domain.Enums.EntityStatus.Pending,

                };
                await _billRepository.Add(bill);
                await _unitOfWork.SaveChangesAsync();
                foreach (var item in model.BillDetails)
                {
                    await CreateBillDetail(bill, item);
                }
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw new Exception("Create bill failed");
            }
        }

        private async Task CreateBillDetail(Bill bill, BillDetailCreateViewModel item)
        {
            var billDetail = new OrderDetail
            {
                Id = Guid.NewGuid(),
                BillId = bill.Id,
                CreatedDate = bill.CreatedDate,
                Status = Domain.Enums.EntityStatus.Active,
                ProductName = item.ProductName,
                UnitPrice = item.Price,
                Quantity = item.Quantity,
            };
            await _billDetailRepository.Add(billDetail);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
