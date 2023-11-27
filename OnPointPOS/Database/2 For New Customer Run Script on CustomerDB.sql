--Before run this Query need to make outlet and terminal in admin 

--********On customer DB Online ************ 


declare @OutletId as UNIQUEIDENTIFIER;
declare @TerminalId as UNIQUEIDENTIFIER;
declare @email as varchar(50);
declare @userName as varchar(50);

set @OutletId=(select id from Outlet);
set @TerminalId=(select id from Terminal);
set @email=(select Email from Outlet);
set @userName=(select Name from Outlet);

update OutletUser set outletid=@OutletId,Email=@email,UserName=@userName,Updated=GETDATE();
update CashDrawer set TerminalId=@TerminalId
update Setting set TerminalId=@TerminalId,OutletId=@OutletId,Created=GETDATE(),Updated=GETDATE() where TerminalId is not null and OutletId is not null


--********On customer DB Online ************ 

--IndexinginPOS

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


declare @TerminalId as UNIQUEIDENTIFIER;
set @TerminalId=(select id from Terminal);
update Printer set TerminalId=@TerminalId

***************************

update setting
set Code = Id

Tack för besöket och välkommen åter!   