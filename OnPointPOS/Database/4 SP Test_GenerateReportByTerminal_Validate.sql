First create SP then then execute below query

DECLARE @RC int
DECLARE @TerminalId uniqueidentifier
DECLARE @ReportType int
DECLARE @ReportId uniqueidentifier
DECLARE @OpenDate datetime
DECLARE @CloseDate datetime

-- TODO: Set parameter values here.

SET @TerminalId = 'C9BF09E3-AE61-4F25-8F78-B7196292C587';
SET @ReportType = 1;
SET @ReportId =   NEWID();
SET @OpenDate = '2020-06-18 05:40:04.570';
SET @CloseDate = '2020-06-18 17:58:06.147';


EXECUTE @RC = [dbo].[GenerateReportByTerminal_Validate] 
   @TerminalId
  ,@ReportType
  ,@ReportId OUTPUT
  ,@OpenDate
  ,@CloseDate
GO



/****** Object:  StoredProcedure [dbo].[GenerateReportByTerminal]    Script Date: 28-Sep-20 2:38:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Muhammad Munir @ POSSUM
-- Create date: 2015.05.22
-- Last Updated By: Munir
-- Last Updated: 2016.01.26
-- Description:	Bus
-- =============================================
ALTER PROCEDURE [dbo].[GenerateReportByTerminal_Validate]
	-- Add the parameters for the stored procedure here
	@TerminalId UNIQUEIDENTIFIER,
	@ReportType INT,
	@ReportId UNIQUEIDENTIFIER OUTPUT,
	@OpenDate AS DATETIME,
	@CloseDate AS DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Declare @SortOrder AS INT;
	Declare @DepositInCash AS DECIMAL(18,2);
	Declare @DepositInReturnOrder AS DECIMAL(18,2);
	Declare @DepositInCard AS DECIMAL(18,2);

	Declare @DepositInCashTotal AS DECIMAL(18,2);
	Declare @DepositInReturnOrderTotal AS DECIMAL(18,2);
	Declare @DepositInCardTotal AS DECIMAL(18,2);
	Declare @DepositOutTotal AS DECIMAL(18,2);

	Declare @SubTotalAllCustomer AS DECIMAL(18,2);

	Declare @DepositOut AS DECIMAL(18,2);
	Declare @DepositSum AS DECIMAL(18,2);
	Declare @ReportNumber AS INT;
	Declare @OutletId AS INT;
	Declare @UniqueIdentification AS nvarchar(255);
	Declare @OpenCash AS DECIMAL(18,2);
	Declare @CashIn AS DECIMAL(18,2);
	Declare @CashInTip AS DECIMAL(18,2);
	Declare @CashOut AS DECIMAL(18,2);
	declare @CashOutPant as decimal(18,2);
	declare @CashOutLotter as decimal(18,2);
	Declare @CashOutReturnAmountMobile AS DECIMAL(18,2);
	Declare @CashOutTipMobile AS DECIMAL(18,2);
	Declare @CashOutTipAll AS DECIMAL(18,2);
	Declare @CashAdded AS DECIMAL(18,2);
	Declare @CashDropped AS DECIMAL(18,2);
	Declare @CashSum AS DECIMAL(18,2);
	Declare @NetTotal AS DECIMAL(12,2);
	Declare @VatSum AS DECIMAL(12,2);
	Declare @RoundingAmount AS DECIMAL(12,2);
	Declare @TotalPayment AS DECIMAL(12,2);
	Declare @SaleTotal AS DECIMAL(18,2);
	Declare @SaleReturn AS DECIMAL(18,2);

	SET @ReportId = NEWID();
	set @SortOrder=1;
	SELECT @ReportNumber = IsNull(Max(ReportNumber),0)+1 FROM Report WHERE Report.ReportType = @ReportType AND Report.TerminalId = @TerminalId;

	INSERT INTO Report (Id,CreationDate,ReportType,ReportNumber,TerminalId)
		VALUES (@ReportId,GETDATE(),@ReportType,@ReportNumber,@TerminalId);

	-- Set reportnumber on report
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder) VALUES (NEWID(), @ReportId, 'ReportNumber', @ReportNumber, @SortOrder);
	set @SortOrder=@SortOrder+1;

	-- Get unique identifier for terminal
	SELECT @UniqueIdentification = UniqueIdentification FROM Terminal WHERE Terminal.Id = @TerminalId

	INSERT INTO ReportData (Id, ReportId, DataType, TextValue, SortOrder) VALUES (NEWID(), @ReportId, 'UniqueIdentification', @UniqueIdentification, @SortOrder);
	set @SortOrder=@SortOrder+1;

	IF @ReportType = 1 
	BEGIN
		-- Get date last close
		-- set @CloseDate =isnull((SELECT  Max(ActivityDate) FROM TerminalStatusLog WHERE TerminalId = @TerminalId AND Status = 0),GETDATE());-- closing date should always GETDATE();--
		-- Get date last open
		-- set @OpenDate=ISNULL((SELECT  Max(ActivityDate) FROM TerminalStatusLog WHERE TerminalId = @TerminalId AND Status = 1 AND ActivityDate < @CloseDate),GETDATE());
		
		INSERT INTO ReportData (Id, ReportId, DataType, DateValue, SortOrder) VALUES (NEWID(),@ReportId,'OpenDate', @OpenDate, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO ReportData (Id, ReportId, DataType, DateValue, SortOrder) VALUES (NEWID(),@ReportId,'CloseDate', @CloseDate, @SortOrder);
		set @SortOrder=@SortOrder+1;

	END
	ELSE
	BEGIN

		-- Get date last close
		SET @CloseDate = GETDATE()
		-- INSERT INTO ReportData (Id, ReportId, DataType, DateValue, SortOrder) VALUES (NEWID(),@ReportId,'CloseDate', @CloseDate, 3);

		-- Get date last open
	
			set @OpenDate=ISNULL((SELECT  Max(ActivityDate) FROM TerminalStatusLog WHERE TerminalId = @TerminalId AND Status = 1),GETDATE());
	
			INSERT INTO ReportData (Id, ReportId, DataType, DateValue, SortOrder) VALUES (NEWID(),@ReportId,'OpenDate', @OpenDate, @SortOrder);
			set @SortOrder=@SortOrder+1;

	END

	
	-- ### Get total sale of given period

	SELECT @SaleTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 
				AND (OrderMaster.Status =13 or OrderMaster.Status = 17) AND OrderDetail.ItemType<>1 AND OrderDetail.Active=1 
				 and OrderDetail.Direction=1 AND  OrderMaster.TerminalId = @TerminalId;


SELECT @SaleReturn= isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount),0)
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND (OrderMaster.Status=15 or OrderMaster.Status = 18) AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderDetail.Direction = -1 AND OrderDetail.ItemType<>1 AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId;



	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	Values(NEWID(), @ReportId, 'TotalSale',@SaleTotal,@SortOrder);
	set @SortOrder=@SortOrder+1;
		
	-- ### Get total net of given period

	
	SELECT @NetTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)) 
   FROM OrderMaster LEFT JOIN OrderDetail 
    ON OrderMaster.Id = OrderDetail.OrderId     
     WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND (OrderMaster.Status=13 or OrderMaster.Status = 17)  AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1 AND OrderDetail.Direction=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  OrderMaster.TerminalId = @TerminalId;


	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
				VALUES(NEWID(), @ReportId, 'TotalNet',@NetTotal,@SortOrder);
				
	set @SortOrder=@SortOrder+1;
		
					-- ### Get total return of given period
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	values(NEWID(), @ReportId, 'TotalReturn',@SaleReturn,@SortOrder);
	set @SortOrder=@SortOrder+1;
		
-- ### Get total return of given period
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	Values(NEWID(), @ReportId, 'SaleTotal',@SaleTotal-@SaleReturn,@SortOrder);

	set @SortOrder=@SortOrder+1;
		--- ### Get Total Rounded Amount	

		SELECT @VatSum=isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0)
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1  AND OrderDetail.ItemType<>1 AND  OrderDetail.Active=1 AND OrderDetail.Direction=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND  OrderMaster.TerminalId = @TerminalId; 
		
			SELECT  @TotalPayment= Sum(Payment.PaidAmount)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = 1 AND OrderMaster.Status in (13,17) AND  OrderMaster.TerminalId = @TerminalId;
				---### Return Payment
			Declare @TotalReturnPayment as decimal(12,2);
			SELECT  @TotalReturnPayment= Sum(Payment.PaidAmount)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate 
					AND Payment.Direction = -1  AND  OrderMaster.TerminalId = @TerminalId And OrderMaster.Status in (13,17);
			
			SELECT  @CashOutTipMobile = isnull(Sum(Payment.TipAmount),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' AND    
					OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType=9 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  
					OrderMaster.TerminalId = @TerminalId;

			SELECT  @RoundingAmount= ISNULL(Sum((-1)*RoundedAmount),0)
			FROM OrderMaster 			
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND (OrderMaster.Status=13 or OrderMaster.Status = 17)  AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  OrderMaster.TerminalId = @TerminalId;
					
					declare @res as decimal(12,2);
					set @res=@TotalPayment+(-1)*@NetTotal+(-1)*@VatSum+@RoundingAmount-@TotalReturnPayment-(-1)*@SaleReturn - @CashOutTipMobile;
					if (@res<>0)
					begin
					set @RoundingAmount=@RoundingAmount-@res;
					end


	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	VALUES(NEWID(), @ReportId, 'Rounding',ISNULL(@RoundingAmount,0),@SortOrder);

	set @SortOrder=@SortOrder+1;
		-- ### Get acumulated total vat without taking consideration of return orders

		

	INSERT INTO ReportData (Id, ReportId, DataType, Value, TaxPercent, SortOrder)				
	VALUES( NEWID(), @ReportId, 'VATSum',@VatSum,0,@SortOrder);
	
	set @SortOrder=@SortOrder+1;
				--## Group by Tax Percentage
 
	INSERT INTO ReportData (Id, ReportId, DataType, Value, TaxPercent,TextValue, SortOrder)
		SELECT NEWID(), @ReportId, 'VATPercent', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,OrderDetail.TaxPercent,CAST( OrderDetail.TaxPercent as varchar(10))+'%', @SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND OrderDetail.ItemType<>1 AND  OrderDetail.Active=1 AND OrderDetail.Direction=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY (OrderDetail.TaxPercent);

	set @SortOrder=@SortOrder+1;

	-- ### Get acumulated total return vat without taking consideration of return orders
	INSERT INTO ReportData (Id, ReportId, DataType, Value, TaxPercent, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnVATSum', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,0, @SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1  AND OrderDetail.ItemType<>1 AND  OrderDetail.Active=1 AND OrderDetail.Direction=-1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND  OrderMaster.TerminalId = @TerminalId
	
	set @SortOrder=@SortOrder+1;			
				--## Group by Tax Percentage
 
	INSERT INTO ReportData (Id, ReportId, DataType, Value, TaxPercent,TextValue, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnVATPercent', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,OrderDetail.TaxPercent,CAST( OrderDetail.TaxPercent as varchar(10))+'%', @SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND OrderDetail.ItemType<>1  AND  OrderDetail.Active=1 AND OrderDetail.Direction=-1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY (OrderDetail.TaxPercent);

	set @SortOrder=@SortOrder+1;

	SELECT @DepositInCard = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 1 
	AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate 
	AND DepositHistory.TerminalId = @TerminalId;

	SELECT @DepositInCardTotal = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 1;

	SELECT @DepositInCash = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 2 
	AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate 
	AND DepositHistory.TerminalId = @TerminalId;

	SELECT @DepositInCashTotal = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 2;

	SELECT @DepositInReturnOrder = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 3 
	AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate 
	AND DepositHistory.TerminalId = @TerminalId;

	SELECT @DepositInReturnOrderTotal = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 3;
	
	SELECT @DepositOut = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 0 
	AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate 
	AND DepositHistory.TerminalId = @TerminalId;

	SELECT @DepositOutTotal = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 0;
	
	SET @DepositSum = @DepositInReturnOrder + @DepositInCard + @DepositInCash - @DepositOut;
	SET @SubTotalAllCustomer = @DepositInReturnOrderTotal + @DepositInCardTotal + @DepositInCashTotal - @DepositOutTotal;

--#Get Training mode sale
	

	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)		
		SELECT NEWID(), @ReportId, 'TrainingModeSale', Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount), @SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE   OrderMaster.TrainingMode=1 AND (OrderMaster.Status=13 or OrderMaster.Status = 17) AND OrderMaster.InvoiceGenerated=1 AND OrderDetail.ItemType<>1 AND  OrderDetail.Active=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId;

	set @SortOrder=@SortOrder+1;
	-- Get open cash
	SELECT TOP 1 @OpenCash = Isnull(CashDrawerLog.Amount,0) FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityType = 1 AND TerminalId = @TerminalId ORDER BY ActivityDate DESC

	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder) VALUES (NEWID(), @ReportId, 'CashDrawerOpen', @OpenCash, @SortOrder);

	set @SortOrder=@SortOrder+1;
	-- ###  cash added in drawer 
	SELECT @CashAdded= isnull(Sum(Amount),0)
			FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityType = 5 AND ActivityDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId

	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		values( NEWID(), @ReportId, 'CashAdded',@CashAdded, @SortOrder);

	set @SortOrder=@SortOrder+1;
			
	-- ###  cash dropped in drawer 
	SELECT @CashDropped= Isnull(Sum(Amount),0)
			FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityType = 4 AND ActivityDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId
		
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		values( NEWID(), @ReportId, 'CashDropped', @CashDropped, @SortOrder);

	set @SortOrder=@SortOrder+1;

			-- Get TotalSale Cash In
	SELECT @CashIn=isnull(Sum(Payment.PaidAmount),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.PaymentType=1 AND Payment.Direction = 1 AND OrderMaster.TerminalId = @TerminalId;

					SELECT @CashInTip=isnull(Sum(Payment.TipAmount),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.PaymentType=1 AND Payment.Direction = 1 AND OrderMaster.TerminalId = @TerminalId;

			SET @CashIn = @CashIn + @CashInTip + @DepositInCash; 

			INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)VALUES(NEWID(), @ReportId, 'CashIn',@CashIn,@SortOrder);

			set @SortOrder=@SortOrder+1;
			
			-- Get TotalSale Cash Out
		SELECT  @CashOut= isnull(Sum(Payment.CashChange),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' AND    
OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType in (7,9,4) AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  OrderMaster.TerminalId = @TerminalId;

		SELECT @CashOutPant = isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount),0)
						FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			LEFT JOIN Product ON OrderDetail.ItemId = Product.id
			inner join ItemCategory on OrderDetail.ItemId=ItemCategory.ItemId AND ItemCategory.IsPrimary=1
			inner join Category on ItemCategory.CategoryId=Category.Id
				WHERE   OrderMaster.TrainingMode=0 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND 
				(OrderMaster.Status = 13 or OrderMaster.Status = 17)   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
				AND Product.PLU = 'PANT';

				SELECT @CashOutLotter = isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount),0)
						FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			LEFT JOIN Product ON OrderDetail.ItemId = Product.id
			inner join ItemCategory on OrderDetail.ItemId=ItemCategory.ItemId AND ItemCategory.IsPrimary=1
			inner join Category on ItemCategory.CategoryId=Category.Id
				WHERE   OrderMaster.TrainingMode=0 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND 
				(OrderMaster.Status = 13  or OrderMaster.Status = 17)  AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
				AND Product.PLU = 'LOTTER';

				

		SELECT  @CashOutReturnAmountMobile = isnull(Sum(Payment.ReturnAmount),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' AND    
OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType=9 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  
OrderMaster.TerminalId = @TerminalId;

	

SELECT  @CashOutTipAll = isnull(Sum(Payment.TipAmount),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' AND    
OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType!=9 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  
OrderMaster.TerminalId = @TerminalId;



set @CashOut= @CashOut;

	INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)VALUES( NEWID(), @ReportId, 'CashOut', @CashOut, @SortOrder);

	set @SortOrder=@SortOrder+1;
	
	-- Get TotalSale Cash Sum	
	set @CashSum=@OpenCash+@CashAdded+@CashIn-@CashDropped-@CashOut - @CashOutTipAll- @CashOutTipMobile;
	
	INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'TipTypeSale', Sum(Payment.TipAmount), Payment.PaymentType, @SortOrder
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = 1 AND OrderMaster.Status in (13,17) AND  OrderMaster.TerminalId = @TerminalId
						GROUP BY Payment.PaymentType;

	set @SortOrder=@SortOrder+1;

	INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'CashSum',@CashSum, @SortOrder);

	set @SortOrder=@SortOrder+1;
  
  if(@CashOutPant != 0)
  BEGIN
	INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'PANT',@CashOutPant, @SortOrder);
	set @SortOrder=@SortOrder+1;
  END

	if(@CashOutLotter != 0)
	BEGIN
		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'LOTTER',@CashOutLotter, @SortOrder);
		set @SortOrder=@SortOrder+1;
	END
	
	if(not (@DepositInCash = 0 AND @DepositInCard = 0 AND @DepositOut = 0 AND @DepositSum = 0 AND @DepositInReturnOrder = 0))
	BEGIN
		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit In (Return Orders)',@DepositInReturnOrder, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit In (Cash)',@DepositInCash, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit In (Card)',@DepositInCard, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit Out',@DepositOut, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit Sum',@DepositSum, @SortOrder);
		set @SortOrder=@SortOrder+1;

		INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder) 
		values( NEWID(), @ReportId, 'Loaded Deposit',0, @SortOrder);

		set @SortOrder=@SortOrder+1;

		INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, TextValue, SortOrder) 
		SELECT NEWID(), @ReportId, Customer.CustomerNo, DepositHistory.DepositAmount, DepositHistory.DepositType, 'DepositType', @SortOrder
		FROM DepositHistory
		INNER JOIN Customer on Customer.Id = DepositHistory.CustomerId 
		where DepositHistory.DepositType in (1,2,3) 
		AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate  
		AND DepositHistory.TerminalId = @TerminalId
		order by DepositHistory.CreatedOn;

		set @SortOrder=@SortOrder+1;

		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Total Deposit',@DepositInCash + @DepositInCard + @DepositInReturnOrder, @SortOrder);
		set @SortOrder=@SortOrder+1;

		INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder) 
		values( NEWID(), @ReportId, 'LoadedDepositEnd',null, @SortOrder);
		set @SortOrder=@SortOrder+1;

		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Subtotal deponering',0, @SortOrder);
		set @SortOrder=@SortOrder+1;

		INSERT INTO ReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'alla kunder',@SubTotalAllCustomer, @SortOrder);
		set @SortOrder=@SortOrder+1;

		INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder) 
		values( NEWID(), @ReportId, 'LoadedDepositEnd',null, @SortOrder);

		set @SortOrder=@SortOrder+1;
	END

		-- Get TotalSale on PaymentType
	INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeSale', Sum(Payment.PaidAmount), Payment.PaymentType, @SortOrder
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE Payment.PaymentType != 9 AND OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 
					AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = 1 AND OrderMaster.Status in (13,17) 
					AND  OrderMaster.TerminalId = @TerminalId
						GROUP BY Payment.PaymentType;

	set @SortOrder=@SortOrder+1;

	INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeSale', Sum(Payment.PaidAmount) + @CashOutReturnAmountMobile, Payment.PaymentType, @SortOrder
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE Payment.PaymentType = 9 AND OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 
					AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = 1 AND 
					OrderMaster.Status in (13,17) AND  OrderMaster.TerminalId = @TerminalId
						GROUP BY Payment.PaymentType;

	/*INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
	SELECT NEWID(), @ReportId, 'PaymentTypeSale', Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount), 
	Payment.PaymentType, @SortOrder
	FROM OrderMaster 
	JOIN Payment ON Payment.OrderId = OrderMaster.Id
	LEFT JOIN OrderDetail ON OrderMaster.Id = OrderDetail.OrderId 
			LEFT JOIN Product ON OrderDetail.ItemId = Product.id
				LEFT JOIN ItemCategory ON Product.Id = ItemCategory.ItemId AND ItemCategory.IsPrimary=1
			LEFT JOIN Category ON ItemCategory.CategoryId = Category.Id
				WHERE   OrderMaster.TrainingMode=0 
				AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate 
				AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status = 13   
				AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  
				AND  OrderMaster.TerminalId = @TerminalId
				AND Payment.PaymentType != 9
				GROUP BY Payment.PaymentType;

	set @SortOrder=@SortOrder+1;

	INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
	SELECT NEWID(), @ReportId, 'PaymentTypeSale', Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount) + @CashOutReturnAmountMobile , 
	Payment.PaymentType, @SortOrder
	FROM OrderMaster 
	JOIN Payment ON Payment.OrderId = OrderMaster.Id
	LEFT JOIN OrderDetail ON OrderMaster.Id = OrderDetail.OrderId 
			LEFT JOIN Product ON OrderDetail.ItemId = Product.id
				LEFT JOIN ItemCategory ON Product.Id = ItemCategory.ItemId AND ItemCategory.IsPrimary=1
			LEFT JOIN Category ON ItemCategory.CategoryId = Category.Id
				WHERE   OrderMaster.TrainingMode=0 AND  
				OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate 
				AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status = 13   
				AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  
				AND  OrderMaster.TerminalId = @TerminalId
				AND Payment.PaymentType = 9
				GROUP BY Payment.PaymentType;*/

	set @SortOrder=@SortOrder+1;
	
	-- Get TotalReturn on PaymentType 
	
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	values( NEWID(), @ReportId, 'PaymentReturnCount',0, @SortOrder);

	set @SortOrder=@SortOrder+1;
	
	INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeReturn', Sum(Payment.PaidAmount), Payment.PaymentType, @SortOrder
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated = 1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.Status in (15,18) AND OrderMaster.TerminalId = @TerminalId
						GROUP BY Payment.PaymentType;

	set @SortOrder=@SortOrder+1;
						
		-- ### Get number of standard orders
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'OrderCount', COUNT(*), @SortOrder
		FROM OrderMaster 				
				WHERE OrderMaster.TrainingMode=0 AND   (OrderMaster.Status=13 or OrderMaster.Status = 17) AND  OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND OrderMaster.Id IN (SELECT OrderId FROM OrderDetail WHERE Direction = 1 AND OrderDetail.Active=1 ) AND  OrderMaster.TerminalId = @TerminalId;

	set @SortOrder=@SortOrder+1;

	-- ### Get total sale prodcut count of given period
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'ProductsCount',Sum(OrderDetail.Qty), @SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 				
				WHERE OrderMaster.TrainingMode=0 AND  (OrderMaster.Status=13 or OrderMaster.Status = 17) AND  OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND OrderDetail.ItemType<>1  AND OrderDetail.Direction = 1  AND OrderDetail.Active=1 AND  OrderMaster.TerminalId = @TerminalId;

	set @SortOrder=@SortOrder+1;

	-- ### Get Category Sale	
	INSERT INTO ReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	VALUES( NEWID(), @ReportId, 'CategorySaleHeading',0,'',0,@SortOrder);

	set @SortOrder=@SortOrder+1;

	INSERT INTO ReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	SELECT NEWID(), @ReportId, 'CategorySale', Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount), Category.Name, 0, @SortOrder
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			LEFT JOIN Product ON OrderDetail.ItemId = Product.id
				LEFT JOIN ItemCategory ON Product.Id = ItemCategory.ItemId AND ItemCategory.IsPrimary=1
			LEFT JOIN Category ON ItemCategory.CategoryId = Category.Id
				WHERE   OrderMaster.TrainingMode=0 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate 
				AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status in (13,15,17)   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
				GROUP BY Category.Id,Category.Name
				Order BY Category.Name;

	set @SortOrder=@SortOrder+1;
	
	INSERT INTO ReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	VALUES( NEWID(), @ReportId, 'CategorySaleCountHeading',0,'',0,@SortOrder);

	set @SortOrder=@SortOrder+1;

	INSERT INTO ReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	SELECT NEWID(), @ReportId, 'CategorySaleCount', Sum(OrderDetail.Qty), Category.Name as Name,Category.Id as CategoryId, @SortOrder
	FROM OrderMaster LEFT JOIN OrderDetail 
		ON OrderMaster.Id = OrderDetail.OrderId 
		LEFT JOIN Product ON OrderDetail.ItemId = Product.id
				LEFT JOIN ItemCategory ON Product.Id = ItemCategory.ItemId AND ItemCategory.IsPrimary=1
			LEFT JOIN Category ON ItemCategory.CategoryId = Category.Id
			WHERE Category.Name is not null AND OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 
			AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderDetail.Direction = 1 
			AND (OrderMaster.Status = 13  or OrderMaster.Status = 17) AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND OrderMaster.TerminalId = @TerminalId
					GROUP BY  Category.Id,Category.Name
					Order BY Category.Name;

	set @SortOrder=@SortOrder+1;

--### Accounting
DECLARE @LoopCounter INT = 1, @MaxAccountingId INT  , 
        @AccountingId int,@accountingTotal decimal(12,2),@Title varchar(100)
 select @MaxAccountingId=MAX(Id) from Accounting
 INSERT INTO ReportData (Id, ReportId, DataType,TextValue, SortOrder)
 Values(NEWID(), @ReportId, 'Accounting',' ',@SortOrder);
 	
set @SortOrder=@SortOrder+1;

WHILE(@LoopCounter <= @MaxAccountingId)
BEGIN
   SELECT @AccountingId = Id
   FROM Accounting WHERE Id = @LoopCounter   
   set @SortOrder=@SortOrder+1;
 select @Title=(cast(Accounting.AcNo as varchar(10))+'-'+ Accounting.Name) from Accounting where Id=@AccountingId
  SELECT @accountingTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount)
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID
			
				WHERE   OrderMaster.TrainingMode=0 AND Product.AccountingId=@AccountingId AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
		
		if(@accountingTotal is not null)
		begin
		--##empty line	
	 INSERT INTO ReportData (Id, ReportId, DataType,TextValue,  SortOrder)
 Values(NEWID(), @ReportId, ' ',' ',@SortOrder);		
  set @SortOrder=@SortOrder+1;
	INSERT INTO ReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACTotal',@Title,@AccountingId,@accountingTotal,@SortOrder)	
	
	end

				SELECT @accountingTotal=Sum((((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount)/(1+OrderDetail.TaxPercent/100))) 
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID
			
				WHERE   OrderMaster.TrainingMode=0 AND Product.AccountingId=@AccountingId AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
			if(@accountingTotal is not null)	
			Begin	
				   set @SortOrder=@SortOrder+1;
INSERT INTO ReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACNetTotal',@Title,@AccountingId,@accountingTotal,@SortOrder)	
	end
	--##Acoounitng Standared Total
			  SELECT @accountingTotal= Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount))
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID
			
				WHERE   OrderMaster.TrainingMode=0 AND Product.AccountingId=@AccountingId AND OrderMaster.Type=0 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
					if(@accountingTotal is not null)
					begin
				   set @SortOrder=@SortOrder+1;
				INSERT INTO ReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACStandared',@Title,@AccountingId,@accountingTotal,@SortOrder)
	end
	--##Acoounitng Takeaway Total
				  SELECT @accountingTotal= Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount))
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID
			
				WHERE   OrderMaster.TrainingMode=0 AND Product.AccountingId=@AccountingId AND OrderMaster.Type=3 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14    AND OrderDetail.ItemType<>1 AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
					if(@accountingTotal is not null)
					begin
				   set @SortOrder=@SortOrder+1;
				INSERT INTO ReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACTakeaway',@Title,@AccountingId,@accountingTotal,@SortOrder)
	end
	
				--##Acoounitng Takeaway Total
				  SELECT @accountingTotal= Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount))
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID			
				WHERE   OrderMaster.TrainingMode=0 AND Product.AccountingId=@AccountingId AND OrderMaster.Type=6 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14    AND OrderDetail.ItemType<>1 AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
					if(@accountingTotal is not null)
					begin
				   set @SortOrder=@SortOrder+1;
				INSERT INTO ReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACTable',@Title,@AccountingId,@accountingTotal,@SortOrder)
	end

	SELECT  @accountingTotal= isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0)
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID	
				WHERE OrderMaster.TrainingMode=0 AND Product.AccountingId=@AccountingId AND   OrderMaster.InvoiceGenerated=1  AND OrderDetail.ItemType<>1 AND  OrderDetail.Active=1 AND OrderDetail.Direction=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND  OrderMaster.TerminalId = @TerminalId
		 if(@accountingTotal>0)
					begin
					--##empty line	
	 INSERT INTO ReportData (Id, ReportId, DataType,TextValue,  SortOrder)
 Values(NEWID(), @ReportId, ' ',' ',@SortOrder);		
  set @SortOrder=@SortOrder+1;
		  
				INSERT INTO ReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACVatSum',@Title,@AccountingId,@accountingTotal,@SortOrder)
	

	  set @SortOrder=@SortOrder+1;
	  INSERT INTO ReportData (Id, ReportId, DataType, Value, TaxPercent,TextValue, SortOrder)
	SELECT NEWID(), @ReportId, 'ACVATPercent', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,OrderDetail.TaxPercent,CAST( OrderDetail.TaxPercent as varchar(10))+'%', @SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID	
				WHERE OrderMaster.TrainingMode=0 AND Product.AccountingId=@AccountingId AND   OrderMaster.InvoiceGenerated=1 AND OrderDetail.ItemType<>1  AND  OrderDetail.Active=1 AND OrderDetail.Direction=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY (OrderDetail.TaxPercent);
					end
   SET @LoopCounter  = @LoopCounter  + 1        
END


-- ### Open cash drawer count

	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'CashDrawerOpenCount', Count(*), 73
			FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId
 
 	
-- ### Receipt count

	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'ReceiptCount', Count(*), 74
			FROM Receipt WHERE printDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId
			
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		VALUES (NEWID(), @ReportId, 'ReceiptCopyCount', 0, 75)
	--INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		--VALUES (NEWID(), @ReportId, 'ReceiptCopyAmount', 0, 56)


	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		VALUES (NEWID(), @ReportId, 'ServicesCount', 0, 76)


		
		
	

	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	SELECT NEWID(), @ReportId, 'Discount', Sum(OrderDetail.ItemDiscount), 81
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND (OrderMaster.Status =13 or OrderMaster.Status = 17) AND OrderDetail.ItemType<>1 AND OrderDetail.Active=1 AND OrderDetail.Direction=1 AND  OrderMaster.TerminalId = @TerminalId;

INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	SELECT NEWID(), @ReportId, 'DiscountCount', COUNT(*), 82
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND (OrderMaster.Status =13 or OrderMaster.Status = 17) AND OrderDetail.ItemType<>1 AND OrderDetail.Active=1 AND OrderDetail.Direction=1 AND OrderDetail.ItemDiscount>0 AND  OrderMaster.TerminalId = @TerminalId;


	-- Get number of return orders
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnCount', COUNT(*), 83
		FROM OrderMaster 
				WHERE 
					 OrderMaster.InvoiceGenerated=1 AND OrderMaster.Type=5 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId;

	

-- ### Get number of training sale orders
					INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)	
		SELECT NEWID(), @ReportId, 'TrainingModeCount', count(Id), 84
		FROM OrderMaster 
				WHERE    OrderMaster.InvoiceGenerated=1 AND OrderMaster.TrainingMode=1  AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND OrderMaster.TerminalId = @TerminalId;

	-- Get number of return orders
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'HoldCount', COUNT(*), 85
		FROM OrderMaster 
				WHERE OrderMaster.TrainingMode=0 AND OrderMaster.CreationDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.status = 3 AND  OrderMaster.TerminalId = @TerminalId;

	-- ### Get total sale of unfinished orders on given period
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'HoldTotalSale', IsNull(Sum(OrderTotal),0), 86
		FROM OrderMaster 
			WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.status = 3 AND OrderMaster.CreationDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.status = 3 AND  OrderMaster.TerminalId = @TerminalId;


	

	-- ### Get acumulated total sale without taking consideration of return orders
		SELECT @SaleTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount)
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND (OrderMaster.Status=13 or OrderMaster.Status = 17) AND OrderMaster.InvoiceGenerated=1   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId;


SELECT @SaleReturn= Isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount),0)
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND (OrderMaster.Status=15 or OrderMaster.Status = 18) AND OrderMaster.InvoiceGenerated=1 AND OrderDetail.Direction=-1 AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId;



	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	values(NEWID(), @ReportId, 'GrandTotalSale',@SaleTotal,102)
	-- ### Get acumulated total return
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
		values(NEWID(), @ReportId, 'GrandTotalReturn',@SaleReturn,103)
	-- ### Get acumulated total return
	INSERT INTO ReportData (Id, ReportId, DataType, Value, SortOrder)
	values( NEWID(), @ReportId, 'GrandTotalNet',@SaleTotal-@SaleReturn,104)
		


	UPDATE TerminalStatusLog SET ReportId = @ReportId WHERE TerminalId = @TerminalId AND ActivityDate = @CloseDate AND Status = 0;

	RETURN
END









