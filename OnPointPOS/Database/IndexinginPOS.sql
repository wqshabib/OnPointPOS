GO  
--Create index on ReceiptNumber column of Receipt Table 
CREATE NONCLUSTERED INDEX Index_Receipt_ReceiptNumber   
    ON Receipt (ReceiptNumber);   
GO
GO  
--Create index on ReceiptNumber column of Receipt Table 
CREATE NONCLUSTERED INDEX Index_OrderDetail_Id   
    ON OrderDetail (Id)
	INCLUDE (OrderId);  
GO

GO  
--Create index on ReceiptNumber column of Receipt Table 
CREATE NONCLUSTERED INDEX Index_OrderMaster_Id   
    ON OrderMaster (Id);
GO
GO
--Create Index on OrderMaster InvoiceDate colum group with OrderId,OutletId
	CREATE INDEX Index_OrderMaster_InvoiceDate ON OrderMaster(InvoiceDate) 
  INCLUDE(Id,OutletId)
GO

