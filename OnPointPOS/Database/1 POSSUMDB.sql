
/****** Object:  UserDefinedFunction [dbo].[Fn_CategoryByItem]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2019-02-05
-- Description:	Get Cateogory Name by Item Id
-- =============================================
CREATE FUNCTION [dbo].[Fn_CategoryByItem]
(
	@itemid uniqueidentifier  
)
RETURNS  varchar(100)
AS
BEGIN
	-- Declare the return variable here
	Declare @cateName varchar(100);
select  @cateName=Category.Name FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
			inner JOIN Category ON ItemCategory.CategoryId = Category.Id
		Where Product.Id=@itemid
	-- Return the result of the function

	RETURN isnull(@cateName,' ')

END




































GO
/****** Object:  UserDefinedFunction [dbo].[Fn_CategoryIdByItem]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2015-12-10
-- Description:	Get Cateogory Name by Item Id
-- =============================================
CREATE FUNCTION [dbo].[Fn_CategoryIdByItem]
(
	@itemid uniqueidentifier 
)
RETURNS  int
AS
BEGIN
	-- Declare the return variable here
	Declare @catId int;
select  @catId=Category.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
			inner JOIN Category ON ItemCategory.CategoryId = Category.Id
		Where Product.Id=@itemid
	-- Return the result of the function

	RETURN isnull(@catId,0)

END
































GO
/****** Object:  UserDefinedFunction [dbo].[Fn_ItemDesc]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2015-11-03
-- Description:	Get item qty, unitprice and detail to print journal
-- =============================================

CREATE FUNCTION [dbo].[Fn_ItemDesc]
(
	@itemid uniqueidentifier,@orderId uniqueidentifier 
)
RETURNS  varchar(100)
AS
BEGIN
	-- Declare the return variable here
	Declare @itemDesc varchar(100);
DECLARE @qty int;

	DECLARE @vat varchar(10);

	DECLARE @unitprice varchar(10);
		DECLARE @taxPerc decimal;
		
			SELECT top 1  @qty=sum( OrderDetail.Qty) ,@unitprice=Cast(Cast(Round(Sum(OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100)),2) as numeric(36,2)) as varchar(10)),@vat=CAST(cast(Round(Sum((OrderDetail.UnitPrice*OrderDetail.Qty/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),2) as numeric(36,2)) as varchar(10)),@taxPerc=Sum(OrderDetail.TaxPercent)
                             FROM dbo.OrderDetail
							 Where OrderId=@orderId AND ItemId=@itemid
							 set @itemDesc='qty:'+Cast(@qty as varchar(10))+ ' unitprice:'+@unitprice+' vat:'+@vat+' vat%:'+Cast(@taxPerc as varchar(10))
	-- Return the result of the function

	RETURN isnull(@itemDesc,' ')

END





































GO
/****** Object:  UserDefinedFunction [dbo].[Fn_OrderPaidTotal]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2015-12-10
-- Description:	Get Cateogory Name by Item Id
-- =============================================
CREATE FUNCTION [dbo].[Fn_OrderPaidTotal]
(
	@orderId uniqueidentifier 
)
RETURNS  decimal
AS
BEGIN
	-- Declare the return variable here
	Declare @orderTotal decimal;
 select @orderTotal= SUM(PaidAmount) from Payment Where OrderId=@orderId AND  Direction=1

	RETURN isnull(@orderTotal,0)

END





































GO
/****** Object:  UserDefinedFunction [dbo].[Fn_OrderTotal]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2015-12-10
-- Description:	Get Cateogory Name by Item Id
-- =============================================
CREATE FUNCTION [dbo].[Fn_OrderTotal]
(
	@orderId uniqueidentifier 
)
RETURNS  decimal
AS
BEGIN
	-- Declare the return variable here
	Declare @orderTotal decimal;
 select @orderTotal= SUM((Direction*Qty)*UnitPrice-ItemDiscount) from OrderDetail Where OrderId=@orderId AND OrderDetail.ItemType<>1 AND  OrderDetail.Active=1

	RETURN isnull(@orderTotal,0)

END





































GO
/****** Object:  UserDefinedFunction [dbo].[Fn_OutletById]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2015-12-10
-- Description:	Get Cateogory Name by Item Id
-- =============================================
CREATE FUNCTION [dbo].[Fn_OutletById]
(
	@outletId uniqueidentifier 
)
RETURNS  varchar(150)
AS
BEGIN
	-- Declare the return variable here
	Declare @cateName varchar(100);
select  @cateName=Name FROM  Outlet Where Id=@outletId
	-- Return the result of the function

	RETURN isnull(@cateName,' ')

END

























GO
/****** Object:  UserDefinedFunction [dbo].[Fn_SaleByAccountingPercentage]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2016-06-27
-- Description:	Get Total Sale of Item by Cateogory  AND Percentage
-- =============================================
CREATE FUNCTION [dbo].[Fn_SaleByAccountingPercentage]
(
     @ValueType Int,
	 @TerminalId UNIQUEIDENTIFIER,
	 @AccountingId int ,
	 @Perc decimal,
	 @OpenDate datetime,
	 @CloseDate datetime
)
RETURNS  decimal(12,2)
AS
BEGIN
	-- Declare the return variable here
	Declare @grossTotal varchar(100);
	IF @TerminalId<>'00000000-0000-0000-0000-000000000000'
	BEGIN
						IF @ValueType=1

						-- Return the result of the function
						 SELECT @grossTotal=sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount)
							FROM OrderMaster LEFT JOIN OrderDetail 
								ON OrderMaster.Id = OrderDetail.OrderId 
									WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1  AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.TerminalId=@TerminalId  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product Where Product.[Tax]=@Perc AND Product.AccountingId=@AccountingId) ;

					ELSE IF @ValueType=2
					 SELECT @grossTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))
							FROM OrderMaster LEFT JOIN OrderDetail 
								ON OrderMaster.Id = OrderDetail.OrderId 
									WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1   AND  OrderDetail.ItemType<>1  AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.TerminalId=@TerminalId  AND OrderDetail.ItemId in (  select  Distinct Product.Id FROM  Item Where Product.[Tax]=@Perc AND Product.AccountingId=@AccountingId);

					ELSE
					 SELECT @grossTotal=Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100))
							FROM OrderMaster LEFT JOIN OrderDetail 
								ON OrderMaster.Id = OrderDetail.OrderId 
									WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1  AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.TerminalId=@TerminalId  AND OrderDetail.ItemId in (  select  Distinct Product.Id FROM  Item Where Product.[Tax]=@Perc AND Product.AccountingId=@AccountingId) ;
 END
 ELSE
 BEGIN
 IF @ValueType=1

						-- Return the result of the function
						 SELECT @grossTotal=sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount)
							FROM OrderMaster LEFT JOIN OrderDetail 
								ON OrderMaster.Id = OrderDetail.OrderId 
									WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1  AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1   AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Item Where Product.[Tax]=@Perc AND Product.AccountingId=@AccountingId) ;

					ELSE IF @ValueType=2
					 SELECT @grossTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))
							FROM OrderMaster LEFT JOIN OrderDetail 
								ON OrderMaster.Id = OrderDetail.OrderId 
									WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1  AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1   AND OrderDetail.ItemId in (  select  Distinct Product.Id FROM  Item Where Product.[Tax]=@Perc AND Product.AccountingId=@AccountingId);

					ELSE
					 SELECT @grossTotal=Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100))
							FROM OrderMaster LEFT JOIN OrderDetail 
								ON OrderMaster.Id = OrderDetail.OrderId 
									WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1   AND  OrderDetail.ItemType<>1  AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1  AND OrderDetail.ItemId in (  select  Distinct Product.Id FROM  Item Where Product.[Tax]=@Perc AND Product.AccountingId=@AccountingId) ;

 END


	RETURN isnull(@grossTotal,0)


END


GO
/****** Object:  UserDefinedFunction [dbo].[Fn_SaleByCategoryPercentage]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2016-06-27
-- Description:	Get Total Sale of Item by Cateogory  AND Percentage
-- =============================================
CREATE FUNCTION [dbo].[Fn_SaleByCategoryPercentage]
(
	@ValueType Int,
	@TerminalId UNIQUEIDENTIFIER,
	 @cateName varchar(100) ,
	 @Perc decimal,
	 @OpenDate datetime,
	 @CloseDate datetime
)
RETURNS  decimal(12,2)
AS
BEGIN
	-- Declare the return variable here
	Declare @grossTotal varchar(100);
	IF @TerminalId<>'00000000-0000-0000-0000-000000000000'
	BEGIN
	       IF @ValueType=1

			-- Return the result of the function
			 SELECT @grossTotal=sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount)
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType=0   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.TerminalId=@TerminalId  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName) ;

		ELSE IF @ValueType=2
		 SELECT @grossTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType=0   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.TerminalId=@TerminalId  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName);

					ELSE
			 SELECT @grossTotal=Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100))
					FROM OrderMaster LEFT JOIN OrderDetail 
						ON OrderMaster.Id = OrderDetail.OrderId 
							WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType=0   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.TerminalId=@TerminalId  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
						inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
					Where Product.[Tax]=@Perc AND Category.Name Like @cateName) ;
END
ELSE
BEGIN
					IF @ValueType=1

			-- Return the result of the function
			 SELECT @grossTotal=sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount)
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1  AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName) ;

		ELSE IF @ValueType=2
		 SELECT @grossTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType=0   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1   AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName);

		ELSE
		 SELECT @grossTotal=Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100))
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType=0   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName) ;
END

	RETURN isnull(@grossTotal,0)

END



GO
/****** Object:  UserDefinedFunction [dbo].[Fn_SaleByUserPercentage]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2016-06-27
-- Description:	Get Total Sale of Item by Cateogory  AND Percentage
-- =============================================
CREATE FUNCTION [dbo].[Fn_SaleByUserPercentage]
(
     @ValueType Int,
	@UserId UNIQUEIDENTIFIER,
	 @cateName varchar(100) ,
	 @Perc decimal,
	 @OpenDate datetime,
	 @CloseDate datetime
)
RETURNS  decimal(12,2)
AS
BEGIN
	-- Declare the return variable here
	Declare @grossTotal varchar(100);
	IF @UserId<>'00000000-0000-0000-0000-000000000000'
	BEGIN
	       IF @ValueType=1

			-- Return the result of the function
			 SELECT @grossTotal=sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount)
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.UserId=@UserId  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName) ;

		ELSE IF @ValueType=2
		 SELECT @grossTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)) 
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.UserId=@UserId  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName);

					ELSE
			 SELECT @grossTotal=Sum((OrderDetail.Direction*OrderDetail.Qty*OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100))
					FROM OrderMaster LEFT JOIN OrderDetail 
						ON OrderMaster.Id = OrderDetail.OrderId 
							WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1 AND OrderMaster.UserId=@UserId  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
						inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
					Where Product.[Tax]=@Perc AND Category.Name Like @cateName) ;
END
ELSE
BEGIN
					IF @ValueType=1

			-- Return the result of the function
			 SELECT @grossTotal=sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount)
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1   AND  OrderDetail.ItemType<>1  AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName) ;

		ELSE IF @ValueType=2
		 SELECT @grossTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)) 
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1   AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName);

		ELSE
		 SELECT @grossTotal=Sum((OrderDetail.Direction*OrderDetail.Qty*OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100))
				FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
						WHERE OrderMaster.TrainingMode=0 AND OrderDetail.Active=1    AND  OrderDetail.ItemType<>1   AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1  AND OrderDetail.Active=1  AND OrderDetail.ItemId in ( select  Distinct Product.Id FROM  Product inner JOIN ItemCategory ON Product.Id = ItemCategory.ItemId
					inner JOIN Category ON ItemCategory.CategoryId = Category.Id  
				Where Product.[Tax]=@Perc AND Category.Name Like @cateName) ;
END

	RETURN isnull(@grossTotal,0)

END



































GO
/****** Object:  Table [dbo].[Accounting]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounting](
	[Id] [int] NOT NULL,
	[AcNo] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[IsDeleted] [bit] NOT NULL,
	[TAX] [decimal](18, 2) NOT NULL,
	[Updated] [datetime] NOT NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Accounting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BongCounter]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BongCounter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Counter] [int] NOT NULL,
	[BarCounter] [int] NOT NULL,
 CONSTRAINT [PK_dbo.BongCounter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Campaign]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Campaign](
	[Id] [int] NOT NULL,
	[BuyLimit] [int] NOT NULL,
	[FreeOffer] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Updated] [datetime] NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[IsDiscount] [bit] NOT NULL,
	[DiscountPercentage] [decimal](8, 2) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[OnceOnly] [bit] NOT NULL,
	[LimitDiscountPercentage] [int] NOT NULL,
	[DiscountType] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CashDrawer]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashDrawer](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Location] [nvarchar](max) NULL,
	[UserId] [nvarchar](max) NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
	[ConnectionString] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.CashDrawer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CashDrawerLog]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashDrawerLog](
	[Id] [uniqueidentifier] NOT NULL,
	[CashDrawerId] [uniqueidentifier] NOT NULL,
	[ActivityDate] [datetime] NOT NULL,
	[ActivityType] [int] NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[Synced] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CashDrawerLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Category]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Parant] [int] NOT NULL,
	[CategoryLevel] [int] NOT NULL,
	[Type] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[Deleted] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
	[ColorCode] [nvarchar](max) NULL,
	[SortOrder] [int] NOT NULL,
	[IconId] [int] NULL,
	[ReportOrder] [int] NOT NULL,
	[ImageURL] [nvarchar](50) NULL,
	[Description] [nvarchar](50) NULL,
	[WC_ID] [int] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_dbo.Category] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CategoryCampaign]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CategoryCampaign](
	[Id] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[CampaignId] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[Updated] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Client]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Client](
	[Id] [int] NOT NULL,
	[clientId] [nvarchar](max) NULL,
	[clientRequestTimeout] [bigint] NULL,
	[password] [nvarchar](max) NULL,
	[connectionString] [nvarchar](max) NULL,
	[ClientUserId] [nvarchar](max) NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.Client] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Customer]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[OrgNo] [nvarchar](50) NULL,
	[Address1] [nvarchar](250) NULL,
	[Address2] [nvarchar](250) NULL,
	[FloorNo] [int] NOT NULL,
	[PortCode] [int] NOT NULL,
	[CustomerNo] [nvarchar](100) NULL,
	[City] [nvarchar](50) NULL,
	[Phone] [nvarchar](50) NULL,
	[ZipCode] [nvarchar](50) NULL,
	[Reference] [nvarchar](50) NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
	[DirectPrint] [bit] NOT NULL,
	[Active] [bit] NOT NULL,
	[Email] [nvarchar](50) NULL,
	[ExternalId] [nvarchar](50) NULL,
	[PinCode] [nvarchar](50) NULL,
	[LastBalanceUpdated] [datetime] NULL,
	[DepositAmount] [decimal](18, 2) NOT NULL DEFAULT ((0)),
	[HasDeposit] [bit] NOT NULL DEFAULT ((0)),
	[WC_ID] [int] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_dbo.Customer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Customer_CustomField]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer_CustomField](
	[Id] [uniqueidentifier] NOT NULL,
	[FieldId] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[Caption] [nvarchar](max) NULL,
	[Text] [nvarchar](max) NULL,
	[SortOrder] [int] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.Customer_CustomField] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerBonus]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerBonus](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[OutletId] [uniqueidentifier] NOT NULL,
	[ChangeValue] [decimal](18, 2) NOT NULL,
	[CurrentSum] [decimal](18, 2) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CustomerBonus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerCard]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerCard](
	[CardId] [uniqueidentifier] NOT NULL,
	[UniqueId] [nvarchar](200) NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](max) NULL,
	[Active] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CustomerCard] PRIMARY KEY CLUSTERED 
(
	[CardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerCustomField]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerCustomField](
	[Id] [uniqueidentifier] NOT NULL,
	[Caption] [nvarchar](max) NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CustomerCustomField] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerDiscountGroup]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerDiscountGroup](
	[Id] [uniqueidentifier] NOT NULL,
	[DiscountId] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[Deleted] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CustomerDiscountGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CustomerInvoice]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerInvoice](
	[Id] [uniqueidentifier] NOT NULL,
	[InvoiceNumber] [bigint] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[Remarks] [nvarchar](250) NULL,
	[InvoiceTotal] [decimal](18, 2) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Synced] [bit] NOT NULL,
	[PaymentStatus] [int] NOT NULL,
	[PaidDate] [datetime] NULL,
	[DueDate] [datetime] NULL,
	[OutletId] [uniqueidentifier] NOT NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_dbo.CustomerInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DepositHistory]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DepositHistory](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[DepositAmount] [decimal](18, 2) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[OrderId] [uniqueidentifier] NULL,
	[DepositType] [int] NOT NULL,
	[MerchantReceipt] [nvarchar](max) NULL,
	[CustomerReceipt] [nvarchar](max) NULL,
	[OldBalance] [decimal](18, 2) NOT NULL,
	[NewBalance] [decimal](18, 2) NOT NULL,
	[TerminalId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DiscountGroup]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiscountGroup](
	[DiscountId] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](max) NULL,
	[Discount] [decimal](18, 2) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.DiscountGroup] PRIMARY KEY CLUSTERED 
(
	[DiscountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Employee]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employee](
	[Id] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[SSNO] [nvarchar](50) NULL,
	[Email] [nvarchar](150) NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
 CONSTRAINT [PK_dbo.Employee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EmployeeLog]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeLog](
	[LogId] [uniqueidentifier] NOT NULL,
	[EmployeeId] [uniqueidentifier] NOT NULL,
	[CheckIn] [datetime] NULL,
	[CheckOut] [datetime] NULL,
	[Completed] [bit] NOT NULL,
	[Synced] [int] NOT NULL,
 CONSTRAINT [PK_dbo.EmployeeLog] PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExceptionLog]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExceptionLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ExceptiontText] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
	[Synced] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.ExceptionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Floor]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Floor](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Floor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FoodTable]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoodTable](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[FloorId] [int] NOT NULL,
	[PositionX] [int] NOT NULL,
	[PositionY] [int] NOT NULL,
	[Height] [int] NOT NULL,
	[Width] [int] NOT NULL,
	[Chairs] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[ImageUrl] [nvarchar](max) NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.FoodTable] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[IconStore]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[IconStore](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [int] NOT NULL,
	[Title] [nvarchar](max) NULL,
	[Photo] [varbinary](max) NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
 CONSTRAINT [PK_dbo.IconStore] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[impProduct]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[impProduct](
	[Barcode] [float] NULL,
	[Namn] [nvarchar](255) NULL,
	[kost] [float] NULL,
	[Category] [nvarchar](255) NULL,
	[Quantity] [float] NULL,
	[VAT] [float] NULL,
	[Percentage] [float] NULL,
	[Price] [float] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Inbox]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Inbox](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[IsRead] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Inbox] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[InventoryTask]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryTask](
	[Id] [uniqueidentifier] NOT NULL,
	[Priority] [int] NOT NULL,
	[Type] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[StatusMessage] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[Executed] [datetime] NULL,
	[Processed] [datetime] NULL,
	[ItemId] [uniqueidentifier] NULL,
	[OrderGuid] [uniqueidentifier] NULL,
 CONSTRAINT [PK_dbo.InventoryTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[InvoiceCounter]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InvoiceCounter](
	[Id] [int] NOT NULL,
	[LastNo] [nvarchar](max) NULL,
	[InvoiceType] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InvoiceCounter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ItemCategory]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[IsPrimary] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.ItemCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ItemInventory]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemInventory](
	[ItemInventoryID] [uniqueidentifier] NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[WarehouseID] [uniqueidentifier] NOT NULL,
	[WarehouseLocationID] [uniqueidentifier] NOT NULL,
	[StockCount] [int] NOT NULL,
	[StockReservations] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ItemInventory] PRIMARY KEY CLUSTERED 
(
	[ItemInventoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ItemStock]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemStock](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[BatchNo] [nvarchar](max) NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.ItemStock] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ItemTransaction]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemTransaction](
	[ItemTransactionID] [uniqueidentifier] NOT NULL,
	[ItemID] [uniqueidentifier] NOT NULL,
	[OutletID] [uniqueidentifier] NOT NULL,
	[TerminalID] [uniqueidentifier] NOT NULL,
	[OrderID] [uniqueidentifier] NOT NULL,
	[WarehouseID] [uniqueidentifier] NOT NULL,
	[Qty] [decimal](18, 2) NOT NULL,
	[Direction] [int] NOT NULL,
	[TransactionDate] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.ItemTransaction] PRIMARY KEY CLUSTERED 
(
	[ItemTransactionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Journal]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Journal](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[OrderId] [uniqueidentifier] NULL,
	[ItemId] [uniqueidentifier] NULL,
	[TableId] [int] NULL,
	[ActionId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[LogMessage] [nvarchar](150) NULL,
	[TerminalId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_dbo.Journal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[JournalAction]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JournalAction](
	[Id] [int] NOT NULL,
	[Type] [nvarchar](max) NULL,
	[ActionCode] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Description2] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.JournalAction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Language]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Language](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Code] [nvarchar](max) NULL,
	[IsDefault] [bit] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.Language] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MQTTBuffer]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MQTTBuffer](
	[Id] [uniqueidentifier] NOT NULL,
	[MessageId] [uniqueidentifier] NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NULL,
	[Action] [nvarchar](max) NULL,
	[JsonData] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.MQTTBuffer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MQTTClient]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MQTTClient](
	[Id] [uniqueidentifier] NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[LastPing] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.MQTTClient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OrderDetail]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[Qty] [numeric](18, 10) NOT NULL,
	[DiscountedUnitPrice] [decimal](8, 2) NOT NULL,
	[PurchasePrice] [decimal](8, 2) NOT NULL,
	[ItemDiscount] [numeric](18, 10) NOT NULL,
	[TaxPercent] [decimal](18, 2) NOT NULL,
	[Active] [int] NOT NULL,
	[ItemComments] [nvarchar](max) NULL,
	[UnitPrice] [decimal](8, 2) NOT NULL,
	[DiscountPercentage] [decimal](8, 2) NOT NULL,
	[UnitsInPackage] [int] NOT NULL,
	[ItemStatus] [int] NOT NULL,
	[IsCoupon] [int] NOT NULL,
	[Direction] [int] NOT NULL,
	[DiscountType] [int] NOT NULL,
	[DiscountDescription] [nvarchar](max) NULL,
	[ItemType] [int] NOT NULL,
	[GroupId] [uniqueidentifier] NOT NULL,
	[GroupKey] [uniqueidentifier] NULL,
	[IngredientMode] [nvarchar](max) NULL,
	[ParentID] [uniqueidentifier] NULL,
	[ProductName] [nvarchar](50) NULL,
	[IsInventoryAdjusted] [bit] NOT NULL,
	[WC_ID] [int] NOT NULL,
	[ItemDescription] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.OrderDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OrderMaster]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[OrderMaster](
	[Id] [uniqueidentifier] NOT NULL,
	[TableId] [int] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[OrderTotal] [numeric](18, 10) NOT NULL,
	[OrderNoOfDay] [nvarchar](20) NULL,
	[Status] [int] NOT NULL,
	[PaymentStatus] [int] NOT NULL,
	[Updated] [int] NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[TaxPercent] [decimal](18, 2) NOT NULL,
	[InvoiceNumber] [nvarchar](20) NULL,
	[InvoiceDate] [datetime] NULL,
	[InvoiceGenerated] [int] NOT NULL,
	[TipAmount] [decimal](18, 2) NOT NULL,
	[Comments] [nvarchar](max) NULL,
	[ShiftNo] [int] NOT NULL,
	[ShiftOrderNo] [int] NOT NULL,
	[ShiftClosed] [int] NOT NULL,
	[TipAmountType] [int] NOT NULL,
	[ZPrinted] [int] NOT NULL,
	[CheckOutUserId] [nvarchar](40) NULL,
	[OrderComments] [nvarchar](max) NULL,
	[Type] [int] NOT NULL,
	[CustomerInvoiceId] [uniqueidentifier] NULL,
	[Bong] [nvarchar](max) NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
	[OutletId] [uniqueidentifier] NOT NULL,
	[TrainingMode] [bit] NOT NULL,
	[RoundedAmount] [decimal](8, 3) NULL,
	[DailyBong] [nvarchar](max) NULL,
	[ExternalID] [nvarchar](50) NULL,
	[IsVismaInvoiceGenerated] [bit] NOT NULL,
	[OrderIntID] [int] NULL,
	[IsInInvoice] [bit] NOT NULL,
	[OrderSource] [int] NOT NULL,
	[WC_ID] [int] NOT NULL,
	[DiscountTotal] [decimal](18, 2) NOT NULL,
	[DiscountTax] [decimal](18, 2) NOT NULL,
	[IsOnlineOrder] [bit] NOT NULL,
	[FnResponse] [varchar](max) NULL,
 CONSTRAINT [PK_dbo.OrderMaster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Outlet]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Outlet](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Address1] [nvarchar](max) NULL,
	[Address2] [nvarchar](max) NULL,
	[Address3] [nvarchar](max) NULL,
	[City] [nvarchar](max) NULL,
	[PostalCode] [nvarchar](max) NULL,
	[BillPrinterId] [int] NOT NULL,
	[KitchenPrinterId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[Email] [nvarchar](max) NULL,
	[WebUrl] [nvarchar](max) NULL,
	[Phone] [nvarchar](max) NULL,
	[OrgNo] [nvarchar](max) NULL,
	[HeaderText] [nvarchar](max) NULL,
	[FooterText] [nvarchar](max) NULL,
	[TaxDescription] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
	[WarehouseID] [uniqueidentifier] NOT NULL,
	[UniqueCode] [nvarchar](max) NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Outlet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[OutletUser]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OutletUser](
	[Id] [nvarchar](128) NOT NULL,
	[OutletId] [uniqueidentifier] NOT NULL,
	[Email] [nvarchar](max) NULL,
	[UserCode] [nvarchar](max) NULL,
	[UserName] [nvarchar](max) NULL,
	[Password] [nvarchar](max) NULL,
	[TrainingMode] [bit] NOT NULL,
	[Active] [bit] NOT NULL,
	[DallasKey] [nvarchar](max) NULL,
	[Updated] [datetime] NULL,
 CONSTRAINT [PK_dbo.OutletUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Payment]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[Payment](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[PaymentType] [int] NOT NULL,
	[PaidAmount] [decimal](18, 10) NULL,
	[ReturnAmount] [decimal](8, 2) NOT NULL,
	[PaymentDate] [datetime] NOT NULL,
	[PaymentRef] [nvarchar](max) NULL,
	[CreditCardNo] [nvarchar](max) NULL,
	[TipAmount] [decimal](8, 2) NOT NULL,
	[CashCollected] [decimal](8, 2) NOT NULL,
	[CashChange] [decimal](8, 2) NOT NULL,
	[IsCashSaleDropped] [int] NOT NULL,
	[Direction] [int] NOT NULL,
	[ProductName] [nvarchar](max) NULL,
	[DeviceTotal] [decimal](18, 2) NOT NULL,
	[PayerRef] [varchar](100) NULL,
 CONSTRAINT [PK_dbo.Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PaymentDeviceLog]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentDeviceLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[OrderTotal] [decimal](18, 2) NOT NULL,
	[VatAmount] [decimal](18, 2) NOT NULL,
	[CashBack] [decimal](18, 2) NOT NULL,
	[SendDate] [datetime] NOT NULL,
	[Synced] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.PaymentDeviceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PaymentType]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[SwedishName] [nvarchar](max) NULL,
	[AccountingCode] [int] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.PaymentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PricePolicy]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PricePolicy](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BuyLimit] [int] NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.PricePolicy] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Printer]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Printer](
	[Id] [int] NOT NULL,
	[LocationName] [nvarchar](max) NULL,
	[PrinterName] [nvarchar](max) NULL,
	[TerminalId] [uniqueidentifier] NULL,
	[IPAddress] [nvarchar](max) NULL,
	[Updated] [datetime] NULL,
 CONSTRAINT [PK_dbo.Printer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Product]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[Id] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](150) NULL,
	[SKU] [nvarchar](50) NULL,
	[BarCode] [nvarchar](50) NULL,
	[PLU] [nvarchar](50) NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[PurchasePrice] [decimal](18, 2) NOT NULL,
	[Tax] [decimal](18, 2) NOT NULL,
	[Unit] [int] NOT NULL,
	[AskPrice] [bit] NOT NULL,
	[AskWeight] [bit] NOT NULL,
	[PriceLock] [bit] NOT NULL,
	[ColorCode] [nvarchar](10) NULL,
	[PrinterId] [int] NOT NULL,
	[SortOrder] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[Deleted] [bit] NOT NULL,
	[PlaceHolder] [bit] NOT NULL,
	[AskVolume] [bit] NOT NULL,
	[NeedIngredient] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
	[Bong] [bit] NOT NULL,
	[Sticky] [int] NOT NULL,
	[Seamless] [bit] NOT NULL,
	[ShowItemButton] [bit] NOT NULL,
	[AccountingId] [int] NOT NULL,
	[ReceiptMethod] [int] NOT NULL,
	[ItemType] [int] NOT NULL,
	[DiscountAllowed] [bit] NOT NULL,
	[PreparationTime] [int] NOT NULL,
	[ReorderLevelQuantity] [decimal](18, 2) NOT NULL,
	[StockQuantity] [decimal](18, 2) NOT NULL,
	[ExternalID] [nvarchar](50) NULL,
	[ProductDescription] [nvarchar](50) NULL,
	[ImageURL] [nvarchar](50) NULL,
	[PantProductId] [nvarchar](50) NULL,
	[IsPantEnabled] [bit] NOT NULL,
	[Weight] [decimal](16, 3) NULL,
	[TempPriceStart] [datetime] NULL,
	[TempPriceEnd] [datetime] NULL,
	[TempPrice] [decimal](18, 0) NULL,
	[MinStockLevel] [decimal](18, 0) NULL,
	[WC_ID] [int] NOT NULL,
	[ShortDescription] [nvarchar](max) NULL,
	[SaleType] [nvarchar](max) NULL,
	[ProductType] [nvarchar](max) NULL,
	[ArticleNumber] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Product] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Product_PricePolicy]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product_PricePolicy](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[BuyLimit] [int] NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[Active] [bit] NOT NULL,
	[Deleted] [bit] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.Product_PricePolicy] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Product_Text]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product_Text](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[LanguageId] [int] NOT NULL,
	[TextDescription] [nvarchar](max) NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.Product_Text] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductCampaign]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductCampaign](
	[Id] [int] NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[CampaignId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[Active] [bit] NOT NULL,
	[Updated] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductGroup]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductGroup](
	[Id] [int] NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[GroupId] [uniqueidentifier] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.ProductGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductPrice]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductPrice](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[PurchasePrice] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[OutletId] [uniqueidentifier] NOT NULL,
	[PriceMode] [int] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.ProductPrice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductStockHistory]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductStockHistory](
	[Id] [uniqueidentifier] NOT NULL,
	[ProductId] [uniqueidentifier] NOT NULL,
	[LastStock] [decimal](18, 2) NOT NULL,
	[NewStock] [decimal](18, 2) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[StockHistoryGroupId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Receipt]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Receipt](
	[ReceiptId] [uniqueidentifier] NOT NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
	[TerminalNo] [int] NOT NULL,
	[ReceiptNumber] [bigint] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[ReceiptCopies] [tinyint] NOT NULL,
	[GrossAmount] [decimal](8, 2) NOT NULL,
	[VatAmount] [decimal](18, 2) NOT NULL,
	[VatDetail] [nvarchar](max) NULL,
	[PrintDate] [datetime] NOT NULL,
	[ControlUnitName] [nvarchar](max) NULL,
	[ControlUnitCode] [nvarchar](max) NULL,
	[CustomerPaymentReceipt] [nvarchar](max) NULL,
	[MerchantPaymentReceipt] [nvarchar](max) NULL,
	[IsSignature] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Receipt] PRIMARY KEY CLUSTERED 
(
	[ReceiptId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Report]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Report](
	[Id] [uniqueidentifier] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[ReportType] [int] NOT NULL,
	[ReportNumber] [int] NOT NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_dbo.Report] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ReportData]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportData](
	[Id] [uniqueidentifier] NOT NULL,
	[ReportId] [uniqueidentifier] NOT NULL,
	[DataType] [nvarchar](max) NULL,
	[TextValue] [nvarchar](max) NULL,
	[ForeignId] [int] NULL,
	[Value] [decimal](18, 2) NULL,
	[TaxPercent] [decimal](18, 2) NULL,
	[DateValue] [datetime] NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ReportData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Roles]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Setting]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Setting](
	[Id] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Type] [int] NOT NULL,
	[Code] [int] NOT NULL,
	[Value] [nvarchar](max) NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
	[OutletId] [uniqueidentifier] NOT NULL,
	[Sort] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
 CONSTRAINT [PK_dbo.Setting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StockHistoryGroup]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockHistoryGroup](
	[StockHistoryGroupId] [uniqueidentifier] NOT NULL,
	[GroupName] [nvarchar](150) NULL,
	[CreatedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[StockHistoryGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Subscription]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Subscription](
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[id] [int] NOT NULL,
	[parent_id] [int] NOT NULL,
	[status] [nvarchar](max) NULL,
	[currency] [nvarchar](max) NULL,
	[version] [nvarchar](max) NULL,
	[prices_include_tax] [bit] NOT NULL,
	[date_created] [datetime] NOT NULL,
	[date_modified] [datetime] NOT NULL,
	[discount_total] [nvarchar](max) NULL,
	[discount_tax] [nvarchar](max) NULL,
	[shipping_total] [nvarchar](max) NULL,
	[shipping_tax] [nvarchar](max) NULL,
	[cart_tax] [nvarchar](max) NULL,
	[total] [nvarchar](max) NULL,
	[total_tax] [nvarchar](max) NULL,
	[customer_id] [int] NOT NULL,
	[order_key] [nvarchar](max) NULL,
	[payment_method] [nvarchar](max) NULL,
	[payment_method_title] [nvarchar](max) NULL,
	[customer_ip_address] [nvarchar](max) NULL,
	[customer_user_agent] [nvarchar](max) NULL,
	[created_via] [nvarchar](max) NULL,
	[customer_note] [nvarchar](max) NULL,
	[number] [nvarchar](max) NULL,
	[date_created_gmt] [datetime] NOT NULL,
	[date_modified_gmt] [datetime] NULL,
	[date_completed_gmt] [datetime] NULL,
	[date_paid_gmt] [datetime] NULL,
	[billing_period] [nvarchar](max) NULL,
	[billing_interval] [nvarchar](max) NULL,
	[start_date_gmt] [datetime] NOT NULL,
	[trial_end_date_gmt] [nvarchar](max) NULL,
	[next_payment_date_gmt] [nvarchar](max) NULL,
	[last_payment_date_gmt] [nvarchar](max) NULL,
	[cancelled_date_gmt] [nvarchar](max) NULL,
	[end_date_gmt] [nvarchar](max) NULL,
	[resubscribed_from] [nvarchar](max) NULL,
	[resubscribed_subscription] [nvarchar](max) NULL,
	[ProductId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Subscription] PRIMARY KEY CLUSTERED 
(
	[SubscriptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SwishPayment]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[SwishPayment](
	[Id] [uniqueidentifier] NOT NULL,
	[SwishId] [varchar](1000) NULL,
	[SwishPaymentId] [varchar](1000) NULL,
	[SwishPaymentStatus] [int] NOT NULL,
	[SwishResponse] [varchar](max) NULL,
	[SwishLocation] [varchar](max) NULL,
	[OrderId] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TablesSyncLog]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TablesSyncLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TableName] [int] NOT NULL,
	[TableKey] [nvarchar](max) NULL,
	[OutletId] [uniqueidentifier] NOT NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_dbo.TablesSyncLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Tax]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tax](
	[Id] [int] NOT NULL,
	[TaxValue] [decimal](18, 2) NOT NULL,
	[AccountingCode] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Tax] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Terminal]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[Terminal](
	[Id] [uniqueidentifier] NOT NULL,
	[OutletId] [uniqueidentifier] NOT NULL,
	[TerminalNo] [int] NOT NULL,
	[TerminalType] [uniqueidentifier] NOT NULL,
	[UniqueIdentification] [nvarchar](max) NULL,
	[HardwareAddress] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[RootCategoryId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
	[CCUData] [varchar](max) NULL,
 CONSTRAINT [PK_dbo.Terminal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TerminalStatusLog]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TerminalStatusLog](
	[Id] [uniqueidentifier] NOT NULL,
	[TerminalId] [uniqueidentifier] NOT NULL,
	[ActivityDate] [datetime] NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[ReportId] [uniqueidentifier] NOT NULL,
	[Status] [int] NOT NULL,
	[Synced] [int] NOT NULL,
 CONSTRAINT [PK_dbo.TerminalStatusLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserClaims]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.UserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserLog]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[LoginTime] [datetime] NULL,
	[LogOutTime] [datetime] NULL,
	[LogDate] [datetime] NULL,
	[IsLogedOut] [int] NOT NULL,
 CONSTRAINT [PK_dbo.UserLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserLogins]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.UserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserOrder]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[UserOrder](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[OrderBy] [uniqueidentifier] NOT NULL,
	[DeviceInformation] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Users]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[PasswordHash] [nvarchar](70) NULL,
	[SecurityStamp] [nvarchar](40) NULL,
	[PhoneNumber] [nvarchar](20) NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[Password] [nvarchar](max) NULL,
	[TrainingMode] [bit] NULL,
	[Active] [bit] NULL,
	[Created] [datetime] NULL,
	[Updated] [datetime] NULL,
	[DallasKey] [nvarchar](max) NULL,
	[Discriminator] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UsersInRoles]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UsersInRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.UsersInRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Warehouse]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Warehouse](
	[WarehouseID] [uniqueidentifier] NOT NULL,
	[Alias] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[Address1] [nvarchar](max) NULL,
	[Address2] [nvarchar](max) NULL,
	[Address3] [nvarchar](max) NULL,
	[Zipcode] [nvarchar](max) NULL,
	[City] [nvarchar](max) NULL,
	[Country] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Warehouse] PRIMARY KEY CLUSTERED 
(
	[WarehouseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WarehouseLocation]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WarehouseLocation](
	[WarehouseLocationID] [uniqueidentifier] NOT NULL,
	[WarehouseID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Path] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.WarehouseLocation] PRIMARY KEY CLUSTERED 
(
	[WarehouseLocationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VoucherTransaction]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VoucherTransaction](
	[Id] [uniqueidentifier] NOT NULL,
	[OrderId] [uniqueidentifier] NOT NULL,
	[TransactionDate] [datetime] NOT NULL,
	[ErsReference] [nvarchar](max) NULL,
	[Canceled] [bit] NOT NULL,
	[Product_Id] [uniqueidentifier] NULL,
 CONSTRAINT [PK_dbo.VoucherTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ZReportSetting]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ZReportSetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReportTag] [nvarchar](max) NULL,
	[Visiblity] [bit] NOT NULL,
	[Updated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.ZReportSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
INSERT [dbo].[Accounting] ([Id], [AcNo], [Name], [IsDeleted], [TAX], [Updated], [SortOrder]) VALUES (1, 3051, N'Försäljning av varor inom Sverige, moms 25%', 0, CAST(25.00 AS Decimal(18, 2)), CAST(N'2016-07-19 15:16:38.000' AS DateTime), 1)
GO
INSERT [dbo].[Accounting] ([Id], [AcNo], [Name], [IsDeleted], [TAX], [Updated], [SortOrder]) VALUES (2, 3052, N'Försäljning av varor inom Sverige, moms 12%', 0, CAST(12.00 AS Decimal(18, 2)), CAST(N'2016-07-19 15:16:24.000' AS DateTime), 2)
GO
INSERT [dbo].[Accounting] ([Id], [AcNo], [Name], [IsDeleted], [TAX], [Updated], [SortOrder]) VALUES (3, 3053, N'Försäljning av varor inom Sverige, moms 6%', 0, CAST(6.00 AS Decimal(18, 2)), CAST(N'2018-09-14 13:54:00.260' AS DateTime), 3)
GO
INSERT [dbo].[Accounting] ([Id], [AcNo], [Name], [IsDeleted], [TAX], [Updated], [SortOrder]) VALUES (4, 3054, N'Försäljning av varor inom Sverige, moms 0%', 0, CAST(0.00 AS Decimal(18, 2)), CAST(N'2016-07-20 00:26:34.000' AS DateTime), 4)
GO
INSERT [dbo].[CashDrawer] ([Id], [Name], [Location], [UserId], [TerminalId], [ConnectionString]) VALUES (N'bcc2ea00-afd2-41c0-afe8-315fe4569904', N'Kassalåda 1', N'Main Terminal', NULL, N'c810666d-9b1e-4ada-b55c-71db03f6e2fb', NULL)
GO
INSERT [dbo].[CashDrawer] ([Id], [Name], [Location], [UserId], [TerminalId], [ConnectionString]) VALUES (N'ed23d3f5-2476-4786-aa4d-84ee17c0912d', N'Kassalåda 1', N'Kassan', NULL, N'c810666d-9b1e-4ada-b55c-71db03f6e2fb', NULL)
GO
INSERT [dbo].[CashDrawer] ([Id], [Name], [Location], [UserId], [TerminalId], [ConnectionString]) VALUES (N'8a1750a6-5ed0-4c73-952c-ccc42dc5e24e', N'Kassalåda 1', N'Kassan', NULL, N'c810666d-9b1e-4ada-b55c-71db03f6e2fb', NULL)
GO
INSERT [dbo].[Category] ([Id], [Name], [Parant], [CategoryLevel], [Type], [Active], [Deleted], [Created], [Updated], [ColorCode], [SortOrder], [IconId], [ReportOrder], [ImageURL], [Description], [WC_ID]) VALUES (1, N'Root Category', 0, 1, 1, 1, 0, CAST(N'2018-07-14 00:00:00.000' AS DateTime), CAST(N'2018-07-14 00:00:00.000' AS DateTime), NULL, 0, 0, 0, NULL, NULL, 0)
GO
INSERT [dbo].[Category] ([Id], [Name], [Parant], [CategoryLevel], [Type], [Active], [Deleted], [Created], [Updated], [ColorCode], [SortOrder], [IconId], [ReportOrder], [ImageURL], [Description], [WC_ID]) VALUES (2, N'Category 1', 1, 2, 1, 1, 0, CAST(N'2018-08-15 00:00:00.000' AS DateTime), CAST(N'2018-12-19 08:38:01.167' AS DateTime), NULL, 1, 0, 1, NULL, NULL, 0)
GO
INSERT [dbo].[Category] ([Id], [Name], [Parant], [CategoryLevel], [Type], [Active], [Deleted], [Created], [Updated], [ColorCode], [SortOrder], [IconId], [ReportOrder], [ImageURL], [Description], [WC_ID]) VALUES (3, N'Category 2', 1, 2, 0, 1, 0, CAST(N'2018-12-19 11:14:29.580' AS DateTime), CAST(N'2019-01-24 14:41:59.913' AS DateTime), N'#994646', 0, 0, 0, NULL, NULL, 0)
GO
INSERT [dbo].[Customer] ([Id], [Name], [OrgNo], [Address1], [Address2], [FloorNo], [PortCode], [CustomerNo], [City], [Phone], [ZipCode], [Reference], [Created], [Updated], [DirectPrint], [Active], [Email], [ExternalId], [PinCode], [LastBalanceUpdated], [DepositAmount], [HasDeposit], [WC_ID]) VALUES (N'c92a52e4-5ba6-4fea-a5c9-d14976c8880f', N'Zahid Hazoor', N'1', N'PWD Rawalpindi', NULL, 1, 1, N'1', N'Rawalpindi', N'03001234567', N'44000', NULL, CAST(N'2018-09-12 12:44:04.377' AS DateTime), CAST(N'2018-09-12 12:44:57.963' AS DateTime), 0, 1, NULL, NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), 0, 0)
GO
INSERT [dbo].[Customer] ([Id], [Name], [OrgNo], [Address1], [Address2], [FloorNo], [PortCode], [CustomerNo], [City], [Phone], [ZipCode], [Reference], [Created], [Updated], [DirectPrint], [Active], [Email], [ExternalId], [PinCode], [LastBalanceUpdated], [DepositAmount], [HasDeposit], [WC_ID]) VALUES (N'918b9888-edfb-4478-96ba-d8c0a0d55897', N'test ', NULL, NULL, NULL, 0, 0, N'0', NULL, NULL, NULL, NULL, CAST(N'2018-09-24 16:14:12.937' AS DateTime), CAST(N'2018-09-24 16:14:20.857' AS DateTime), 0, 1, NULL, NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), 0, 0)
GO
INSERT [dbo].[CustomerBonus] ([Id], [CustomerId], [OrderId], [OutletId], [ChangeValue], [CurrentSum], [CreatedOn]) VALUES (N'61a9528d-c09e-4262-98c4-3ec40fe276b3', N'c92a52e4-5ba6-4fea-a5c9-d14976c8880f', N'7cca64d0-c00a-4ce7-b22e-5e38bf1ede2b', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', CAST(0.00 AS Decimal(18, 2)), CAST(60.00 AS Decimal(18, 2)), CAST(N'2018-09-24 16:14:53.450' AS DateTime))
GO
INSERT [dbo].[CustomerBonus] ([Id], [CustomerId], [OrderId], [OutletId], [ChangeValue], [CurrentSum], [CreatedOn]) VALUES (N'2adca05b-424f-4ea5-b4f4-4f2710c556df', N'c92a52e4-5ba6-4fea-a5c9-d14976c8880f', N'c43f84ab-5b4a-4989-b731-e6afe74d1ef6', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', CAST(0.00 AS Decimal(18, 2)), CAST(90.00 AS Decimal(18, 2)), CAST(N'2018-09-17 16:26:14.520' AS DateTime))
GO
INSERT [dbo].[CustomerBonus] ([Id], [CustomerId], [OrderId], [OutletId], [ChangeValue], [CurrentSum], [CreatedOn]) VALUES (N'c0e2bb63-09c1-469a-b137-500838265c30', N'c92a52e4-5ba6-4fea-a5c9-d14976c8880f', N'6add72cc-96f1-4c5b-899a-c2769e8cbc8e', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', CAST(0.00 AS Decimal(18, 2)), CAST(85.00 AS Decimal(18, 2)), CAST(N'2018-09-24 16:16:13.930' AS DateTime))
GO
INSERT [dbo].[CustomerBonus] ([Id], [CustomerId], [OrderId], [OutletId], [ChangeValue], [CurrentSum], [CreatedOn]) VALUES (N'32b5ef99-2910-4b5e-ba70-876ba2fe411b', N'918b9888-edfb-4478-96ba-d8c0a0d55897', N'e811f17b-8c3a-436a-9760-61066a7f0527', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', CAST(0.00 AS Decimal(18, 2)), CAST(85.00 AS Decimal(18, 2)), CAST(N'2018-09-24 16:14:24.837' AS DateTime))
GO
INSERT [dbo].[CustomerBonus] ([Id], [CustomerId], [OrderId], [OutletId], [ChangeValue], [CurrentSum], [CreatedOn]) VALUES (N'3c7bc093-d9a0-4818-a3df-8e2d9fe1b44e', N'c92a52e4-5ba6-4fea-a5c9-d14976c8880f', N'655a5b27-dcda-4c52-b112-1a32ef2885a8', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', CAST(0.00 AS Decimal(18, 2)), CAST(160.00 AS Decimal(18, 2)), CAST(N'2018-09-12 12:45:33.020' AS DateTime))
GO
INSERT [dbo].[CustomerBonus] ([Id], [CustomerId], [OrderId], [OutletId], [ChangeValue], [CurrentSum], [CreatedOn]) VALUES (N'5788cb93-8601-4de8-a93a-a9a49ee22470', N'c92a52e4-5ba6-4fea-a5c9-d14976c8880f', N'c627ab1c-82f1-490f-8944-189ad2b0ef55', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', CAST(0.00 AS Decimal(18, 2)), CAST(100.00 AS Decimal(18, 2)), CAST(N'2018-09-24 16:21:58.557' AS DateTime))
GO
INSERT [dbo].[Employee] ([Id], [FirstName], [LastName], [SSNO], [Email], [Created], [Updated]) VALUES (N'12d2c639-38b7-4a74-ac56-a33dab9f885c', N'Raja', N'Ali', N'123456789101', N' ', CAST(N'2018-09-24 14:32:10.313' AS DateTime), CAST(N'2018-09-24 14:32:31.260' AS DateTime))
GO
INSERT [dbo].[Floor] ([Id], [Name]) VALUES (1, N'Floor 1')
GO
INSERT [dbo].[Floor] ([Id], [Name]) VALUES (2, N'Floor 2')
GO
INSERT [dbo].[Floor] ([Id], [Name]) VALUES (3, N'Floor 3')
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (1, N'1', 1, 13, 8, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (2, N'2', 1, 11, 143, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (3, N'3', 1, 10, 306, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (4, N'4', 1, 7, 437, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (5, N'5', 1, 81, 443, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (6, N'6', 1, 77, 11, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (7, N'7', 1, 313, 128, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (8, N'8', 1, 316, 253, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (9, N'9', 1, 311, 10, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (10, N'10', 1, 166, 11, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (11, N'11', 1, 316, 440, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (12, N'12', 1, 159, 198, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (13, N'13', 1, 160, 440, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (14, N'14', 1, 244, 6, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (15, N'15', 1, 235, 439, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (16, N'1', 2, 5, 10, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (17, N'2', 2, 5, 110, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (18, N'3', 2, 5, 210, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (19, N'4', 2, 5, 310, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (20, N'5', 2, 5, 410, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (21, N'6', 2, 65, 10, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (22, N'7', 2, 65, 110, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (23, N'8', 2, 65, 210, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (24, N'9', 2, 65, 310, 51, 109, 0, 0, N'squarTable.png', CAST(N'2018-06-22 16:31:24.963' AS DateTime))
GO
INSERT [dbo].[FoodTable] ([Id], [Name], [FloorId], [PositionX], [PositionY], [Height], [Width], [Chairs], [Status], [ImageUrl], [Updated]) VALUES (25, N'10', 2, 125, 410, 51, 110, 0, 0, N'squarTable.png', CAST(N'2018-06-22 17:24:29.207' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[ItemStock] ON 

GO
INSERT [dbo].[ItemStock] ([Id], [ItemId], [Quantity], [BatchNo], [ExpiryDate], [Updated]) VALUES (1, N'b76533cc-0457-4639-b0c5-00c09a8838eb', CAST(10.00 AS Decimal(18, 2)), N'BCH123', CAST(N'2018-09-20 00:00:00.000' AS DateTime), CAST(N'2018-09-18 16:31:18.167' AS DateTime))
GO
INSERT [dbo].[ItemStock] ([Id], [ItemId], [Quantity], [BatchNo], [ExpiryDate], [Updated]) VALUES (2, N'b76533cc-0457-4639-b0c5-00c09a8838eb', CAST(200.00 AS Decimal(18, 2)), N'bat-200', CAST(N'2018-09-22 00:00:00.000' AS DateTime), CAST(N'2018-09-18 17:42:52.900' AS DateTime))
GO
INSERT [dbo].[ItemStock] ([Id], [ItemId], [Quantity], [BatchNo], [ExpiryDate], [Updated]) VALUES (3, N'2c9b1ac1-66b7-4911-8e9b-035538b0c80e', CAST(300.00 AS Decimal(18, 2)), N'bat-300', CAST(N'2018-09-29 00:00:00.000' AS DateTime), CAST(N'2018-09-18 17:44:27.707' AS DateTime))
GO
INSERT [dbo].[ItemStock] ([Id], [ItemId], [Quantity], [BatchNo], [ExpiryDate], [Updated]) VALUES (1002, N'959c0f54-bf92-4573-b646-e26ee1b39b83', CAST(20.00 AS Decimal(18, 2)), N'7up-125', CAST(N'2018-09-30 00:00:00.000' AS DateTime), CAST(N'2018-09-19 14:46:08.090' AS DateTime))
GO
INSERT [dbo].[ItemStock] ([Id], [ItemId], [Quantity], [BatchNo], [ExpiryDate], [Updated]) VALUES (1003, N'959c0f54-bf92-4573-b646-e26ee1b39b83', CAST(2.00 AS Decimal(18, 2)), N'bat-256', CAST(N'2018-09-28 00:00:00.000' AS DateTime), CAST(N'2018-09-19 14:53:40.867' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[ItemStock] OFF
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (0, N'Order', N'NewOrderEntry', N'New order transaction has been started with order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (1, N'Order', N'OrderTypeNew', N' has been marked as New.', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (2, N'Order', N'OrderTypeTakeAway', N' has been marked as TakeAway.', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (3, N'Order', N'OrderTypeReturnTakeAway', N' has been marked as return of TakeAway.', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (4, N'Order', N'OrderTypeReturn', N' has been marked as Return.', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (5, N'Order', N'OrderCancelled', N' has been marked as Cancelled', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (6, N'Order', N'OrderComment', N'Comments has been added on Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (7, N'Order', N'SwitchedToItemView', N'Switched To Item View', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (8, N'Order', N'SwitchedToTableView', N'Switched To TableView', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (9, N'Order', N'DiscountOnOrder', N'Discount On Order', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (10, N'Order', N'TableViewIsClosed', N'Table View Is Closed', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (11, N'Order', N'ItemAdded', N' - is added in cart for order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (12, N'Order', N'ReturnItemAdded', N'Return Item is added in cart for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (13, N'Order', N'ItemDeleted', N' - is deleted for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (14, N'Order', N'ItemAddedWithCustomPrice', N' - is added in cart with custom price for order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (15, N'Order', N'ItemDiscountAdded', N'- Discount is added in cart  for order#:', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (16, N'Order', N'ItemPriceChanged', N'Price was change for item', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (17, N'Order', N'ItemPriceReverted', N'Price was change for item', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (18, N'Order', N'ItemMoved', N'Item Moved For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (20, N'Order', N'OrderTableSelected', N' is selected for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (21, N'CheckOut', N'OpenOrderSelected', N'Open Order Selected Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (22, N'CheckOut', N'DirectCashPaymentStarted', N'Direct Cash Payment Started For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (23, N'CheckOut', N'DirectCardPaymentStarted', N'Direct Card Payment Started For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (30, N'Order', N'PaymentScreenNavigation', N' has been navigated to payment screen.', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (31, N'Order', N'PaymentScreenCancelled', N' has been cancelled on payment screen.', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (32, N'CheckOut', N'PaymentTerminalWindowOpen', N'Payment Terminal WindowOpen For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (33, N'CheckOut', N'PaymentTerminalWindowCancel', N'Payment Terminal Window Cancel For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (34, N'CheckOut', N'PaymentTerminalWindowClosed', N'Payment Terminal WindowClosed For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (35, N'CheckOut', N'PaymentDeviceTotal', N'Device Total For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (197, N'Invoice', N'ReceiptGenerating', N'Generating Receipt For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (198, N'Invoice', N'ReceiptGenerated', N'Receipt Generated Successfully For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (199, N'Invoice', N'ReceiptFail', N'Receipt Failed For Order#', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (200, N'Invoice', N'ReceiptPrinted', N'Receipt has been printed for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (201, N'Invoice', N'ReceiptCopyPrinted', N'Receipt copy has been printed for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (202, N'Invoice', N'ReceiptPrintedForReturnOrder', N'Receipt has been printed for Return Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (203, N'Invoice', N'ReceiptPrintedViaTrainingMode', N'Receipt has been printed via training mode for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (204, N'Invoice', N'ReceiptCopyPrintedViaTrainingMode', N'Receipt copy has been printed via training mode for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (205, N'Invoice', N'ReceiptPrintedForReturnOrderViaTrainingMode', N'Receipt has been printed via training mode for Return Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (210, N'Invoice', N'ReceiptKitchen', N'Bong is sent to kitchen for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (220, N'Invoice', N'PrintPerforma', N'Proforma is printed for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (230, N'Invoice', N'ReceiptViewed', N'Receipt has been viewed for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (231, N'Invoice', N'ReceiptViewedViaTrainingMode', N'Receipt has been viewed via training mode for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (300, N'Payment', N'OrderCashPayment', N'Payment is successfully processed via cash for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (301, N'Payment', N'OrderCreditcardPayment', N'Payment is successfully processed via credit/debit card for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (302, N'Payment', N'OrderReturnCashPayment', N'Payment return is successfully processed via cash for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (303, N'Payment', N'OrderSwishPayment', N'Payment is successfully processed via swish for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (304, N'Payment', N'OrderAccountPayment', N'Payment is successfully processed via account for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (305, N'Payment', N'OrderCouponPayment', N'Payment is successfully processed via coupon for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (306, N'Payment', N'OrderMobileTerminalPayment', N'Payment is successfully processed via Mobile terminal for Order#: ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (400, N'Report', N'ReportXViewed', N'X report is viewed by ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (401, N'Report', N'ReportXPrinted', N'X report is printed by ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (402, N'Report', N'ReportZPrinted', N'Z report is printed by ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (403, N'Report', N'ReportZViewed', N'Z report is viewed by ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (500, N'Authentication', N'LoggedIn', N' is LoggedIn ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (501, N'Authentication', N'LoggedOut', N'User is LoggedOut ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (502, N'Authentication', N'LoggedInFailure', N'Login failure attemt has been made', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (600, N'CashDrawer', N'OpenCashDrawer', N'Cash drawer is opened by ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (700, N'Terminal', N'TerminalClosed', N'Terminal is closed by ', NULL)
GO
INSERT [dbo].[JournalAction] ([Id], [Type], [ActionCode], [Description], [Description2]) VALUES (701, N'Terminal', N'TerminalOpened', N'Terminal is opened by ', NULL)
GO
INSERT [dbo].[OutletUser] ([Id], [OutletId], [Email], [UserCode], [UserName], [Password], [TrainingMode], [Active], [DallasKey], [Updated]) VALUES (N'A9A50982-559D-4C61-800C-D1E306BD32A5', N'fa8aa275-2518-434a-a893-8776052fc46b', NULL, N'111', N'Salong Rivoli', N'7C4A8D09CA3762AF61E59520943DC26494F8941B', 0, 1, NULL, CAST(N'2019-02-07 12:59:07.117' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (0, N'Free Coupon', N'Gratis kupong', 1909, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (1, N'Paid by Cash', N'Betales kontant', 1910, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (2, N'On Credit', N'på Credit', 1911, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (3, N'Paid By Gift', N'Betalt av kupong', 1912, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (4, N'Paid By Credit Card', N'Betalt med kredittkort', 1930, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (5, N'Paid By Debit Card', N'Betalt av debetkort', 1931, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (6, N'Paid By Cheque', N'Betalt med sjekk', 1932, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (7, N'Cash Back', N'Tillbaka', 1933, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (8, N'Returned', N'Returnerad', 1914, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (9, N'Mobile Card', N'Mobil korterminal', 1910, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (10, N'Swish', N'Swish', 1908, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (11, N'Elve Card', N'Elevkort', 1907, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (12, N'Credit Note', N'Tillgodokvitto', 1906, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (13, N'Beam', N'beam', 1905, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (14, N'Paid By AMEX  Card', N'Betalt med AMEX  kort', 1514, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (15, N'Online Cash', N'Online Kontant', 1515, CAST(N'2018-08-17 17:20:06.747' AS DateTime))
GO
INSERT [dbo].[PaymentType] ([Id], [Name], [SwedishName], [AccountingCode], [Updated]) VALUES (17, N'Swish Online', N'Swish Online', 1908, CAST(N'2023-01-13 10:35:00.000' AS DateTime))
GO
INSERT [dbo].[Printer] ([Id], [LocationName], [PrinterName], [TerminalId], [IPAddress], [Updated]) VALUES (1, N'Kvittoskrivare', N'THERMAL Receipt Printer', N'6ff4c405-3bc9-4b61-9676-1f287557777a', NULL, CAST(N'2016-12-06 09:50:35.000' AS DateTime))
GO
INSERT [dbo].[Printer] ([Id], [LocationName], [PrinterName], [TerminalId], [IPAddress], [Updated]) VALUES (2, N'Kök', N'THERMAL Receipt Printer', N'6ff4c405-3bc9-4b61-9676-1f287557777a', NULL, CAST(N'2016-12-06 09:50:40.000' AS DateTime))
GO
INSERT [dbo].[Printer] ([Id], [LocationName], [PrinterName], [TerminalId], [IPAddress], [Updated]) VALUES (3, N'Invoice', N'Canon MF8500C Series UFRII LT', NULL, NULL, CAST(N'2016-05-11 09:53:33.000' AS DateTime))
GO
INSERT [dbo].[Printer] ([Id], [LocationName], [PrinterName], [TerminalId], [IPAddress], [Updated]) VALUES (4, N'Global', N'A4 Size', NULL, NULL, CAST(N'2016-05-13 16:18:07.000' AS DateTime))
GO
INSERT [dbo].[Printer] ([Id], [LocationName], [PrinterName], [TerminalId], [IPAddress], [Updated]) VALUES (5, N'Printer 2', N'THERMAL Receipt Printer#:2', NULL, NULL, CAST(N'2018-09-06 14:40:31.643' AS DateTime))
GO
INSERT [dbo].[Printer] ([Id], [LocationName], [PrinterName], [TerminalId], [IPAddress], [Updated]) VALUES (6, N'Printer 1', N'Printer 1', NULL, NULL, CAST(N'2018-09-06 14:40:53.410' AS DateTime))
GO
INSERT [dbo].[ProductGroup] ([Id], [ItemId], [GroupId], [Price], [Updated]) VALUES (10, N'b76533cc-0457-4639-b0c5-00c09a8838eb', N'3666e3aa-f079-4d11-8aa5-5ea2e0cf3ae9', CAST(10.00 AS Decimal(18, 2)), CAST(N'2018-09-05 18:28:24.447' AS DateTime))
GO
INSERT [dbo].[ProductGroup] ([Id], [ItemId], [GroupId], [Price], [Updated]) VALUES (11, N'd7b7d141-5914-4495-aeec-0312bb75cc54', N'3666e3aa-f079-4d11-8aa5-5ea2e0cf3ae9', CAST(15.00 AS Decimal(18, 2)), CAST(N'2018-09-05 18:09:02.580' AS DateTime))
GO
INSERT [dbo].[ProductGroup] ([Id], [ItemId], [GroupId], [Price], [Updated]) VALUES (12, N'2c9b1ac1-66b7-4911-8e9b-035538b0c80e', N'3666e3aa-f079-4d11-8aa5-5ea2e0cf3ae9', CAST(8.00 AS Decimal(18, 2)), CAST(N'2018-09-05 18:09:13.637' AS DateTime))
GO
INSERT [dbo].[Roles] ([Id], [Name]) VALUES (N'23E942F2-7D47-468E-A38C-4FC7CE45A93C', N'Admin')
GO
INSERT [dbo].[Roles] ([Id], [Name]) VALUES (N'77E34C7E-A204-42E4-89D7-C885338A6CE3', N'Content Manager')
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (0, N'LIVE DATA SYNC URL', 1, 0, N'http://api.stageshop.app01.possum.com/', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (1, N'Sync API USER', 1, 1, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (2, N'Sync API PASSWORD', 1, 2, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (3, N'SHOW EXTERNAL ORDER', 1, 3, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (4, N'EXTERNAL ORDER API URL', 1, 4, N'www.api.com', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (5, N'EXTERNAL ORDER API USER', 1, 5, N'abc', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (6, N'EXTERNAL API PASSWORD', 1, 6, N'abc123', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (7, N'EXTERNAL ORDER MQTT URL', 1, 7, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (8, N'RESTAURANT GROUP ID', 1, 8, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (9, N'AskForPrintInvoice', 1, 9, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (10, N'ENABLE MQTT', 1, 10, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (11, N'M2MQTT URL', 1, 11, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (12, N'LAST EXECUTION DATE', 1, 0, N'2020-06-25 17:25:57', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2020-06-25 17:25:57.763' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (15, N'CONTROL UNIT TYPE', 1, 15, N'2', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (16, N'CONTROL UNIT CONECTION STRING', 1, 16, N'COM6:57600,DTR', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (17, N'CASHDRAWER TYPE', 2, 17, N'4', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (18, N'CASHDRAWER PORT', 2, 18, N'1154', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (19, N'ENABLE CASHGUARD', 2, 19, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (20, N'CASHGUARD PORT', 2, 20, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (21, N'PAYMENT DEVICE TYPE', 2, 21, N'3', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (22, N'PAYENT DEVICE CONNECTION', 2, 22, N'connect2t://192.168.8.78:1337', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (23, N'SCALE TYPE', 2, 23, N'DUMMY', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (24, N'SCALE PORT', 2, 24, N'COM1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (25, N'POS ID FOR CU', 2, 25, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (26, N'ENABLE CLEAN CASH LOG', 2, 26, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (27, N'Receipt Comment', 2, 27, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (31, N'CASHDRAWER LANGUAGE', 3, 31, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (32, N'CASHDRAWER PORT', 3, 32, N'sv-SE', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (33, N'CASHDRAWER PORT', 3, 33, N'kr', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (34, N'SALE TYPE', 3, 34, N'Retail', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (35, N'SHOW CATEGORY LINE', 3, 35, N'3', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (36, N'SHOW ITEMS LINES', 3, 36, N'4', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (37, N'TABLE VIEW', 3, 37, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (38, N'SHOW TAKEAWAY BUTTON', 3, 38, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (39, N'SHOW DIRECT CASH BUTTON', 3, 39, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (40, N'SHOW DIRECT CARD BUTTON', 3, 40, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (41, N'SHOW DIRECT SWISH BUTTON', 3, 41, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (42, N'SHOW DIRECT STUDENT CARD BUTTON', 3, 42, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (43, N'SHOW CUSTOMER VIEW', 3, 43, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (44, N'ADD CUSTOMER INFO TO ORDER', 3, 44, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (45, N'SHOW CREDIT NOTE BUTTON', 3, 45, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (46, N'SHOW BEAM PAYMENT BUTTON', 3, 46, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (47, N'EMPLOYEE LOG', 3, 47, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (48, N'ENABLE ONLY DIGIT IN TEXTBOX', 3, 48, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (49, N'SHOW PRICE ON ITEM BUTTON', 3, 49, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (50, N'HIDE PAYMENT BUTTON', 3, 50, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (51, N'ENABLE PRINT LOGO', 4, 51, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (52, N'SHOW ALERT BEFORE BONG', 4, 52, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (53, N'SET BONG FONT NORMAL', 4, 53, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (54, N'PRINT BONG', 4, 54, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (55, N'PRINT BONG BY PRODUCT', 4, 55, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (56, N'SHOW TABLE ON BONG', 4, 56, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (57, N'ENABLE DAILY BONG COUNTER', 4, 57, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (58, N'ENABLE BONG COUNTER', 4, 58, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (59, N'ENABLE MULTI KICHEN', 4, 59, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (60, N'SHOW ORDER NO ON BONG', 4, 60, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (65, N'TERMINAL MODE CLIENT OR SERVER', 0, 65, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (66, N'ENABLE DALASS KEY LOGIN', 0, 66, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (67, N'Night Mode Start Hour', 0, 67, N'8', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (68, N'Night Mode End hour', 0, 68, N'14', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (69, N'ENABLE Dual Price Mode', 0, 69, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (70, N'SET RUNNING MODE', 0, 70, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (71, N'ENABLE PAYMENT DEVICE LOGING', 0, 71, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (72, N'ENABLE PRICE POLICy', 0, 72, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (73, N'ENABLE EXTERNALE NEWTWORKING', 0, 73, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (74, N'Set Customer ID', 0, 74, N'00000000-0000-0000-0000-000000000000', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (75, N'CUSTOMER VIEW SLIDE URL', 0, 75, N'C:\POSSUM\pos\index.html', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (76, N'INVOICE DUE DAYS', 0, 76, N'10', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (77, N'ENABLE ASK DISCOUNT CODE', 0, 77, N'', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (78, N'ENABLE CHECKOUT LOG', 0, 78, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (79, N'ENABLE ORDER ENTRY', 0, 79, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (80, N'SET Account Number', 0, 80, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (81, N'Payment Receiver Name', 0, 81, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (82, N'Invoice Reference', 0, 82, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (83, N'SaleType', 1, 83, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (84, N'Category Bold', 0, 84, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (85, N'Product Bold', 0, 85, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (86, N'Defaul Bong', 0, 86, N'1', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (87, N'ClientSettingsProvider', 0, 87, N'http://api.possumsystem.com/v11/
', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (88, N'TerminalId', 0, 88, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (89, N'ElveCard', 0, 89, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (90, N'DisableCreditCard', 0, 90, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (91, N'TipStatus', 3, 91, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (92, N'DisableCashButton', 0, 92, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-13 05:11:47.457' AS DateTime), CAST(N'2019-12-13 05:11:47.457' AS DateTime))
GO
INSERT [dbo].[Setting] ([Id], [Description], [Type], [Code], [Value], [TerminalId], [OutletId], [Sort], [Created], [Updated]) VALUES (93, N'Deposit', 0, 93, N'0', N'77596f15-2d46-453e-a236-d734a3a9b375', N'a7d84bc0-435c-4975-9749-935a226210b1', 0, CAST(N'2019-12-17 19:12:36.023' AS DateTime), CAST(N'2019-12-17 19:11:56.013' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[TablesSyncLog] ON 

GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (1, 40, N'cd5b8304-cc4e-4981-b86b-23b3910a313b', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1764d338-bdfc-45a3-80ab-8ec47d3294a6')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (2, 40, N'067d164f-85c0-48d9-8127-29722da6a213', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1764d338-bdfc-45a3-80ab-8ec47d3294a6')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (3, 40, N'1151fa8a-f7e1-4293-ac59-f2b0a227e8c9', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'aa15c188-3bb9-4868-be4a-470231350894')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (4, 40, N'1151fa8a-f7e1-4293-ac59-f2b0a227e8c9', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1208c019-419a-4b9f-9e87-e3c1c583422e')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (5, 4, N'2', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'aa15c188-3bb9-4868-be4a-470231350894')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (6, 4, N'2', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1208c019-419a-4b9f-9e87-e3c1c583422e')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (7, 4, N'3', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'aa15c188-3bb9-4868-be4a-470231350894')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (8, 4, N'3', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1208c019-419a-4b9f-9e87-e3c1c583422e')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (9, 40, N'3666e3aa-f079-4d11-8aa5-5ea2e0cf3ae9', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'aa15c188-3bb9-4868-be4a-470231350894')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (10, 40, N'3666e3aa-f079-4d11-8aa5-5ea2e0cf3ae9', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1208c019-419a-4b9f-9e87-e3c1c583422e')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (11, 40, N'862726ca-5dff-4fa0-9d50-89deacb5e2fe', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'aa15c188-3bb9-4868-be4a-470231350894')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (12, 40, N'862726ca-5dff-4fa0-9d50-89deacb5e2fe', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1208c019-419a-4b9f-9e87-e3c1c583422e')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (13, 40, N'a783d10c-d15b-40ca-b85f-a23e3f6d7edf', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'aa15c188-3bb9-4868-be4a-470231350894')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (14, 40, N'a783d10c-d15b-40ca-b85f-a23e3f6d7edf', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1208c019-419a-4b9f-9e87-e3c1c583422e')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (15, 40, N'32a9bf24-eb90-4f04-9788-dedbbc66b439', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'aa15c188-3bb9-4868-be4a-470231350894')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (16, 40, N'32a9bf24-eb90-4f04-9788-dedbbc66b439', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1208c019-419a-4b9f-9e87-e3c1c583422e')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (17, 40, N'ce7f7778-725a-40c1-8102-3f120130c652', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'aa15c188-3bb9-4868-be4a-470231350894')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (18, 40, N'ce7f7778-725a-40c1-8102-3f120130c652', N'2b35687a-c363-4a40-95f1-d5a8e859c7ba', N'1208c019-419a-4b9f-9e87-e3c1c583422e')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (19, 40, N'3666e3aa-f079-4d11-8aa5-5ea2e0cf3ae9', N'2274f511-8d9d-4341-83f7-454b5921428a', N'e0a1a175-3b01-4c03-b568-c93dac7b915c')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (20, 4, N'2', N'2274f511-8d9d-4341-83f7-454b5921428a', N'e0a1a175-3b01-4c03-b568-c93dac7b915c')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (21, 4, N'3', N'2274f511-8d9d-4341-83f7-454b5921428a', N'e0a1a175-3b01-4c03-b568-c93dac7b915c')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (22, 40, N'd3a8ce92-5314-4c76-930b-e127f6b4b70f', N'2274f511-8d9d-4341-83f7-454b5921428a', N'e0a1a175-3b01-4c03-b568-c93dac7b915c')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (23, 4, N'4', N'2274f511-8d9d-4341-83f7-454b5921428a', N'e0a1a175-3b01-4c03-b568-c93dac7b915c')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (24, 40, N'ce7f7778-725a-40c1-8102-3f120130c652', N'2274f511-8d9d-4341-83f7-454b5921428a', N'e0a1a175-3b01-4c03-b568-c93dac7b915c')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (25, 40, N'1151fa8a-f7e1-4293-ac59-f2b0a227e8c9', N'2274f511-8d9d-4341-83f7-454b5921428a', N'e0a1a175-3b01-4c03-b568-c93dac7b915c')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (26, 40, N'c50b0aa4-b5c9-4af6-ac0c-614778678b44', N'2274f511-8d9d-4341-83f7-454b5921428a', N'e0a1a175-3b01-4c03-b568-c93dac7b915c')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (27, 40, N'1151fa8a-f7e1-4293-ac59-f2b0a227e8c9', N'fa8aa275-2518-434a-a893-8776052fc46b', N'c810666d-9b1e-4ada-b55c-71db03f6e2fb')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (28, 40, N'ce7f7778-725a-40c1-8102-3f120130c652', N'fa8aa275-2518-434a-a893-8776052fc46b', N'c810666d-9b1e-4ada-b55c-71db03f6e2fb')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (29, 40, N'c50b0aa4-b5c9-4af6-ac0c-614778678b44', N'fa8aa275-2518-434a-a893-8776052fc46b', N'c810666d-9b1e-4ada-b55c-71db03f6e2fb')
GO
INSERT [dbo].[TablesSyncLog] ([Id], [TableName], [TableKey], [OutletId], [TerminalId]) VALUES (30, 4, N'3', N'fa8aa275-2518-434a-a893-8776052fc46b', N'c810666d-9b1e-4ada-b55c-71db03f6e2fb')
GO
SET IDENTITY_INSERT [dbo].[TablesSyncLog] OFF
GO
INSERT [dbo].[Tax] ([Id], [TaxValue], [AccountingCode]) VALUES (1, CAST(0.00 AS Decimal(18, 2)), 2641)
GO
INSERT [dbo].[Tax] ([Id], [TaxValue], [AccountingCode]) VALUES (2, CAST(6.00 AS Decimal(18, 2)), 2631)
GO
INSERT [dbo].[Tax] ([Id], [TaxValue], [AccountingCode]) VALUES (3, CAST(12.00 AS Decimal(18, 2)), 2621)
GO
INSERT [dbo].[Tax] ([Id], [TaxValue], [AccountingCode]) VALUES (4, CAST(25.00 AS Decimal(18, 2)), 2611)
GO
INSERT [dbo].[Users] ([Id], [Email], [PasswordHash], [SecurityStamp], [PhoneNumber], [UserName], [Password], [TrainingMode], [Active], [Created], [Updated], [DallasKey], [Discriminator]) VALUES (N'6213df28-97cc-4ec5-888e-764adc8b567b', N'admin@possum.com', N'AOXQMvmYKVwqcv6G3YtLAfTkax2n9atBxTHKTiIk2g21oPS+5GXqJA68bJF+crTcVA==', N'bf6c0a87-cb82-4ce7-92d1-1732c42fa23f', N'03435478889', N'admin', N'admin@123', 0, 1, CAST(N'2018-08-12 14:06:54.167' AS DateTime), NULL, NULL, N'ApplicationUser')
GO
INSERT [dbo].[UsersInRoles] ([UserId], [RoleId], [IdentityUser_Id]) VALUES (N'6213df28-97cc-4ec5-888e-764adc8b567b', N'23E942F2-7D47-468E-A38C-4FC7CE45A93C', NULL)
GO
SET IDENTITY_INSERT [dbo].[ZReportSetting] ON 

GO
INSERT [dbo].[ZReportSetting] ([Id], [ReportTag], [Visiblity], [Updated]) VALUES (1, N'Category_Sale', 1, CAST(N'2018-09-06 14:41:46.300' AS DateTime))
GO
INSERT [dbo].[ZReportSetting] ([Id], [ReportTag], [Visiblity], [Updated]) VALUES (2, N'Accounting_Sale', 1, CAST(N'2018-09-06 14:41:46.300' AS DateTime))
GO
INSERT [dbo].[ZReportSetting] ([Id], [ReportTag], [Visiblity], [Updated]) VALUES (3, N'Item_Correction', 1, CAST(N'2018-09-06 14:41:46.300' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[ZReportSetting] OFF
GO
ALTER TABLE [dbo].[Campaign] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Campaign] ADD  DEFAULT ((0)) FOR [OnceOnly]
GO
ALTER TABLE [dbo].[Campaign] ADD  DEFAULT ((0)) FOR [LimitDiscountPercentage]
GO
ALTER TABLE [dbo].[Campaign] ADD  DEFAULT ((0)) FOR [DiscountType]
GO
ALTER TABLE [dbo].[CategoryCampaign] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[DepositHistory] ADD  DEFAULT ((0)) FOR [DepositType]
GO
ALTER TABLE [dbo].[DepositHistory] ADD  DEFAULT ((0)) FOR [OldBalance]
GO
ALTER TABLE [dbo].[DepositHistory] ADD  DEFAULT ((0)) FOR [NewBalance]
GO
ALTER TABLE [dbo].[OrderDetail] ADD  DEFAULT ((0)) FOR [IsInventoryAdjusted]
GO
ALTER TABLE [dbo].[OrderDetail] ADD  DEFAULT ((0)) FOR [WC_ID]
GO
ALTER TABLE [dbo].[OrderMaster] ADD  DEFAULT ((0)) FOR [IsVismaInvoiceGenerated]
GO
ALTER TABLE [dbo].[OrderMaster] ADD  DEFAULT ((0)) FOR [IsInInvoice]
GO
ALTER TABLE [dbo].[OrderMaster] ADD  DEFAULT ((0)) FOR [OrderSource]
GO
ALTER TABLE [dbo].[OrderMaster] ADD  DEFAULT ((0)) FOR [WC_ID]
GO
ALTER TABLE [dbo].[OrderMaster] ADD  DEFAULT ((0)) FOR [DiscountTotal]
GO
ALTER TABLE [dbo].[OrderMaster] ADD  DEFAULT ((0)) FOR [DiscountTax]
GO
ALTER TABLE [dbo].[OrderMaster] ADD  DEFAULT ('FALSE') FOR [IsOnlineOrder]
GO
ALTER TABLE [dbo].[Product] ADD  DEFAULT ((0)) FOR [ReorderLevelQuantity]
GO
ALTER TABLE [dbo].[Product] ADD  DEFAULT ((0)) FOR [StockQuantity]
GO
ALTER TABLE [dbo].[Product] ADD  DEFAULT ((0)) FOR [IsPantEnabled]
GO
ALTER TABLE [dbo].[Product] ADD  DEFAULT ((0)) FOR [Weight]
GO
ALTER TABLE [dbo].[Product] ADD  DEFAULT ((0)) FOR [WC_ID]
GO
ALTER TABLE [dbo].[ProductCampaign] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[SwishPayment] ADD  DEFAULT ((0)) FOR [SwishPaymentStatus]
GO
ALTER TABLE [dbo].[CashDrawerLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CashDrawerLog_dbo.CashDrawer_CashDrawerId] FOREIGN KEY([CashDrawerId])
REFERENCES [dbo].[CashDrawer] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CashDrawerLog] CHECK CONSTRAINT [FK_dbo.CashDrawerLog_dbo.CashDrawer_CashDrawerId]
GO
ALTER TABLE [dbo].[Customer_CustomField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Customer_CustomField_dbo.Customer_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Customer_CustomField] CHECK CONSTRAINT [FK_dbo.Customer_CustomField_dbo.Customer_CustomerId]
GO
ALTER TABLE [dbo].[Customer_CustomField]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Customer_CustomField_dbo.CustomerCustomField_FieldId] FOREIGN KEY([FieldId])
REFERENCES [dbo].[CustomerCustomField] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Customer_CustomField] CHECK CONSTRAINT [FK_dbo.Customer_CustomField_dbo.CustomerCustomField_FieldId]
GO
ALTER TABLE [dbo].[EmployeeLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.EmployeeLog_dbo.Employee_EmployeeId] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EmployeeLog] CHECK CONSTRAINT [FK_dbo.EmployeeLog_dbo.Employee_EmployeeId]
GO
ALTER TABLE [dbo].[FoodTable]  WITH CHECK ADD  CONSTRAINT [FK_dbo.FoodTable_dbo.Floor_FloorId] FOREIGN KEY([FloorId])
REFERENCES [dbo].[Floor] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FoodTable] CHECK CONSTRAINT [FK_dbo.FoodTable_dbo.Floor_FloorId]
GO
ALTER TABLE [dbo].[OrderDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.OrderDetail_dbo.OrderMaster_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[OrderMaster] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderDetail] CHECK CONSTRAINT [FK_dbo.OrderDetail_dbo.OrderMaster_OrderId]
GO
ALTER TABLE [dbo].[OrderDetail]  WITH CHECK ADD  CONSTRAINT [FK_dbo.OrderDetail_dbo.Product_ItemId] FOREIGN KEY([ItemId])
REFERENCES [dbo].[Product] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderDetail] CHECK CONSTRAINT [FK_dbo.OrderDetail_dbo.Product_ItemId]
GO
ALTER TABLE [dbo].[OrderMaster]  WITH CHECK ADD  CONSTRAINT [FK_dbo.OrderMaster_dbo.Outlet_OutletId] FOREIGN KEY([OutletId])
REFERENCES [dbo].[Outlet] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderMaster] CHECK CONSTRAINT [FK_dbo.OrderMaster_dbo.Outlet_OutletId]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Payment_dbo.OrderMaster_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[OrderMaster] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK_dbo.Payment_dbo.OrderMaster_OrderId]
GO
ALTER TABLE [dbo].[ProductStockHistory]  WITH CHECK ADD FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([Id])
GO
ALTER TABLE [dbo].[ProductStockHistory]  WITH CHECK ADD FOREIGN KEY([StockHistoryGroupId])
REFERENCES [dbo].[StockHistoryGroup] ([StockHistoryGroupId])
GO
ALTER TABLE [dbo].[Receipt]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Receipt_dbo.OrderMaster_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[OrderMaster] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Receipt] CHECK CONSTRAINT [FK_dbo.Receipt_dbo.OrderMaster_OrderId]
GO
ALTER TABLE [dbo].[Report]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Report_dbo.Terminal_TerminalId] FOREIGN KEY([TerminalId])
REFERENCES [dbo].[Terminal] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Report] CHECK CONSTRAINT [FK_dbo.Report_dbo.Terminal_TerminalId]
GO
ALTER TABLE [dbo].[SwishPayment]  WITH CHECK ADD FOREIGN KEY([OrderId])
REFERENCES [dbo].[OrderMaster] ([Id])
GO
ALTER TABLE [dbo].[Terminal]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Terminal_dbo.Category_RootCategoryId] FOREIGN KEY([RootCategoryId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Terminal] CHECK CONSTRAINT [FK_dbo.Terminal_dbo.Category_RootCategoryId]
GO
ALTER TABLE [dbo].[Terminal]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Terminal_dbo.Outlet_OutletId] FOREIGN KEY([OutletId])
REFERENCES [dbo].[Outlet] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Terminal] CHECK CONSTRAINT [FK_dbo.Terminal_dbo.Outlet_OutletId]
GO
ALTER TABLE [dbo].[TerminalStatusLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TerminalStatusLog_dbo.Terminal_TerminalId] FOREIGN KEY([TerminalId])
REFERENCES [dbo].[Terminal] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TerminalStatusLog] CHECK CONSTRAINT [FK_dbo.TerminalStatusLog_dbo.Terminal_TerminalId]
GO
ALTER TABLE [dbo].[UserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserClaims_dbo.Users_IdentityUser_Id] FOREIGN KEY([IdentityUser_Id])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserClaims] CHECK CONSTRAINT [FK_dbo.UserClaims_dbo.Users_IdentityUser_Id]
GO
ALTER TABLE [dbo].[UserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserLogins_dbo.Users_IdentityUser_Id] FOREIGN KEY([IdentityUser_Id])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserLogins] CHECK CONSTRAINT [FK_dbo.UserLogins_dbo.Users_IdentityUser_Id]
GO
ALTER TABLE [dbo].[UserOrder]  WITH CHECK ADD FOREIGN KEY([OrderId])
REFERENCES [dbo].[OrderMaster] ([Id])
GO
ALTER TABLE [dbo].[UsersInRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UsersInRoles_dbo.Roles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UsersInRoles] CHECK CONSTRAINT [FK_dbo.UsersInRoles_dbo.Roles_RoleId]
GO
ALTER TABLE [dbo].[UsersInRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UsersInRoles_dbo.Users_IdentityUser_Id] FOREIGN KEY([IdentityUser_Id])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UsersInRoles] CHECK CONSTRAINT [FK_dbo.UsersInRoles_dbo.Users_IdentityUser_Id]
GO
ALTER TABLE [dbo].[VoucherTransaction]  WITH CHECK ADD  CONSTRAINT [FK_dbo.VoucherTransaction_dbo.Product_Product_Id] FOREIGN KEY([Product_Id])
REFERENCES [dbo].[Product] ([Id])
GO
ALTER TABLE [dbo].[VoucherTransaction] CHECK CONSTRAINT [FK_dbo.VoucherTransaction_dbo.Product_Product_Id]
GO
/****** Object:  StoredProcedure [dbo].[GenerateReportByTerminal]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GenerateReportByTerminal]
	-- Add the parameters for the stored procedure here
	@TerminalId UNIQUEIDENTIFIER,
	@ReportType INT,
	@ReportId UNIQUEIDENTIFIER OUTPUT
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
	Declare @CloseDate AS DATETIME;
	Declare @OpenDate AS DATETIME;
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
		set @CloseDate =isnull((SELECT  Max(ActivityDate) FROM TerminalStatusLog WHERE TerminalId = @TerminalId AND Status = 0),GETDATE());-- closing date should always GETDATE();--
		-- Get date last open
		set @OpenDate=ISNULL((SELECT  Max(ActivityDate) FROM TerminalStatusLog WHERE TerminalId = @TerminalId AND Status = 1 AND ActivityDate < @CloseDate),GETDATE());
		
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
OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType in (7,9) AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  OrderMaster.TerminalId = @TerminalId;
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

	CREATE TABLE #TempPaymentData(
 
	[PaidAmount] [decimal](12, 2) NULL,	
	[Direction] [decimal](12, 2) NULL,
	[CashChange] [decimal](12, 2) NULL,	
	[PaymentType] [int] NULL
	);

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
					WHERE OrderMaster.TrainingMode=0 AND Payment.ReturnAmount > 0 AND 
					Payment.PaymentType != 1 AND   OrderMaster.InvoiceGenerated=1 
					AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND 
					OrderMaster.TerminalId = @TerminalId and (OrderMaster.Status = 13 or OrderMaster.Status = 17);

	INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
	SELECT  NEWID(), @ReportId, 'PaymentTypeSale', Sum(Payment.PaidAmount), Payment.PaymentType, @SortOrder
			FROM #TempPaymentData Payment 
			WHERE Payment.Direction = 1
			GROUP BY Payment.PaymentType;

		-- Get TotalSale on PaymentType
	/*INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeSale', Sum(Payment.PaidAmount), Payment.PaymentType, @SortOrder
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE Payment.PaymentType != 9 AND OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 
					AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = 1 AND OrderMaster.Status in (13,17) 
					AND  OrderMaster.TerminalId = @TerminalId
						GROUP BY Payment.PaymentType;*/

	/*set @SortOrder=@SortOrder+1;

	INSERT INTO ReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeSale', Sum(Payment.PaidAmount) + @CashOutReturnAmountMobile, Payment.PaymentType, @SortOrder
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE Payment.PaymentType = 9 AND OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 
					AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.Direction = 1 AND 
					OrderMaster.Status in (13,17) AND  OrderMaster.TerminalId = @TerminalId
						GROUP BY Payment.PaymentType;*/

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













GO
/****** Object:  StoredProcedure [dbo].[SP_DailySale]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2017-04-21
-- Description:	Sale By Day
-- =============================================
CREATE PROCEDURE [dbo].[SP_DailySale] 
@OutletId UNIQUEIDENTIFIER,
		@OpenDate AS DATETIME,
 	@CloseDate AS DATETIME
AS
BEGIN
	IF @OutletId<>'00000000-0000-0000-0000-000000000000'
		
SELECT  sum(((OrderDetail.Direction* OrderDetail.Qty)* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum((((OrderDetail.Direction*OrderDetail.Qty)*OrderDetail.UnitPrice)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)) as NetTotal,Category.Name as GroupName,OrderMaster.OutletId
	FROM OrderMaster LEFT JOIN OrderDetail 
		ON OrderMaster.Id = OrderDetail.OrderId 
		inner join ItemCategory on ItemCategory.ItemId=OrderDetail.ItemId AND ItemCategory.IsPrimary=1
		inner join Category on ItemCategory.CategoryId=Category.Id
			WHERE OrderMaster.InvoiceGenerated=1   AND OrderDetail.Active=1 AND  OrderDetail.ItemType<>1  AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.OutletId=@OutletId
                 GROUP BY   Category.Name,OrderMaster.OutletId
	ELSE
		SELECT  sum(((OrderDetail.Direction* OrderDetail.Qty)* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum((((OrderDetail.Direction*OrderDetail.Qty)*OrderDetail.UnitPrice)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)) as NetTotal,Category.Name as GroupName,OrderMaster.OutletId
	FROM OrderMaster LEFT JOIN OrderDetail 
		ON OrderMaster.Id = OrderDetail.OrderId 
		inner join ItemCategory on ItemCategory.ItemId=OrderDetail.ItemId AND ItemCategory.IsPrimary=1
		inner join Category on ItemCategory.CategoryId=Category.Id
			WHERE OrderMaster.InvoiceGenerated=1   AND OrderDetail.Active=1 AND  OrderDetail.ItemType<>1  AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate
                 GROUP BY   Category.Name,OrderMaster.OutletId

END
































GO
/****** Object:  StoredProcedure [dbo].[SP_DashBoardSale]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammd Munir
-- Create date: 2015-12-09
-- Description:	Generate monthly report by category wise and Day wise
-- =============================================
CREATE PROCEDURE [dbo].[SP_DashBoardSale] 
	 @OpenDate datetime,
						 @CloseDate datetime
						
AS
BEGIN

SELECT  cast( OrderMaster.InvoiceDate as date) as SaleDate,  sum(((OrderDetail.Direction* OrderDetail.Qty)* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum(((OrderDetail.UnitPrice*(OrderDetail.Qty*OrderDetail.Direction))-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NetTotal,OrderMaster.OutletId, [dbo].[Fn_OutletById](OrderMaster.OutletId) as OutletName
FROM            OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId						
						Where OrderDetail.Active=1 AND  OrderDetail.ItemType<>1 AND (OrderMaster.InvoiceDate between @OpenDate AND @CloseDate) 
						group by cast(OrderMaster.InvoiceDate as date),OrderMaster.OutletId
				
					
END




























GO
/****** Object:  StoredProcedure [dbo].[SP_ExportAccounting]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_ExportAccounting] 	
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
					
insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT '2820','Dricks', cast(OrderMaster.InvoiceDate as date) as SaleDay,isnull(Sum(Payment.TipAmount),0)*-1,isnull(Sum(Payment.TipAmount),0)*-1,1000
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












GO
/****** Object:  StoredProcedure [dbo].[SP_ExportAccountingByProduct]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_ExportAccountingByProduct] 	
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
					


insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT  '3740','Rounded Amount', cast(OrderMaster.InvoiceDate as date) as SaleDay,CAST( cast((ISNULL(Sum((-1)*RoundedAmount),0))+@ReturnRound as numeric(12,2)) as varchar(20)),cast(ISNULL(Sum((-1)*RoundedAmount),0)+@ReturnRound as numeric(12,2)),2 FROM OrderMaster 
					WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=13 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date)

DECLARE @LoopCounter INT = 1, @MaxAccountingId INT  , 
        @AccountingId int,@AcNo varchar(50),@Title varchar(100), @SortOrder INT
 select @MaxAccountingId=MAX(Id) from Accounting; 
 

 insert into #TempSale(DataType,DataTypeText,SaleDay,DataValue,Amount,SortOrder)
SELECT  Tax.AccountingCode,CAST( Item.TAX as varchar(10))+'%',cast(OrderMaster.InvoiceDate as date) as SaleDay,cast(cast((-1)*isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)/(1+Item.TAX/100))*(Item.TAX/100)),0) as numeric(12,2))as varchar(20)),(-1)*isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)/(1+Item.TAX/100))*(Item.TAX/100)),0) ,3
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Item on OrderDetail.ItemId=Item.Id
			inner join Tax on Item.TAX=Tax.TaxValue
				WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=13 AND    OrderMaster.InvoiceGenerated=1 AND  OrderDetail.Active=1 AND OrderDetail.ItemType<>1 AND OrderDetail.Direction=1  AND OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId
				GROUP BY Item.TAX, cast(OrderMaster.InvoiceDate as date),Tax.AccountingCode order by Tax.AccountingCode asc




	
   set @SortOrder=4;
	WHILE(@LoopCounter <= @MaxAccountingId)
 BEGIN
 set @AcNo=null;
 SELECT @AccountingId = Id
   FROM Accounting WHERE Id = @LoopCounter   
   
   select @AcNo=cast(AccountingTerminal.AcNo as varchar(10)) from AccountingTerminal where AccountingId=@AccountingId AND TerminalId=@TerminalId; 

if  @AcNo is null
select @AcNo=cast(Accounting.AcNo as varchar(10)),@Title= Accounting.Name from Accounting where Id=@AccountingId
else
select @Title= Accounting.Name from Accounting where Id=@AccountingId
 
	  set @SortOrder=@SortOrder+1;
	INSERT INTO #TempSale (DataType, DataTypeText,SaleDay,DataValue,Amount, SortOrder)
	SELECT @AcNo,@Title,cast(OrderMaster.InvoiceDate as date) as SaleDay,Cast(Cast((-1)*Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)/(1+Item.TAX/100))) as numeric(12,2))as varchar(20)),(-1)*Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)/(1+Item.TAX/100))),@SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Item on Item.Id=OrderDetail.itemID	
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
SELECT  Tax.AccountingCode,CAST( Item.TAX as varchar(10))+'%',cast(OrderMaster.InvoiceDate as date) as SaleDay,cast(cast(isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount)/(1+Item.TAX/100))*(Item.TAX/100)),0) as numeric(12,2))as varchar(20)),(-1)*isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount)/(1+Item.TAX/100))*(Item.TAX/100)),0),51
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Item on OrderDetail.ItemId=Item.Id
			inner join Tax on Item.Tax=Tax.TaxValue
				WHERE OrderMaster.TrainingMode=0 AND  OrderMaster.Status=15 AND    OrderMaster.InvoiceGenerated=1 AND  OrderDetail.Active=1 AND OrderDetail.ItemType<>1  AND OrderDetail.Direction=-1 AND OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId
				GROUP BY Item.Tax, cast(OrderMaster.InvoiceDate as date),Tax.AccountingCode order by Tax.AccountingCode asc


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
	SELECT @AcNo,@Title,cast(OrderMaster.InvoiceDate as date) as SaleDay,Cast(Cast(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount)/(1+Item.Tax/100))) as numeric(12,2))as varchar(20)),(-1)*Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount)/(1+Item.Tax/100))),@SortOrder
		FROM OrderMaster LEFT JOIN OrderDetail 
			ON OrderMaster.Id = OrderDetail.OrderId 
			inner join Item on Item.Id=OrderDetail.itemID	
				WHERE OrderMaster.TrainingMode=0 AND Item.AccountingId=@AccountingId AND  OrderMaster.Status=15 AND   OrderMaster.InvoiceGenerated=1 AND  OrderDetail.Active=1 AND OrderDetail.ItemType<>1  AND OrderDetail.Direction=-1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate   AND  OrderMaster.TerminalId = @TerminalId
					GROUP BY cast(OrderMaster.InvoiceDate as date),Item.AccountingId;
					
   SET @LoopCounter  = @LoopCounter  + 1        
END	

select *from #TempSale order by SaleDay
END































GO
/****** Object:  StoredProcedure [dbo].[SP_HourlySale]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2017-04-21
-- Description:	Sale By Hours
-- =============================================
CREATE PROCEDURE [dbo].[SP_HourlySale] 
@OutletId UNIQUEIDENTIFIER,
		@OpenDate AS DATETIME,
 	@CloseDate AS DATETIME
AS
BEGIN
	IF @OutletId<>'00000000-0000-0000-0000-000000000000'
		SELECT    cast(OrderMaster.InvoiceDate as date) as SaleDay,DATEPART(HOUR,OrderMaster.InvoiceDate) as SaleHour,     OrderMaster.InvoiceDate,  (OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount as GrossTotal,((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100) as NetTotal
                    FROM          
                         OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId 
						Where   OrderDetail.Active=1 AND  OrderDetail.ItemType<>1 AND (OrderMaster.InvoiceDate between @OpenDate AND @CloseDate) AND OrderMaster.OutletId=@OutletId
	ELSE
		SELECT    cast(OrderMaster.InvoiceDate as date) as SaleDay,DATEPART(HOUR,OrderMaster.InvoiceDate) as SaleHour,     OrderMaster.InvoiceDate,  (OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount as GrossTotal,((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100) as NetTotal
                 FROM          
                         OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId 
						Where   OrderDetail.Active=1 AND  OrderDetail.ItemType<>1 AND (OrderMaster.InvoiceDate between @OpenDate AND @CloseDate)

END


































GO
/****** Object:  StoredProcedure [dbo].[SP_MonthlyReport]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammd Munir
-- Create date: 2015-12-09
-- Description:	Generate monthly report by category wise and Day wise
-- =============================================
CREATE PROCEDURE [dbo].[SP_MonthlyReport] 
	 @OpenDate datetime,
						 @CloseDate datetime
						
AS
BEGIN

 CREATE TABLE #TempTable(
 DataType varchar(50),
 SaleDay date null,
 GrossTotal decimal,
 NetTotal decimal,
 Name varchar(100),
 OutletId uniqueidentifier
)
insert into #TempTable(
DataType,
 SaleDay ,
 GrossTotal,
 NetTotal,
 OutletId
)(
SELECT  'SaleDay' ,      cast(OrderMaster.InvoiceDate as date) as SaleDay,  sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NetTotal,OrderMaster.OutletId
FROM           
                         OrderMaster INNER JOIN
                         OrderDetail ON OrderMaster.Id = OrderDetail.OrderId
						Where OrderMaster.Status=13 AND OrderDetail.Active=1 AND  OrderDetail.ItemType<>1 AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate
						group by  cast(OrderMaster.InvoiceDate as date),OrderMaster.OutletId
)
						
						insert into #TempTable(
DataType,
 GrossTotal,
 NetTotal,
 Name,
 OutletId
)(SELECT 'CategorySale', sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))  as NetTotal,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId) as Name,OrderMaster.OutletId
	FROM OrderMaster LEFT JOIN OrderDetail 
		ON OrderMaster.Id = OrderDetail.OrderId 
		
			WHERE OrderMaster.Status=13 AND   OrderMaster.InvoiceGenerated=1   AND OrderDetail.Active=1 AND  OrderDetail.ItemType<>1  AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate
					GROUP BY   [dbo].[Fn_CategoryByItem](OrderDetail.ItemId),OrderMaster.OutletId)

					select *from #TempTable
				
					
END





































GO
/****** Object:  StoredProcedure [dbo].[SP_PrintDetailReportByDateRange]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_PrintDetailReportByDateRange] 
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
OrderMaster.InvoiceGenerated=1 AND Payment.PaymentType in (7,9) AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  OrderMaster.TerminalId = @TerminalId;

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





GO
/****** Object:  StoredProcedure [dbo].[SP_PrintJournal]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2018-08-15
-- Description:	Print Journal Log
-- =============================================
CREATE PROCEDURE [dbo].[SP_PrintJournal] 
	@dtFrom datetime,
	@dtTo datetime
AS
BEGIN
	SELECT Journal.Id, JournalAction.ActionCode, Journal.Created,
CASE JournalAction.ActionCode
              WHEN  'NewOrderEntry' THEN   JournalAction.[Description] + isnull(OrderMaster.OrderNoOfDay,'1')
              WHEN  'ItemAdded' THEN   Journal.LogMessage
			 WHEN  'ReturnItemAdded' THEN   Journal.LogMessage
              WHEN  'ItemDeleted' THEN   Journal.LogMessage
			   WHEN  'ItemMoved' THEN   Journal.LogMessage
             WHEN 'ItemDiscountAdded' THEN  Journal.LogMessage
              WHEN  'ReceiptKitchen' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'ReceiptGenerating' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'ReceiptGenerated' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'ReceiptFail' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )

              WHEN  'OpenOrderSelected' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'DirectCashPaymentStarted' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'DirectCardPaymentStarted' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'PaymentTerminalWindowOpen' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'PaymentTerminalWindowCancel' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'PaymentTerminalWindowClosed' THEN   JournalAction.[Description]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )

              WHEN  'PaymentDeviceTotal' THEN   Journal.[LogMessage]   + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )


              WHEN  'PrintPerforma' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'ReceiptPrinted' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )+'|'+(SELECT 
   STUFF( (SELECT   '~'    +CAST( Product.Description as varchar(50)) +'  '+ Cast( OrderDetail.Qty as varchar(50)) +' X '+Cast(OrderDetail.UnitPrice as varchar(50)) +'  =  '+ cast (OrderDetail.Qty*OrderDetail.UnitPrice as varchar(50))
                             FROM dbo.OrderDetail
							 inner join Product on Product.Id=OrderDetail.ItemId 
                             Where OrderDetail.OrderId=OrderMaster.Id AND OrderDetail.Active=1
                             FOR XML PATH('')), 
                            1, 1, '')) +'|'+ cast((select top 1  VatDetail from Receipt Where ReceiptNumber=OrderMaster.InvoiceNumber) as varchar(500)) 
			  WHEN  'ReceiptPrintedForReturnOrder' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
			   WHEN 'ReceiptPrintedForReturnOrderViaTrainingMode' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
			  WHEN  'ReceiptPrintedViaTrainingMode' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
			  WHEN  'ReceiptCopyPrintedViaTrainingMode' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500) )
              WHEN  'OrderCreditcardPayment' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500))
              WHEN  'OrderCashPayment' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500))
			  WHEN  'OrderSwishPayment' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500))
			  WHEN  'OrderAccountPayment' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500))
			  WHEN  'OrderCouponPayment' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500))
			  WHEN  'OrderMobileTerminalPayment' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500))
			  WHEN  'OrderReturnCashPayment' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500))
              WHEN  'ReceiptCopyPrinted' THEN   JournalAction.[Description] + CAST(OrderMaster.OrderNoOfDay AS varchar(500))
              WHEN  'PaymentScreenNavigation' THEN   'OrderId:'+CAST(OrderMaster.OrderNoOfDay AS varchar(500)) + JournalAction.[Description]          
              WHEN  'OrderTypeReturn' THEN   'OrderId:'+ isnull(OrderMaster.OrderNoOfDay,' ') + JournalAction.[Description] 
			  WHEN  'OrderCancelled' THEN   'OrderId:'+ isnull(OrderMaster.OrderNoOfDay,' ') + JournalAction.[Description]                       
			  WHEN  'ReceiptViewed' THEN   'OrderId:'+ isnull(OrderMaster.OrderNoOfDay,' ') + JournalAction.[Description]                       
              WHEN  'OrderTableSelected' THEN   '(Table:'+CAST(isnull(Journal.TableId,0) AS varchar(10)) + ') ' +JournalAction.[Description] + CAST(isnull(OrderMaster.OrderNoOfDay,'-1') AS varchar(500))
              WHEN  'ReportXViewed' THEN   JournalAction.[Description] + Users.UserName + '(' +CAST(Users.Email AS varchar(500) )+')'
              WHEN  'ReportXPrinted' THEN   JournalAction.[Description] + Users.UserName + '(' +CAST(Users.Email AS varchar(500) )+')'
              WHEN  'ReportZViewed' THEN   JournalAction.[Description] + Users.UserName + '(' +CAST(Users.Email AS varchar(500) )+')'
              WHEN  'ReportZPrinted' THEN   JournalAction.[Description] + Users.UserName + '(' +CAST(Users.Email AS varchar(500) )+')'
              WHEN  'OpenCashDrawer' THEN   JournalAction.[Description] + Users.UserName + '(' +CAST(Users.Email AS varchar(500) )+')'
			   WHEN  'TerminalOpened' THEN   JournalAction.[Description] + Users.UserName + '(' +CAST(Users.Email AS varchar(500) )+')'
			     WHEN  'TerminalClosed' THEN   JournalAction.[Description] + Users.UserName + '(' +CAST(Users.Email AS varchar(500) )+')'
			   WHEN  'LoggedIn' THEN  + Users.UserName+ '('+ Users.Email + ')'+JournalAction.[Description]
			   WHEN  'LoggedOut' THEN   + Users.UserName+ '('+ Users.Email + ')'+JournalAction.[Description]
              WHEN  'InvoiceViewed' THEN   'OrderId:'+CAST(OrderMaster.OrderNoOfDay AS varchar(500)) + JournalAction.[Description] + Users.UserName + '(' +CAST(Users.Email AS varchar(500) )+')'
              ELSE  JournalAction.[Description]
   END AS JournalLog
  FROM [dbo].[Journal]  
  INNER JOIN OutletUser as Users ON Users.Id = Journal.UserId
  LEFT  JOIN Product  ON Product.Id = Journal.ItemId 
  LEFT  JOIN OrderMaster ON OrderMaster.Id = Journal.OrderId 
  INNER JOIN JournalAction ON JournalAction.Id = Journal.ActionId
  Where Journal.Created between @dtFrom AND @dtTo
  ORDER BY Journal.Created

END




































GO
/****** Object:  StoredProcedure [dbo].[SP_SaleByAccounting]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Muhammad Munir
-- Create date: 2016-06-28
-- Description:	Sale By Accounting
-- =============================================
CREATE PROCEDURE [dbo].[SP_SaleByAccounting] 
@TerminalId UNIQUEIDENTIFIER,
		@OpenDate AS DATETIME,
 	@CloseDate AS DATETIME
AS
BEGIN
	IF @TerminalId<>'00000000-0000-0000-0000-000000000000'
		SELECT Accounting.Id,Accounting.AcNo, Accounting.Name, sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)) as NetTotal,Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)) as VAT,				
			[dbo].Fn_SaleByAccountingPercentage(1,@TerminalId,Accounting.Id,25,@OpenDate,@CloseDate) as Percentage25Total,
			
		[dbo].Fn_SaleByAccountingPercentage(1,@TerminalId,Accounting.Id,12,@OpenDate,@CloseDate) as Percentage12Total,
			
			[dbo].Fn_SaleByAccountingPercentage(1,@TerminalId,Accounting.Id,6,@OpenDate,@CloseDate) as Percentage6Total,
			
			[dbo].Fn_SaleByAccountingPercentage(1,@TerminalId,Accounting.Id,0,@OpenDate,@CloseDate) as Percentage0Total
			

		FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
					inner JOIN Product ON OrderDetail.ItemId = Product.Id 
					inner JOIN Accounting ON Product.AccountingId = Accounting.Id 
						WHERE OrderMaster.InvoiceGenerated=1   AND OrderDetail.Active=1    AND  OrderDetail.ItemType<>1   AND (OrderMaster.InvoiceDate between @OpenDate AND @CloseDate) AND OrderMaster.TerminalId=@TerminalId   	group by Accounting.Id,Accounting.AcNo,Accounting.Name  

	ELSE
		
				SELECT Accounting.Id,Accounting.AcNo, Accounting.Name, sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100)) as NetTotal, Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)) as VAT,				
			[dbo].Fn_SaleByAccountingPercentage(1,@TerminalId,Accounting.Id,25,@OpenDate,@CloseDate) as Percentage25Total,
			-- [dbo].Fn_SaleByAccountingPercentage(2,@TerminalId,Accounting.Id,25,@OpenDate,@CloseDate) as Percentage25NetTotal,
			--  [dbo].Fn_SaleByAccountingPercentage(3,@TerminalId,Accounting.Id,25,@OpenDate,@CloseDate) as Percentage25VAT,
	   
		[dbo].Fn_SaleByAccountingPercentage(1,@TerminalId,Accounting.Id,12,@OpenDate,@CloseDate) as Percentage12Total,
			-- [dbo].Fn_SaleByAccountingPercentage(2,@TerminalId,Accounting.Id,12,@OpenDate,@CloseDate) as Percentage12NetTotal,
			 -- [dbo].Fn_SaleByAccountingPercentage(3,@TerminalId,Accounting.Id,12,@OpenDate,@CloseDate) as Percentage12VAT,

			[dbo].Fn_SaleByAccountingPercentage(1,@TerminalId,Accounting.Id,6,@OpenDate,@CloseDate) as Percentage6Total,
			-- [dbo].Fn_SaleByAccountingPercentage(2,@TerminalId,Accounting.Id,6,@OpenDate,@CloseDate) as Percentage6NetTotal,
			--  [dbo].Fn_SaleByAccountingPercentage(3,@TerminalId,Accounting.Id,6,@OpenDate,@CloseDate) as Percentage6VAT,
	
			[dbo].Fn_SaleByAccountingPercentage(1,@TerminalId,Accounting.Id,0,@OpenDate,@CloseDate) as Percentage0Total
			-- [dbo].Fn_SaleByAccountingPercentage(2,@TerminalId,Accounting.Id,0,@OpenDate,@CloseDate) as Percentage0NetTotal,
			 -- [dbo].Fn_SaleByAccountingPercentage(3,@TerminalId,Accounting.Id,0,@OpenDate,@CloseDate) as Percentage0VAT
	

		FROM OrderMaster LEFT JOIN OrderDetail 
					ON OrderMaster.Id = OrderDetail.OrderId 
					inner JOIN Product ON OrderDetail.ItemId = Product.Id 
					inner JOIN Accounting ON Product.AccountingId = Accounting.Id 
						WHERE OrderMaster.InvoiceGenerated=1   AND OrderDetail.Active=1    AND  OrderDetail.ItemType<>1  AND (OrderMaster.InvoiceDate between @OpenDate AND @CloseDate)   	group by Accounting.Id,Accounting.AcNo,Accounting.Name  
	

END




























GO
/****** Object:  StoredProcedure [dbo].[SP_SaleByCategory]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[SP_SaleByCategory]  @TerminalId UNIQUEIDENTIFIER, @OpenDate AS DATETIME, @CloseDate AS DATETIME
AS

BEGIN
IF @TerminalId<>'00000000-0000-0000-0000-000000000000'
	SELECT sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,
sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100))-ItemDiscount) 
as NetTotal,

Sum((OrderDetail.Direction*OrderDetail.Qty* OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)) 
as VAT,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId) as Name,

[dbo].[Fn_SaleByCategoryPercentage](1,@TerminalId,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId),25,@OpenDate,@CloseDate) as Percentage25Total,

[dbo].[Fn_SaleByCategoryPercentage](1,@TerminalId,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId),12,@OpenDate,@CloseDate) as Percentage12Total,

[dbo].[Fn_SaleByCategoryPercentage](1,@TerminalId,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId),6,@OpenDate,@CloseDate) as Percentage6Total,

[dbo].[Fn_SaleByCategoryPercentage](1,@TerminalId,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId),0,@OpenDate,@CloseDate) as Percentage0Total

	FROM OrderMaster LEFT JOIN OrderDetail 
		ON OrderMaster.Id = OrderDetail.OrderId 
		
			WHERE OrderMaster.InvoiceGenerated=1   AND OrderDetail.Active=1    AND  OrderDetail.ItemType=0   AND (OrderMaster.InvoiceDate between @OpenDate AND @CloseDate)  AND OrderMaster.TerminalId=@TerminalId   GROUP BY   [dbo].[Fn_CategoryByItem](OrderDetail.ItemId)
ELSE
SELECT sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice)-ItemDiscount) as GrossTotal,sum((OrderDetail.Direction* OrderDetail.Qty* OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100))-ItemDiscount) as NetTotal,Sum((OrderDetail.Direction*OrderDetail.Qty*OrderDetail.UnitPrice/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)) as VAT,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId) as Name,
[dbo].[Fn_SaleByCategoryPercentage](1,@TerminalId,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId),25,@OpenDate,@CloseDate) as Percentage25Total,

[dbo].[Fn_SaleByCategoryPercentage](1,@TerminalId,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId),12,@OpenDate,@CloseDate) as Percentage12Total,

[dbo].[Fn_SaleByCategoryPercentage](1,@TerminalId,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId),6,@OpenDate,@CloseDate) as Percentage6Total,

[dbo].[Fn_SaleByCategoryPercentage](1,@TerminalId,[dbo].[Fn_CategoryByItem](OrderDetail.ItemId),0,@OpenDate,@CloseDate) as Percentage0Total


	FROM OrderMaster LEFT JOIN OrderDetail 
		ON OrderMaster.Id = OrderDetail.OrderId 
		
			WHERE OrderMaster.InvoiceGenerated=1   AND OrderDetail.Active=1    AND  OrderDetail.ItemType=0   AND (OrderMaster.InvoiceDate between @OpenDate AND @CloseDate)   GROUP BY   [dbo].[Fn_CategoryByItem](OrderDetail.ItemId)

END



GO
/****** Object:  StoredProcedure [dbo].[SP_UserReportByDateRange]    Script Date: 2023-05-11 12:32:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		abc
-- Create date: 2016-02-01
-- update date: 2017-11-01
-- Description:	Print Detail Report in Date Range
-- =============================================
Create PROCEDURE [dbo].[SP_UserReportByDateRange] 
    @TerminalId UNIQUEIDENTIFIER,	
	@OpenDate AS DATETIME,
	@CloseDate AS DATETIME,
	@UserId AS varchar(50)

AS
BEGIN
	
	
	 Declare @ReportId AS uniqueidentifier;
	Declare @ReportType AS int;		
	Declare @OutletId AS INT;
	Declare @UniqueIdentification AS nvarchar(255);	
	Declare @OpenCash AS DECIMAL(12,2);
	Declare @CashIn AS DECIMAL(12,2);
	Declare @CashOut AS DECIMAL(12,2);
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
		

			-- Get unique identifier for terminal
	SELECT top 1 @UniqueIdentification = UniqueIdentification FROM Terminal Where Id=@TerminalId;
	-- Set reportnumber on report
	INSERT INTO #TempReportData (Id, ReportId, DataType, TextValue, SortOrder) VALUES (NEWID(), @ReportId, 'UniqueIdentification', @UniqueIdentification, 1);
	INSERT INTO #TempReportData (Id, ReportId, DataType, DateValue, SortOrder) VALUES (NEWID(),@ReportId,'CloseDate', @CloseDate, 3);
    INSERT INTO #TempReportData (Id, ReportId, DataType, DateValue, SortOrder) VALUES (NEWID(),@ReportId,'OpenDate', @OpenDate, 2);

	
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
			
				WHERE   OrderMaster.TrainingMode=0  AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1    AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId and OrderMaster.UserId=@UserId
		

	SELECT @SaleTotal= Sum((OrderDetail.UnitPrice*OrderDetail.Qty)-OrderDetail.ItemDiscount)
		FROM #TempData OrderDetail 
			
				WHERE  Status =13 AND OrderDetail.Direction=1 ;


SELECT @SaleReturn= isnull(Sum((OrderDetail.UnitPrice*OrderDetail.Qty)+OrderDetail.ItemDiscount),0)
		FROM #TempData OrderDetail 
			
				WHERE  Status =15 AND OrderDetail.Direction=-1;



	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	Values(NEWID(), @ReportId, 'TotalSale',@SaleTotal,4)
		
	-- ### Get total net of given period

	
	SELECT @NetTotal=Sum(((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))
   FROM #TempData OrderDetail 
			
				WHERE  Status =13 AND OrderDetail.Direction=1


	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
				VALUES(NEWID(), @ReportId, 'TotalNet',@NetTotal,5);	
					-- ### Get total return of given period
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	values(NEWID(), @ReportId, 'TotalReturn',@SaleReturn,6)
		
-- ### Get total return of given period
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	Values(NEWID(), @ReportId, 'SaleTotal',@SaleTotal-@SaleReturn,7)
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
INSERT INTO #TempPaymentData(PaidAmount,PaymentType,Direction,CashChange)		
SELECT  Payment.PaidAmount,Payment.PaymentType,Payment.Direction,Payment.CashChange
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND InvoiceDate BETWEEN @OpenDate AND @CloseDate AND Payment.PaidAmount >0  AND  OrderMaster.TerminalId = @TerminalId and OrderMaster.CheckOutUserId=@UserId;
				
				

				---## Total Payment
				SELECT  @TotalPayment= Sum(Payment.PaidAmount)
			FROM #TempPaymentData Payment 
				
					WHERE Payment.Direction = 1;
				---### Return Payment
			Declare @TotalReturnPayment as decimal(12,2);
			SELECT  @TotalReturnPayment= Sum(Payment.PaidAmount)
			FROM #TempPaymentData Payment 
				
					WHERE Payment.Direction = -1;
			

			SELECT  @RoundingAmount= ISNULL(Sum((-1)*RoundedAmount),0)
			FROM OrderMaster 			
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status=13  AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND  OrderMaster.TerminalId = @TerminalId;
					
					declare @res as decimal(12,2);
					set @res=@TotalPayment+(-1)*@NetTotal+(-1)*@VatSum+@RoundingAmount-@TotalReturnPayment-(-1)*@SaleReturn;
					if (@res<>0)
					begin
					set @RoundingAmount=@RoundingAmount-@res;
					end


INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	VALUES(NEWID(), @ReportId, 'Rounding',@RoundingAmount,8);
		-- ### Get acumulated total vat without taking consideration of return orders

		

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent, SortOrder)				
					VALUES( NEWID(), @ReportId, 'VATSum',@VatSum,0,9);
				--## Group by Tax Percentage
 
 INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent,TextValue, SortOrder)
		SELECT NEWID(), @ReportId, 'VATPercent', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)-(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,OrderDetail.TaxPercent,CAST( OrderDetail.TaxPercent as varchar(10))+'%', 10
		FROM #TempData OrderDetail 
			
				WHERE   OrderDetail.Direction=1
					GROUP BY (OrderDetail.TaxPercent);

	-- ### Get acumulated total return vat without taking consideration of return orders
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnVATSum', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,0, 11
		FROM #TempData OrderDetail 
			
				WHERE  Status =15 AND OrderDetail.Direction=-1
				
				--## Group by Tax Percentage
 
 INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TaxPercent,TextValue, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnVATPercent', isnull(Sum((((OrderDetail.UnitPrice*OrderDetail.Qty)+(OrderDetail.ItemDiscount))/(1+OrderDetail.TaxPercent/100))*(OrderDetail.TaxPercent/100)),0) AS VATSum,OrderDetail.TaxPercent,CAST( OrderDetail.TaxPercent as varchar(10))+'%', 12
		FROM #TempData OrderDetail 
			
				WHERE  Status =15 AND OrderDetail.Direction=-1
					GROUP BY (OrderDetail.TaxPercent);


--#Get Training mode sale
	

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)		
		VALUES( NEWID(), @ReportId, 'TrainingModeSale', 0, 13)


	-- Get open cash
	SELECT TOP 1 @OpenCash = Isnull(CashDrawerLog.Amount,0) FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityType = 1 AND TerminalId = @TerminalId ORDER BY ActivityDate DESC

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder) VALUES (NEWID(), @ReportId, 'CashDrawerOpen', @OpenCash, 14);
	-- ###  cash added in drawer 
	SELECT @CashAdded= isnull(Sum(Amount),0)
			FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityType = 5 AND ActivityDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId

	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		values( NEWID(), @ReportId, 'CashAdded',@CashAdded, 15)
			
 -- ###  cash dropped in drawer 
 SELECT @CashDropped= Isnull(Sum(Amount),0)
			FROM CashDrawerLog LEFT JOIN CashDrawer ON CashDrawerLog.CashDrawerId = CashDrawer.Id WHERE ActivityType = 4 AND ActivityDate BETWEEN @OpenDate AND @CloseDate AND terminalId = @TerminalId and CashDrawerLog.UserId=@UserId
		
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		values( NEWID(), @ReportId, 'CashDropped', @CashDropped, 16)
			-- Get TotalSale Cash In
	SELECT @CashIn=isnull(Sum(Payment.PaidAmount),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' 
					AND    OrderMaster.InvoiceGenerated=1 and Payment.Direction = 1 AND Payment.PaymentType=1 
					--AND InvoiceDate BETWEEN @OpenDate AND @CloseDate 
					AND  OrderMaster.TerminalId = @TerminalId and OrderMaster.UserId=@UserId;
			INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)VALUES(NEWID(), @ReportId, 'CashIn',@CashIn,17);

	
	--				-- Get TotalSale Cash Out
	--	SELECT  @CashOut= isnull(Sum(Payment.CashChange),0)
	--		FROM #TempPaymentData Payment 
	--				WHERE Payment.Direction = -1 AND Payment.PaymentType=7
	---- 
	--INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)VALUES( NEWID(), @ReportId, 'CashOut', @CashOut, 18);
	--	-- Get TotalSale Cash Sum	
	--	set @CashSum=@OpenCash+@CashAdded+@CashIn-@CashDropped-@CashOut;
	--INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'CashSum',@CashSum, 19);				

	-- Get TotalSale Cash Out

		SELECT  @CashOut= isnull(Sum(Payment.CashChange),0)
			FROM OrderMaster JOIN Payment 
				ON Payment.OrderId = OrderMaster.Id
					WHERE OrderMaster.TrainingMode=0 AND   OrderMaster.CustomerId='00000000-0000-0000-0000-000000000000' 
					AND    OrderMaster.InvoiceGenerated=1 and Payment.Direction = -1 AND Payment.PaymentType=7 
					--AND InvoiceDate BETWEEN @OpenDate AND @CloseDate 
					AND  OrderMaster.TerminalId = @TerminalId and OrderMaster.UserId=@UserId;
	-- 
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)VALUES( NEWID(), @ReportId, 'CashOut', @CashOut, 18);
		-- Get TotalSale Cash Sum	
		SET @CashSum=@OpenCash+@CashAdded+@CashIn-@CashDropped-@CashOut;
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value,SortOrder)Values(  NEWID(), @ReportId,'CashSum',@CashSum, 19);				


	
	
  
		-- Get TotalSale on PaymentType
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeSale', Sum(Payment.PaidAmount), Payment.PaymentType, 20
			FROM #TempPaymentData Payment 
				
					WHERE Payment.Direction = 1 
						GROUP BY Payment.PaymentType;

							-- Get TotalReturn on PaymentType 
							INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
							values( NEWID(), @ReportId, 'PaymentReturnCount',0, 21)
	
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, ForeignId, SortOrder)
		SELECT  NEWID(), @ReportId, 'PaymentTypeReturn', Sum(Payment.PaidAmount), Payment.PaymentType, 22
			FROM #TempPaymentData Payment 
				
					WHERE Payment.Direction = -1 
						GROUP BY Payment.PaymentType;
						
		-- ### Get number of standard orders
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'OrderCount', COUNT(*), 23
		FROM #TempData 				
				WHERE Status=13;

	-- ### Get total sale prodcut count of given period
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'ProductsCount',Sum(OrderDetail.Qty), 24
		FROM #TempData OrderDetail 
			
				WHERE  Status =13 AND OrderDetail.Direction=1

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
				WHERE   OrderMaster.TrainingMode=0 AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId and OrderMaster.UserId=@UserId


	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	VALUES( NEWID(), @ReportId, 'CategorySaleHeading',0,'',0,25);
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	SELECT NEWID(), @ReportId, 'CategorySale', Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount), OrderDetail.Name, 0, 26
	FROM #TempCategoryData OrderDetail
				GROUP BY OrderDetail.Name

				INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	VALUES( NEWID(), @ReportId, 'CategorySaleCountHeading',0,'',0,27);
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, TextValue, ForeignId, SortOrder)
	SELECT NEWID(), @ReportId, 'CategorySaleCount', Sum(OrderDetail.Qty), OrderDetail.Name,OrderDetail.Id as CategoryId, 28
	FROM #TempCategoryData OrderDetail
				GROUP BY OrderDetail.Name,OrderDetail.Id

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
			
				WHERE   OrderMaster.TrainingMode=0  AND  OrderMaster.InvoiceDate  BETWEEN @OpenDate AND @CloseDate AND OrderMaster.InvoiceGenerated=1    AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1  AND  OrderMaster.TerminalId = @TerminalId and OrderMaster.UserId=@UserId
		

--### Accounting
DECLARE @LoopCounter INT = 1, @MaxAccountingId INT  , 
        @AccountingId int,@accountingTotal decimal(12,2),@Title varchar(100), @SortOrder INT
 select @MaxAccountingId=MAX(Id) from Accounting
 INSERT INTO #TempReportData (Id, ReportId, DataType,TextValue, SortOrder)
 Values(NEWID(), @ReportId, 'Accounting',' ',29)	
set @SortOrder=30;
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
				WHERE  Status =13 AND OrderDetail.Direction=1

INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
	SELECT NEWID(), @ReportId, 'DiscountCount', COUNT(*), 79
		FROM #TempData OrderDetail 
				WHERE  Status =13 AND OrderDetail.Direction=1


	-- Get number of return orders
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		SELECT NEWID(), @ReportId, 'ReturnCount', COUNT(*), 81
		FROM OrderMaster 
				WHERE 
					 OrderMaster.InvoiceGenerated=1 AND OrderMaster.Type=1 AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate AND OrderMaster.TerminalId = @TerminalId and OrderMaster.UserId=@UserId;

	

-- ### Get number of training sale orders
					INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)	
		SELECT NEWID(), @ReportId, 'TrainingModeCount', count(Id), 82
		FROM OrderMaster 
				WHERE    OrderMaster.InvoiceGenerated=1 AND OrderMaster.TrainingMode=1  AND OrderMaster.InvoiceDate BETWEEN @OpenDate AND @CloseDate  AND OrderMaster.TerminalId = @TerminalId and OrderMaster.UserId=@UserId;

	-- Get number of return orders
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		select  NEWID(), @ReportId, 'HoldCount', count(tableId), 83
		From OrderMaster
		where TableId=0 And Status=3 And Type=0
		And OrderMaster.CreationDate  BETWEEN @OpenDate AND @CloseDate

	-- ### Get total sale of unfinished orders on given period
	INSERT INTO #TempReportData (Id, ReportId, DataType, Value, SortOrder)
		select  NEWID(), @ReportId, 'HoldTotalSale', Sum(OrderTotal), 84
		From OrderMaster
		where TableId=0 And Status=3 And Type=0
		And OrderMaster.CreationDate  BETWEEN @OpenDate AND @CloseDate
		

	

			
     select *from #TempReportData



END










GO
