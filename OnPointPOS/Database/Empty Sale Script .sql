truncate table [dbo].[Payment]
truncate table [dbo].[CashDrawerLog]
truncate table [dbo].[Journal]
delete from [dbo].[OrderDetail]
delete from [dbo].[OrderMaster]
truncate table [dbo].[Receipt]
truncate table [dbo].[UserLog]
truncate table [dbo].[Report]

truncate table [dbo].[BongCounter]
truncate table [dbo].[ReportData]
truncate table [dbo].[TerminalStatusLog]
truncate table [dbo].[ExceptionLog]
truncate table [dbo].[EmployeeLog]
truncate table [dbo].[VoucherTransaction]
truncate table [dbo].[CustomerInvoice]
truncate table [dbo].[InventoryTask]
update [dbo].[Terminal] set Status=0

update [dbo].[InvoiceCounter] set LastNo=0

truncate table [dbo].[DepositHistory]

update Customer set DepositAmount=0

