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
        private readonly IRepository<BillDetail, Guid> _billDetailRepository;

        public BillService(IUnitOfWork unitOfWork, IRepository<Bill, Guid> billRepository, IRepository<BillDetail, Guid> billDetailRepository)
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
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Street = model.Street,
                    City = model.City,
                    Country = model.Country,
                    ZipCode = model.ZipCode,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Note = model.Note,
                    TotalAmount = model.BillDetails.Sum(s => s.Quantity * s.Price),
                    PaymentMethod = model.PaymentMethod,
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    Status = Domain.Enums.EntityStatus.Active,

                };
                await _billRepository.Add(bill);
                await _unitOfWork.SaveChangesAsync();
                foreach (var item in model.BillDetails)
                {
                    var billDetail = new BillDetail
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
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }
    }
}
