using POSSUM.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class ReceiptRepository : GenericRepository<Receipt>,IDisposable
    {
        private readonly ApplicationDbContext context;

        public ReceiptRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public ReceiptRepository() : base(new ApplicationDbContext())
        {
        }

        public void Create(Receipt receipt)
        {

            if (receipt.VatDetails.Count > 0)
                receipt.VatDetail = JsonConvert.SerializeObject(receipt.VatDetails);
            using (var uof = new UnitOfWork(new ApplicationDbContext()))
            {             
               
                uof.ReceiptRepository.Add(receipt);
                uof.Commit();
            }


        }

        public bool Update(Receipt receipt)
        {

            if (receipt.VatDetails.Count > 0)
                receipt.VatDetail = JsonConvert.SerializeObject(receipt.VatDetails);
            using (var uof = new UnitOfWork(new ApplicationDbContext()))
            {
                var receipRepo = uof.ReceiptRepository;
                var receiptData = receipRepo.FirstOrDefault(r => r.ReceiptId == receipt.ReceiptId);
                if (receiptData != null)
                {
                    receiptData.ReceiptCopies = receipt.ReceiptCopies;
                    if (!string.IsNullOrEmpty(receipt.ControlUnitName))
                        receiptData.ControlUnitName = receipt.ControlUnitName;
                    if (!string.IsNullOrEmpty(receipt.ControlUnitCode))
                        receiptData.ControlUnitCode = receipt.ControlUnitCode;
                    receiptData.MerchantPaymentReceipt = receipt.MerchantPaymentReceipt;
                    receiptData.CustomerPaymentReceipt = receipt.CustomerPaymentReceipt;
                    receipRepo.AddOrUpdate(receiptData);
                }
                uof.Commit();
                return true;
            }

        }

        public Receipt Get(Guid receiptId)
        {
           
                using (var uof = new UnitOfWork(new ApplicationDbContext()))
                {

                    var receipt = uof.ReceiptRepository.FirstOrDefault(r => r.ReceiptId == receiptId);
                    return FromDb(receipt);
                }
           
        }

        public Receipt GetByOrderId(Guid orderId)
        {
           
                using (var uof = new UnitOfWork(new ApplicationDbContext()))
                {
                    var receipt = uof.ReceiptRepository.FirstOrDefault(r => r.OrderId == orderId);
                    return FromDb(receipt);
                }
            
        }

        public Receipt GetLastReceipt()
        {

            using (var uof = new UnitOfWork(new ApplicationDbContext()))
            {
                var receipt = uof.ReceiptRepository.FirstOrDefault();
                return FromDb(receipt);
            }

        }

        public Receipt GetByUnitName(string unitName)
        {

            using (var uof = new UnitOfWork(new ApplicationDbContext()))
            {
                var receipt = uof.ReceiptRepository.FirstOrDefault(r => r.ControlUnitName == unitName);
                return FromDb(receipt);
            }

        }

        public Receipt FromDb(Receipt receipt)
        {
            if (receipt == null)
            {
                return null;
            }

            Receipt r = new Receipt
            {
                ReceiptId = receipt.ReceiptId,
                TerminalId = receipt.TerminalId,
                ReceiptNumber = receipt.ReceiptNumber,
                OrderId = receipt.OrderId,
                ReceiptCopies = receipt.ReceiptCopies,
                GrossAmount = receipt.GrossAmount,
                VatAmount = receipt.VatAmount,
                VatDetails = JsonConvert.DeserializeObject<List<VAT>>(receipt.VatDetail),
                PrintDate = receipt.PrintDate,
                ControlUnitName = receipt.ControlUnitName,
                ControlUnitCode = receipt.ControlUnitCode,
                CustomerPaymentReceipt = receipt.CustomerPaymentReceipt,
                MerchantPaymentReceipt = receipt.MerchantPaymentReceipt,
                IsSignature = receipt.IsSignature
            };
            return r;
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
