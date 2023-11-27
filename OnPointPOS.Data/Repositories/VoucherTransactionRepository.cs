using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class VoucherTransactionRepository
    {
        public List<VoucherTransaction> GetTransactions(DateTime dtFrom, DateTime dtTo)
        {
            var transactions = new List<VoucherTransaction>();
            using (var db = new ApplicationDbContext())
            {

                transactions =
                    db.VoucherTransaction.Where(
                            t => t.TransactionDate >= dtFrom && t.TransactionDate <= dtTo && t.Canceled == false).ToList()
                        .Select(
                            t =>
                                new VoucherTransaction
                                {
                                    Id = t.Id,
                                    Product = t.Product,
                                    OrderId = t.OrderId,
                                    ErsReference = t.ErsReference,
                                    Canceled = t.Canceled,
                                    TransactionDate = t.TransactionDate,
                                    ItemName = t.Product.Description,
                                    SKU = t.Product.SKU
                                })
                        .ToList();
            }


            return transactions;
        }

        public bool CancelTransaction(List<VoucherTransaction> transactions)
        {

            using (var db = new ApplicationDbContext())
            {

                foreach (var line in transactions)
                {
                    var transaction = db.VoucherTransaction.FirstOrDefault(t => t.Id == line.Id);
                    if (transaction != null) transaction.Canceled = true;

                }
                db.SaveChanges();
                return true;
            }

        }
    }
}
