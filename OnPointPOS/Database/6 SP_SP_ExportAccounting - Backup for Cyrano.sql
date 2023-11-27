USE [CyranoOlskroken]
GO
/****** Object:  StoredProcedure [dbo].[SP_ExportAccounting]    Script Date: 10/7/2022 3:00:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[SP_ExportAccounting] 	
@TerminalId AS uniqueidentifier,
	@OpenDate AS DATETIME,
	@CloseDate AS DATETIME
AS
BEGIN
CREATE TABLE #TempSale(
	[DataType] [nvarchar](50) NOT NULL,	
	[DataTypeText] [nvarchar](150) NOT NULL,
	[SaleDay] [nvarchar](50) NOT NULL,		
	[DataValue][nvarchar](50) NOT NULL,	
	[SortOrder] [int] NULL,
	[Amount] decimal(12,2) NULL

)

declare @accountingTotal decimal(12,2);
set @accountingTotal=0;
insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT   PaymentType.AccountingCode,PaymentType.Name, cast(OrderMaster.InvoiceDate as date) as SaleDay,cast( cast(ISNULL(Sum(Payment.PaidAmount),0) as numeric(12,2)) as varchar(20)),cast(ISNULL(Sum(Payment.PaidAmount),0) as numeric(12,2)),1 FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
				inner join PaymentType on PaymentType.Id=Payment.PaymentType
					WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=13 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = 1  AND Payment.PaidAmount >0 AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date),PaymentType.AccountingCode,PaymentType.Name order by cast(OrderMaster.InvoiceDate as date)

declare @ReturnRound as decimal(12,2);

	SELECT @ReturnRound= cast(ISNULL(Sum(RoundedAmount),0) as numeric(12,2)) FROM OrderMaster 
					WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=15 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId
--		12-10-2022: Dricks in swedish which is use for tipamount---	
insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT '2820','Dricks', cast(OrderMaster.InvoiceDate as date) as SaleDay,isnull(Sum(Payment.TipAmount),0),isnull(Sum(Payment.TipAmount),0),1000
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' AND    
					OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType=9 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  
					OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date)

insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT  '3740','Rounded Amount', cast(OrderMaster.InvoiceDate as date) as SaleDay,CAST( cast((ISNULL(Sum((-1)*RoundedAmount),0))+@ReturnRound as numeric(12,2)) as varchar(20)),cast(ISNULL(Sum((-1)*RoundedAmount),0)+@ReturnRound as numeric(12,2)),2 FROM OrderMaster 
					WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=13 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date)

DECLARE @LoopCounter INT = 1, @MaxAccountingId INT  , 
        @AccountingId int,@AcNo varchar(50),@Title varchar(100), @SortOrder INT
 select @MaxAccountingId=MAX(Id) from Accounting; 
 

 insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT  Tax.AccountingCode,CAST( OrderDetail.TaxPercent as varchar(10))+'%',cast(OrderMaster.InvoiceDate as date) as SaleDay,cast(cast((-1)*isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) as numeric(12,2))as varchar(20)),(-1)*isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) ,3
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Tax on OrderDetail.TaxPercent=Tax.TaxValue
				WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=13 AND    OrderMaster.InvoiceGenerated=1 AND  OrderDetail.Active=1 AND OrderDetail.ItemType<>1 AND OrderDetail.Direction=1  AND OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId
				GROUP BY OrderDetail.TaxPercent, cast(OrderMaster.InvoiceDate as date),Tax.AccountingCode order by Tax.AccountingCode asc




	
   set @SortOrder=4;
	WHILE(@LoopCounter <= @MaxAccountingId)
 BEGIN
 set @AcNo=null;
 SELECT @AccountingId = Id
   FROM Accounting WHERE Id = @LoopCounter   
   
   select @AcNo= null;--cast(AccountingTerminal.AcNo as varchar(10)) from AccountingTerminal where AccountingId=@AccountingId AND TerminalId=@TerminalId; 

if  @AcNo is null
select @AcNo=cast(Accounting.AcNo as varchar(10)),@Title= Accounting.Name from Accounting where Id=@AccountingId
else
select @Title= Accounting.Name from Accounting where Id=@AccountingId
 
	  set @SortOrder=@SortOrder+1;
	INSERT INTO #TempSale (DataType, DataTypeText,SaleDay,DataValue,Amount, SortOrder)
	SELECT @AcNo,@Title,cast(OrderMaster.InvoiceDate as date) as SaleDay,Cast(Cast((-1)*Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))) as numeric(12,2))as varchar(20)),(-1)*Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))),@SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product as Item on Item.Id=OrderDetail.itemID	
				WHERE OrderMaster.TrainingMode=0 AND Item.AccountingId=@AccountingId AND  OrderMaster.Status=13 AND   OrderMaster.InvoiceGenerated=1 AND  OrderDetail.Active=1  AND OrderDetail.ItemType<>1 AND OrderDetail.Direction=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate   AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date),Item.AccountingId;
					
   SET @LoopCounter  = @LoopCounter  + 1        
END		

--Cash out on Card 			
insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT   '-1','Cash on Kort', cast(OrderMaster.InvoiceDate as date) as SaleDay,cast((-1) *cast(ISNULL(Sum(Payment.CashCollected),0) as numeric(12,2)) as varchar(20)),cast(ISNULL(Sum(Payment.CashChange),0) as numeric(12,2)),49 
           FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
				inner join PaymentType on PaymentType.Id=Payment.PaymentType
					WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=13 AND Payment.PaymentType=4 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate   AND Payment.CashChange <>0 AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date),PaymentType.AccountingCode,PaymentType.Name order by cast(OrderMaster.InvoiceDate as date)
		
--return order payment			
insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT   PaymentType.AccountingCode,PaymentType.Name, cast(OrderMaster.InvoiceDate as date) as SaleDay,cast((-1) *cast(ISNULL(Sum(Payment.PaidAmount),0) as numeric(12,2)) as varchar(20)),cast(ISNULL(Sum(Payment.PaidAmount),0) as numeric(12,2)),50 FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
				inner join PaymentType on PaymentType.Id=Payment.PaymentType
					WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=15 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = -1  AND Payment.PaidAmount <>0 AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date),PaymentType.AccountingCode,PaymentType.Name order by cast(OrderMaster.InvoiceDate as date)

--Return TAX

 insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT  Tax.AccountingCode,CAST( OrderDetail.TaxPercent as varchar(10))+'%',cast(OrderMaster.InvoiceDate as date) as SaleDay,cast(cast(isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) as numeric(12,2))as varchar(20)),(-1)*isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0),51
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Tax on OrderDetail.TaxPercent=Tax.TaxValue
				WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=15 AND    OrderMaster.InvoiceGenerated=1 AND  OrderDetail.Active=1 AND OrderDetail.ItemType<>1  AND OrderDetail.Direction=-1 AND OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId
				GROUP BY OrderDetail.TaxPercent, cast(OrderMaster.InvoiceDate as date),Tax.AccountingCode order by Tax.AccountingCode asc


--Return  sale
 
  set @SortOrder=52;
  set @LoopCounter=1;
	WHILE(@LoopCounter <= @MaxAccountingId)
 BEGIN

 SELECT @AccountingId = Id
   FROM Accounting WHERE Id = @LoopCounter   
  
 select @AcNo=cast(Accounting.AcNo as varchar(10)),@Title= Accounting.Name from Accounting where Id=@AccountingId
	  set @SortOrder=@SortOrder+1;
	INSERT INTO #TempSale (DataType, DataTypeText,SaleDay,DataValue,Amount, SortOrder)
	SELECT @AcNo,@Title,cast(OrderMaster.InvoiceDate as date) as SaleDay,Cast(Cast(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))) as numeric(12,2))as varchar(20)),(-1)*Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))),@SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product as Item on Item.Id=OrderDetail.itemID	
				WHERE OrderMaster.TrainingMode=0 AND Item.AccountingId=@AccountingId AND  OrderMaster.Status=15 AND   OrderMaster.InvoiceGenerated=1 AND  OrderDetail.Active=1 AND OrderDetail.ItemType<>1  AND OrderDetail.Direction=-1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate   AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date),Item.AccountingId;
					
   SET @LoopCounter  = @LoopCounter  + 1        
END	

select *from #TempSale order by SaleDay
END

