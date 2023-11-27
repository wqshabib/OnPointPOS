
/****** Object:  StoredProcedure [dbo].[SP_PrintDetailReportByDateRange]    Script Date: 2021-07-03 16:08:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[SP_PrintDetailReportByDateRange] 
    @TerminalId UNIQUEIDENTIFIER,	
	@OpenDate AS DATETIME,
	@CloseDate AS DATETIME
AS
BEGIN
	Declare @SortOrder AS INT;
	Declare @DepositInCash AS DECIMAL(18,2);
	Declare @DepositInReturnOrder AS DECIMAL(18,2);
	Declare @DepositInCard AS DECIMAL(18,2);
	Declare @DepositOut AS DECIMAL(18,2);
	Declare @DepositSum AS DECIMAL(18,2);

	Declare @DepositInCashTotal AS DECIMAL(18,2);
	Declare @DepositInReturnOrderTotal AS DECIMAL(18,2);
	Declare @DepositInCardTotal AS DECIMAL(18,2);
	Declare @DepositOutTotal AS DECIMAL(18,2);

	Declare @SubTotalAllCustomer AS DECIMAL(18,2);

	Declare @CashInTip AS DECIMAL(18,2);
	Declare @CashOutTipMobile AS DECIMAL(18,2);
	Declare @CashOutTipAll AS DECIMAL(18,2);

	 Declare @ReportId AS uniqueidentifier;
	Declare @ReportType AS int;		
	Declare @OutletId AS INT;
	Declare @UniqueIdentification AS nvarchar(255);	
	Declare @OpenCash AS DECIMAL(12,2);
	Declare @CashIn AS DECIMAL(12,2);
	Declare @CashOut AS DECIMAL(12,2);
	declare @CashOutPant as decimal(18,2);
	declare @CashOutLotter as decimal(18,2);
	Declare @CashAdded AS DECIMAL(12,2);
	Declare @CashDropped AS DECIMAL(12,2);
	Declare @CashSum AS DECIMAL(12,2);
	Declare @SaleTotal AS DECIMAL(12,2);
	Declare @SaleReturn AS DECIMAL(12,2);
	Declare @NetTotal AS DECIMAL(12,2);
	Declare @VatSum AS DECIMAL(12,2);
	Declare @RoundingAmount AS DECIMAL(12,2);
	Declare @TotalPayment AS DECIMAL(12,2);
	Declare @ReportNumber as int;
   
	set @ReportType =1;
	SET @ReportId = NEWID();
		
	
	CREATE TABLE #TempReportData(
 [Id] [uniqueidentifier] NOT NULL,
	[ReportId] [uniqueidentifier] NOT NULL,
	[DataType] [nvarchar](50) NOT NULL,
	[TextValue] [nvarchar](255) NULL,
	[ForeignId] [int] NULL,
	[Value] [decimal](12, 2) NULL,
	[TaxPercent] [decimal](12, 2) NULL,
	[DateValue] [datetime] NULL,
	[SortOrder] [int] NULL

)
	
	SELECT @ReportNumber = IsNull(Max(ReportNumber),0)+1 FROM Report WHERE Report.ReportType = @ReportType;
	
	set @SortOrder=1;
			-- Get unique identifier for terminal
	SELECT top 1 @UniqueIdentification = UniqueIdentification FROM Terminal Where Id=@TerminalId;
	-- Set reportnumber on report
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder) VALUES (NEWID(), @ReportId, 'ReportNumber', @ReportNumber, @SortOrder);
	set @SortOrder=@SortOrder+1;
	INSERT INTO #TempReportData (Id, ReportId, DataType, TextValue, SortOrder) VALUES (NEWID(), @ReportId, 'UniqueIdentification', @UniqueIdentification, @SortOrder);
	set @SortOrder=@SortOrder+1;
	INSERT INTO #TempReportData (Id, ReportId, DataType, DateValue, SortOrder) VALUES (NEWID(),@ReportId,'OpenDate', @OpenDate, @SortOrder);
	set @SortOrder=@SortOrder+1;
	INSERT INTO #TempReportData (Id, ReportId, DataType, DateValue, SortOrder) VALUES (NEWID(),@ReportId,'CloseDate', @CloseDate, @SortOrder);
	set @SortOrder=@SortOrder+1;
    

	
	-- ### Get total sale of given period

	CREATE TABLE #TempData(
 
	[UnitPrice] [decimal](12, 2) NULL,
	[TaxPercent] [decimal](12, 2) NULL,
	[Qty] [decimal](12, 3) NULL,
	[Direction] [decimal](12, 2) NULL,
	[ItemDiscount] [decimal](12, 2) NULL,	
	[Type] [int] NULL,
	[Status] [int] NULL
)
INSERT INTO #TempData(UnitPrice,Qty,Direction,ItemDiscount,TaxPercent,[Type],[Status])
SELECT                     OrderDetail.UnitPrice,OrderDetail.Qty,OrderDetail.Direction,OrderDetail.ItemDiscount,OrderDetail.TaxPercent,OrderMaster.Type,OrderMaster.Status
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			
				WHERE   OrderMaster.TrainingMode=0  AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1    AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
		
	SELECT @SaleTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)
		FROM #TempData OrderDetail 
			
				WHERE  (Status =13 or status = 17) AND OrderDetail.Direction=1 ;


SELECT @SaleReturn= isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount),0)
		FROM #TempData OrderDetail 
			
				WHERE  (Status =15 or Status = 18) AND OrderDetail.Direction=-1;



	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	Values(NEWID(), @ReportId, 'TotalSale',@SaleTotal,@SortOrder);
	
	set @SortOrder=@SortOrder+1;
		
	-- ### Get total net of given period

	SELECT @NetTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))
   FROM #TempData OrderDetail
   WHERE  (Status =13 or Status = 17) AND OrderDetail.Direction=1


	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	VALUES(NEWID(), @ReportId, 'TotalNet',@NetTotal,@SortOrder);
				
	set @SortOrder=@SortOrder+1;
	
	-- ### Get total return of given period
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	values(NEWID(), @ReportId, 'TotalReturn',@SaleReturn,@SortOrder);

	set @SortOrder=@SortOrder+1;
		
	-- ### Get total return of given period
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	Values(NEWID(), @ReportId, 'SaleTotal',@SaleTotal-@SaleReturn,@SortOrder);

	set @SortOrder=@SortOrder+1;
	
	--- ### Get Total Rounded Amount	

	SELECT @VatSum=isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0)
	FROM #TempData OrderDetail
	WHERE   OrderDetail.Direction=1;

	CREATE TABLE #TempPaymentData(
 
	[PaidAmount] [decimal](12, 2) NULL,	
	[Direction] [decimal](12, 2) NULL,
	[CashChange] [decimal](12, 2) NULL,	
	[PaymentType] [int] NULL
)

	CREATE TABLE #TempPaymentDataFromOrderDetail(
 
	[UnitPrice] [decimal](12, 2) NULL,
	[TaxPercent] [decimal](12, 2) NULL,
	[Qty] [decimal](12, 3) NULL,
	[Direction] [decimal](12, 2) NULL,
	[ItemDiscount] [decimal](12, 2) NULL,	
	[Type] [int] NULL,
	[Status] [int] NULL,
	[Name] varchar(200) null,
	Id [int] null,
	[PaymentType] [int] NULL
)

	INSERT INTO #TempPaymentData(PaidAmount,PaymentType,Direction,CashChange)		
	SELECT  Payment.PaidAmount,Payment.PaymentType,Payment.Direction,Payment.CashChange
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 
					AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND 
					OrderMaster.TerminalId = @TerminalId and (OrderMaster.Status = 13 or OrderMaster.Status = 17);

	INSERT INTO #TempPaymentData(PaidAmount,PaymentType,Direction,CashChange)		
	SELECT  Payment.ReturnAmount,Payment.PaymentType,Payment.Direction,0
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND Payment.ReturnAmount > 0 AND Payment.PaymentType != 1 AND   OrderMaster.InvoiceGenerated=1 
					AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND 
					OrderMaster.TerminalId = @TerminalId and (OrderMaster.Status = 13 or OrderMaster.Status = 17);

	INSERT INTO #TempPaymentData(PaidAmount,PaymentType,Direction,CashChange)		
	SELECT  Payment.ReturnAmount*-1,1,Payment.Direction,0
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND Payment.ReturnAmount > 0 AND Payment.PaymentType != 1 AND   OrderMaster.InvoiceGenerated=1 
					AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND 
					OrderMaster.TerminalId = @TerminalId and (OrderMaster.Status = 13 or OrderMaster.Status = 17);




	INSERT INTO #TempPaymentDataFromOrderDetail([UnitPrice],[Qty],[Direction],[ItemDiscount],[PaymentType])
	SELECT OrderDetail.UnitPrice,OrderDetail.Qty,OrderDetail.Direction,OrderDetail.ItemDiscount, Payment.PaymentType
	FROM OrderMaster JOIN Payment ON Payment.OrderId = OrderMaster.Id
			LEFT JOIN OrderDetail ON OrderMaster.Id = OrderDetail.OrderId 
			inner join ItemCategory on OrderDetail.ItemId=ItemCategory.ItemId AND ItemCategory.IsPrimary=1
			inner join Category on ItemCategory.CategoryId=Category.Id
				WHERE   OrderMaster.TrainingMode=0 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND 
				(OrderMaster.Status = 13 or OrderMaster.Status = 17)   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId;


	---## Total Payment
	SELECT  @TotalPayment= Sum(Payment.PaidAmount)
	FROM #TempPaymentData Payment 
	WHERE Payment.Direction = 1;
	
	---### Return Payment
	Declare @TotalReturnPayment as decimal(12,2);
	SELECT  @TotalReturnPayment= Sum(Payment.PaidAmount)
	FROM #TempPaymentData Payment 
	WHERE Payment.Direction = -1;

	
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


	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	VALUES(NEWID(), @ReportId, 'Rounding',isnull(@RoundingAmount,0),@SortOrder);

	set @SortOrder=@SortOrder+1;
	
	-- ### Get acumulated total vat without taking consideration of return orders

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent, SortOrder)				
	VALUES( NEWID(), @ReportId, 'VATSum',@VatSum,0,@SortOrder);

	set @SortOrder=@SortOrder+1;
	
	--## Group by Tax Percentage
 
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent,TextValue, SortOrder)
		SELECT NEWID(), @ReportId, 'VATPercent', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,OrderDetail.TaxPercent,CAST( OrderDetail.TaxPercent as varchar(10))+'%', @SortOrder
		FROM #TempData OrderDetail
		WHERE   OrderDetail.Direction=1
		GROUP BY (OrderDetail.TaxPercent);

	set @SortOrder=@SortOrder+1;

	-- ### Get acumulated total return vat without taking consideration of return orders
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnVATSum', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,0, @SortOrder
		FROM #TempData OrderDetail
		WHERE  (Status =15 or Status = 18) AND OrderDetail.Direction=-1;

	set @SortOrder=@SortOrder+1;
	
	--## Group by Tax Percentage
 
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent,TextValue, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnVATPercent', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,OrderDetail.TaxPercent,CAST( OrderDetail.TaxPercent as varchar(10))+'%', @SortOrder
		FROM #TempData OrderDetail 
		WHERE  (Status =15  or Status = 18) AND OrderDetail.Direction=-1
		GROUP BY (OrderDetail.TaxPercent);

	set @SortOrder=@SortOrder+1;

	SELECT @DepositInCard = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 1 
	AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate 
	AND DepositHistory.TerminalId = @TerminalId;

	SELECT @DepositInCardTotal = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 1
	AND DepositHistory.CreatedOn <= @CloseDate;

	SELECT @DepositInCash = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 2 
	AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate 
	AND DepositHistory.TerminalId = @TerminalId;

	SELECT @DepositInCashTotal = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 2 
	AND DepositHistory.CreatedOn <= @CloseDate;

	SELECT @DepositInReturnOrder = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 3 
	AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate 
	AND DepositHistory.TerminalId = @TerminalId;

	SELECT @DepositInReturnOrderTotal = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 3 
	AND DepositHistory.CreatedOn <= @CloseDate;

	SELECT @DepositOut = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 0 
	AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate 
	AND DepositHistory.TerminalId = @TerminalId;

	SELECT @DepositOutTotal = isnull(Sum(DepositHistory.DepositAmount),0) from DepositHistory
	where DepositHistory.DepositType = 0 
	AND DepositHistory.CreatedOn <= @CloseDate;
	
	SET @DepositSum = @DepositInReturnOrder + @DepositInCard + @DepositInCash - @DepositOut;
	SET @SubTotalAllCustomer = @DepositInReturnOrderTotal + @DepositInCardTotal + @DepositInCashTotal - @DepositOutTotal;

	--#Get Training mode sale
	
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)		
		VALUES( NEWID(), @ReportId, 'TrainingModeSale', 0, @SortOrder);

	set @SortOrder=@SortOrder+1;
	
	-- Get open cash
	SELECT  @OpenCash = sum(Isnull(CashDrawerLog.Amount,0)) FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id 
	WHERE ActivityType = 1 AND TerminalId = @TerminalId AND ActivityDate BETWEEN @OpenDate AND @CloseDate 

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder) VALUES (NEWID(), @ReportId, 'CashDrawerOpen', @OpenCash, @SortOrder);

	set @SortOrder=@SortOrder+1;

	-- ###  cash added in drawer 
	SELECT @CashAdded = isnull(Sum(Amount),0) FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityType = 5 AND ActivityDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder) values( NEWID(), @ReportId, 'CashAdded',@CashAdded, @SortOrder);

	set @SortOrder=@SortOrder+1;
	
	-- ###  cash dropped in drawer 
	SELECT @CashDropped= Isnull(Sum(Amount),0)
	FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityType = 4 AND ActivityDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId
		
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		values( NEWID(), @ReportId, 'CashDropped', @CashDropped, @SortOrder);
	
	set @SortOrder=@SortOrder+1;

	-- Get TotalSale Cash In
	SELECT @CashIn=isnull(Sum(Payment.PaidAmount),0)
			FROM #TempPaymentData Payment
			WHERE Payment.Direction = 1 AND Payment.PaymentType=1 AND Payment.PaidAmount > 0;

	SELECT @CashInTip=isnull(Sum(Payment.TipAmount),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.PaymentType=1 AND Payment.Direction = 1 AND OrderMaster.TerminalId = @TerminalId;

	SET @CashIn = @CashIn + @CashInTip + @DepositInCash; 
	
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)VALUES(NEWID(), @ReportId, 'CashIn',@CashIn,@SortOrder);

	set @SortOrder=@SortOrder+1;

	-- Get TotalSale Cash Out
							SELECT  @CashOut= isnull(Sum(Payment.CashChange),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' AND    
OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType in (4,7,9) AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  OrderMaster.TerminalId = @TerminalId;

-- we have make PaymentType 7 only due to we need to include cash only which include mobile as well
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
				(OrderMaster.Status = 13 or OrderMaster.Status = 17)   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
				AND Product.PLU = 'LOTTER';
	
	SELECT  @CashOutTipAll = isnull(Sum(Payment.TipAmount),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' AND    
OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType!=9 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  
OrderMaster.TerminalId = @TerminalId;


	INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)VALUES( NEWID(), @ReportId, 'CashOut', @CashOut, @SortOrder);

	set @SortOrder=@SortOrder+1;
	
	-- Get TotalSale Cash Sum	
	-- set @CashSum=@OpenCash+@CashAdded+@CashIn-@CashDropped-@CashOut;
	
	set @CashSum=@OpenCash+@CashAdded+@CashIn-@CashDropped-@CashOut - @CashOutTipAll- @CashOutTipMobile;
	
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'CashSum',@CashSum, @SortOrder);

	set @SortOrder=@SortOrder+1;
	
	if(@CashOutPant != 0)
	BEGIN
		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'PANT',@CashOutPant, @SortOrder);
		set @SortOrder=@SortOrder+1;
	END

	if(@CashOutLotter != 0)
	BEGIN
		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'LOTTER',@CashOutLotter, @SortOrder);
		set @SortOrder=@SortOrder+1;				
	END

	if(not (@DepositInCash = 0 AND @DepositInCard = 0 AND @DepositOut = 0 AND @DepositSum = 0))
	BEGIN
		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit In (Return Order)',@DepositInReturnOrder, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit In (Cash)',@DepositInCash, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit In (Card)',@DepositInCard, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit Out',@DepositOut, @SortOrder);
		set @SortOrder=@SortOrder+1;
		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Deposit Sum',@DepositSum, @SortOrder);
		set @SortOrder=@SortOrder+1;

		INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder) 
		values( NEWID(), @ReportId, 'Loaded Deposit',0, @SortOrder);

		set @SortOrder=@SortOrder+1;

		INSERT INTO #TempReportData (Id, ReportId, DataType, Value, ForeignId, TextValue, SortOrder) 
		SELECT NEWID(), @ReportId, Customer.CustomerNo, DepositHistory.DepositAmount, DepositHistory.DepositType, 'DepositType', @SortOrder
		FROM DepositHistory
		INNER JOIN Customer on Customer.Id = DepositHistory.CustomerId 
		where DepositHistory.DepositType in (1,2, 3) 
		AND DepositHistory.CreatedOn BETWEEN @OpenDate AND @CloseDate  
		AND DepositHistory.TerminalId = @TerminalId
		order by DepositHistory.CreatedOn;

		set @SortOrder=@SortOrder+1;

		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Total Deposit',@DepositInCash + @DepositInCard + @DepositInReturnOrder, @SortOrder);
		set @SortOrder=@SortOrder+1;

		-- INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Subtotal deponering',0, @SortOrder);
		-- set @SortOrder=@SortOrder+1;
		
		INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'Subtotal deponering alla kunder',@SubTotalAllCustomer, @SortOrder);
		set @SortOrder=@SortOrder+1;

	END
  
		-- Get TotalSale on PaymentType
	/*INSERT INTO #TempReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeSale', Sum((Payment.UnitPrice*Payment.Qty*Payment.Direction)-Payment.ItemDiscount), 
			Payment.PaymentType, @SortOrder
			FROM #TempPaymentDataFromOrderDetail Payment 
			WHERE Payment.Direction = 1
			GROUP BY Payment.PaymentType;*/

	set @SortOrder=@SortOrder+1;

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeSale', Sum(Payment.PaidAmount), Payment.PaymentType, @SortOrder
			FROM #TempPaymentData Payment 
			WHERE Payment.Direction = 1
			GROUP BY Payment.PaymentType;

	set @SortOrder=@SortOrder+1;

	-- Get TotalReturn on PaymentType 
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	values( NEWID(), @ReportId, 'PaymentReturnCount',0, @SortOrder);

	set @SortOrder=@SortOrder+1;
	
	/*INSERT INTO #TempReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeReturn', Sum((Payment.UnitPrice*Payment.Qty*Payment.Direction)-Payment.ItemDiscount)
		, Payment.PaymentType, @SortOrder
			FROM #TempPaymentDataFromOrderDetail Payment
			WHERE Payment.Direction = -1 
			GROUP BY Payment.PaymentType;*/

	set @SortOrder=@SortOrder+1;

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeReturn', Sum(Payment.PaidAmount), Payment.PaymentType, @SortOrder
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated = 1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = -1 
					AND OrderMaster.Status in (15,18) and OrderMaster.TerminalId = @TerminalId
						GROUP BY Payment.PaymentType;
						
	set @SortOrder=@SortOrder+1;

		-- ### Get number of standard orders
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'OrderCount', COUNT(*), @SortOrder
		FROM #TempData 
		WHERE (Status=13 or Status = 17);

	set @SortOrder=@SortOrder+1;

	-- ### Get total sale prodcut count of given period
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'ProductsCount',Sum(OrderDetail.Qty), @SortOrder
		FROM #TempData OrderDetail 
		WHERE  (Status =13 or Status = 17) AND OrderDetail.Direction=1;

	set @SortOrder=@SortOrder+1;

	-- ### Get Category Sale	
	CREATE TABLE #TempCategoryData(
 
	[UnitPrice] [decimal](12, 2) NULL,
	[TaxPercent] [decimal](12, 2) NULL,
	[Qty] [decimal](12, 3) NULL,
	[Direction] [decimal](12, 2) NULL,
	[ItemDiscount] [decimal](12, 2) NULL,	
	[Type] [int] NULL,
	[Status] [int] NULL,
	[Name] varchar(200) null,
	Id [int] null
)
INSERT INTO #TempCategoryData(UnitPrice,Qty,Direction,ItemDiscount,Name,Id)
SELECT OrderDetail.UnitPrice,OrderDetail.Qty,OrderDetail.Direction,OrderDetail.ItemDiscount, Category.Name, Category.Id
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join ItemCategory on OrderDetail.ItemId=ItemCategory.ItemId AND ItemCategory.IsPrimary=1
			inner join Category on ItemCategory.CategoryId=Category.Id
				WHERE   OrderMaster.TrainingMode=0 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND 
				OrderMaster.Status in (13,15,17)  AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	VALUES( NEWID(), @ReportId, 'CategorySaleHeading',0,'',0,@SortOrder);

	set @SortOrder=@SortOrder+1;
	
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	SELECT NEWID(), @ReportId, 'CategorySale', Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount), OrderDetail.Name, 0, @SortOrder
	FROM #TempCategoryData OrderDetail
	GROUP BY OrderDetail.Name,OrderDetail.Id
	order by OrderDetail.Name;

	set @SortOrder=@SortOrder+1;

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	VALUES( NEWID(), @ReportId, 'CategorySaleCountHeading',0,'',0,@SortOrder);
	
	set @SortOrder=@SortOrder+1;

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	SELECT NEWID(), @ReportId, 'CategorySaleCount', Sum(OrderDetail.Qty), OrderDetail.Name,OrderDetail.Id as CategoryId, @SortOrder
	FROM #TempCategoryData OrderDetail
	GROUP BY OrderDetail.Name,OrderDetail.Id
	order by OrderDetail.Name;

	set @SortOrder=@SortOrder+1;

	
	

--### Accounting
CREATE TABLE #TempAccounting(
 
	[UnitPrice] [decimal](12, 2) NULL,
	[TaxPercent] [decimal](12, 2) NULL,
	[Qty] [decimal](12, 2) NULL,
	[Direction] [decimal](12, 2) NULL,
	[ItemDiscount] [decimal](12, 2) NULL,
	[AccountingId] [int] NULL,
	[Type] [int] NULL,
	[Status] [int] NULL
)
INSERT INTO #TempAccounting(UnitPrice,Qty,Direction,ItemDiscount,TaxPercent,AccountingId,[Type],[Status])
SELECT                     OrderDetail.UnitPrice,OrderDetail.Qty,OrderDetail.Direction,OrderDetail.ItemDiscount,OrderDetail.TaxPercent,AccountingId,OrderMaster.Type,OrderMaster.Status
	FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Product on Product.Id=OrderDetail.itemID
			
				WHERE   OrderMaster.TrainingMode=0  AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1    AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId
		

--### Accounting
DECLARE @LoopCounter INT = 1, @MaxAccountingId INT  , 
        @AccountingId int,@accountingTotal decimal(12,2),@Title varchar(100)
 select @MaxAccountingId=MAX(Id) from Accounting
 INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue, SortOrder)
 Values(NEWID(), @ReportId, 'Accounting',' ',@SortOrder);
 	
set @SortOrder=@SortOrder+1;

WHILE(@LoopCounter <= @MaxAccountingId)
BEGIN
   SELECT @AccountingId = Id
   FROM Accounting WHERE Id = @LoopCounter   
   set @SortOrder=@SortOrder+1;
 select @Title=(cast(Accounting.AcNo as varchar(10))+'-'+ Accounting.Name) from Accounting where Id=@AccountingId
  SELECT @accountingTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount)
	FROM  #TempAccounting OrderDetail
			
				WHERE  OrderDetail.AccountingId=@AccountingId AND OrderDetail.Status<>14 
		
		if(@accountingTotal is not null)
		begin
		--##empty line	
	 INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue,  SortOrder)
 Values(NEWID(), @ReportId, ' ',' ',@SortOrder);		
  set @SortOrder=@SortOrder+1;
	INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACTotal',@Title,@AccountingId,@accountingTotal,@SortOrder)	
	
	end

				SELECT @accountingTotal=Sum((((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)))
	FROM #TempAccounting OrderDetail
			
				WHERE  OrderDetail.AccountingId=@AccountingId AND OrderDetail.Status<>14 
			if(@accountingTotal is not null)	
			Begin	
				   set @SortOrder=@SortOrder+1;
INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACNetTotal',@Title,@AccountingId,@accountingTotal,@SortOrder)	
	end
	--##Acoounitng Standared Total
			  SELECT @accountingTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount)
	FROM #TempAccounting OrderDetail
			
				WHERE  OrderDetail.AccountingId=@AccountingId AND OrderDetail.Status<>14 AND OrderDetail.Type=0
					if(@accountingTotal is not null)
					begin
				   set @SortOrder=@SortOrder+1;
				INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACStandared',@Title,@AccountingId,@accountingTotal,@SortOrder)
	end
	--##Acoounitng Takeaway Total
				  SELECT @accountingTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount)
	FROM #TempAccounting OrderDetail
			
				WHERE  OrderDetail.AccountingId=@AccountingId AND OrderDetail.Status<>14 AND OrderDetail.Type=3
					if(@accountingTotal is not null)
					begin
				   set @SortOrder=@SortOrder+1;
				INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACTakeaway',@Title,@AccountingId,@accountingTotal,@SortOrder)
	end
	
				--##Acoounitng Takeaway Total
				  SELECT @accountingTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount)
	FROM #TempAccounting OrderDetail
			
				WHERE  OrderDetail.AccountingId=@AccountingId AND OrderDetail.Status<>14 AND OrderDetail.Type=6
					if(@accountingTotal is not null)
					begin
				   set @SortOrder=@SortOrder+1;
				INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACTable',@Title,@AccountingId,@accountingTotal,@SortOrder)
	end

	SELECT  @accountingTotal= isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0)
		FROM #TempAccounting OrderDetail
			
				WHERE  OrderDetail.AccountingId=@AccountingId  AND OrderDetail.Direction=1
		 if(@accountingTotal>0)
					begin
					--##empty line	
	 INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue,  SortOrder)
 Values(NEWID(), @ReportId, ' ',' ',@SortOrder);		
  set @SortOrder=@SortOrder+1;
		  
				INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue,ForeignId, Value, SortOrder)
	Values(NEWID(), @ReportId, 'ACVatSum',@Title,@AccountingId,@accountingTotal,@SortOrder)
	

	  set @SortOrder=@SortOrder+1;
	  INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent,TextValue, SortOrder)
	SELECT NEWID(), @ReportId, 'ACVATPercent', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,OrderDetail.TaxPercent,CAST( OrderDetail.TaxPercent as varchar(10))+'%', @SortOrder
		FROM #TempAccounting OrderDetail
			
				WHERE  OrderDetail.AccountingId=@AccountingId  AND OrderDetail.Direction=1
					GROUP BY (OrderDetail.TaxPercent);
					end
   SET @LoopCounter  = @LoopCounter  + 1        
END


-- ### Open cash drawer count

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'CashDrawerOpenCount', Count(*), 71
			FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId
 
 	
-- ### Receipt count

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'ReceiptCount', Count(*), 72
			FROM Receipt WHERE printDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId
			
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		VALUES (NEWID(), @ReportId, 'ReceiptCopyCount', 0, 73)
	--INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		--VALUES (NEWID(), @ReportId, 'ReceiptCopyAmount', 0, 56)


	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		VALUES (NEWID(), @ReportId, 'ServicesCount', 0, 74)


		
		
	

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	SELECT NEWID(), @ReportId, 'Discount', Sum(OrderDetail.ItemDiscount), 79
		FROM #TempData OrderDetail 
			
				WHERE  (Status =13 or Status = 17) AND OrderDetail.Direction=1

INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	SELECT NEWID(), @ReportId, 'DiscountCount', COUNT(*), 79
		FROM #TempData OrderDetail 
			
				WHERE  (Status =13 or Status = 17) AND OrderDetail.Direction=1


	-- Get number of return orders
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnCount', COUNT(*), 81
		FROM OrderMaster 
				WHERE 
					 OrderMaster.InvoiceGenerated=1 AND OrderMaster.Type=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId;

	

-- ### Get number of training sale orders
					INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)	
		SELECT NEWID(), @ReportId, 'TrainingModeCount', count(Id), 82
		FROM OrderMaster 
				WHERE    OrderMaster.InvoiceGenerated=1 AND OrderMaster.TrainingMode=1  AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND OrderMaster.TerminalId = @TerminalId;

	-- Get number of return orders
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		VALUES( NEWID(), @ReportId, 'HoldCount', 0, 83)

	-- ### Get total sale of unfinished orders on given period
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		VALUES( NEWID(), @ReportId, 'HoldTotalSale', 0, 84)
		

	
	

	-- ### Get acumulated total sale without taking consideration of return orders
		SELECT @SaleTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount)
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND (OrderMaster.Status=13 or OrderMaster.Status = 17) AND OrderMaster.InvoiceGenerated=1  AND OrderMaster.InvoiceDate <=@CloseDate AND OrderDetail.Active=1    AND  OrderDetail.ItemType<>1   AND  OrderMaster.TerminalId = @TerminalId;


SELECT @SaleReturn= Isnull(Sum(Round((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount,2)),0)
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
				WHERE OrderMaster.TrainingMode=0 AND (OrderMaster.Status=15 or OrderMaster.Status = 18) AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.InvoiceDate <=@CloseDate AND OrderDetail.Direction=-1 AND OrderDetail.Active=1   AND  OrderDetail.ItemType=0    AND  OrderMaster.TerminalId = @TerminalId;



	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	values(NEWID(), @ReportId, 'GrandTotalSale',@SaleTotal,100)
	-- ### Get acumulated total return
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		values(NEWID(), @ReportId, 'GrandTotalReturn',@SaleReturn,101)
	-- ### Get acumulated total return
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	values( NEWID(), @ReportId, 'GrandTotalNet',@SaleTotal-@SaleReturn,102)
		

			
     select *from #TempReportData



END

