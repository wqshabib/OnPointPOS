delete from terminal
delete from outlet

declare @OutletId as UNIQUEIDENTIFIER;
declare @TerminalId as UNIQUEIDENTIFIER;

set @OutletId=NEWID();
set @TerminalId=NEWID();

INSERT INTO [dbo].[Outlet]
           ([Id]
           ,[Name]
           ,[Address1]
           ,[City]
           ,[PostalCode]
           ,[BillPrinterId]
           ,[KitchenPrinterId]
           ,[IsDeleted]
           ,[Email]
           ,[WebUrl]
           ,[Phone]
           ,[OrgNo]
           ,[HeaderText]
           ,[FooterText]
           ,[TaxDescription]
           ,[Created]
           ,[Updated]
           ,[WarehouseID]
           ,[UniqueCode]
           ,[Active])
     VALUES
           (@OutletId
           ,'CustomerName'
           ,'Adress'
           ,'City'
           ,'511 03'
           ,1
           ,1
           ,0
           ,'info@possum.com'
           ,'www.possumsystem.com'
           ,'031-7882400'
           ,'720512-1093'
           ,'Customer Name'
           ,'Tack för besöket och välkommen åter!'
           ,'SE7205121093'
           ,getdate()
           ,getdate()
           ,'00000000-0000-0000-0000-000000000000'
           ,'01'
           ,1)



INSERT INTO [dbo].[Terminal]
           ([Id]
           ,[OutletId]
           ,[TerminalNo]
           ,[TerminalType]
           ,[UniqueIdentification]
           ,[Status]
           ,[RootCategoryId]
           ,[IsDeleted]
           ,[Created]
           ,[Updated])
     VALUES
           (@TerminalId
           ,@OutletId
           ,1
           ,'00000000-0000-0000-0000-000000000000'
           ,'SEPOS-0001'
           ,0
           ,1
           ,0
           ,getdate()
           ,getdate())

		


update CashDrawer set TerminalId=@TerminalId

update Setting set TerminalId=@TerminalId,OutletId=@OutletId where TerminalId is not null and OutletId is not null







