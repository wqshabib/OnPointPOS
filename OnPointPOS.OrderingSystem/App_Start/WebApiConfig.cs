using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace POSSUM.OrderingSystem
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
            config.Routes.MapHttpRoute(
                name: "WChat",
                routeTemplate: "api/{controller}",
                defaults: new { controller = "WChat" },
                constraints: new { controller = "WChat" }
            );
            // PublicUrl
            config.Routes.MapHttpRoute(
                name: "PublicUrl",
                routeTemplate: "api/{controller}/{test}",
                constraints: new { controller = "PublicUrl" },
                defaults: new { test = RouteParameter.Optional } // url = dummy ?
            );

            // Cms
            config.Routes.MapHttpRoute(
                name: "Get/Post Cms",
                routeTemplate: "api/{controller}/{token}",
                constraints: new { controller = "Cms" },
                defaults: new { token = RouteParameter.Optional }
            );

            // Cms Content
            config.Routes.MapHttpRoute(
                name: "Get/Post Cms Content",
                routeTemplate: "api/{controller}/{token}",
                constraints: new { controller = "CmsContent" },
                defaults: new { token = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Put/Delete Cms Content",
                routeTemplate: "api/{controller}/{token}/{id}",
                constraints: new { controller = "CmsContent" },
                defaults: new { token = RouteParameter.Optional }
            );

            // Slide
            config.Routes.MapHttpRoute(
                name: "Get/Post Slide",
                routeTemplate: "api/{controller}/{token}",
                constraints: new { controller = "Slide" },
                defaults: new { token = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Put/Delete Slide",
                routeTemplate: "api/{controller}/{token}/{id}",
                constraints: new { controller = "Slide" },
                defaults: new { token = RouteParameter.Optional }
            );



            // Account Sales
            config.Routes.MapHttpRoute(
                name: "Get AccountSales",
                routeTemplate: "api/{controller}/{token}",
                constraints: new { controller = "AccountSales" },
                defaults: new { token = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Get AccountSales for printer",
                routeTemplate: "api/{controller}/{token}/{id}",
                constraints: new { controller = "AccountSales" },
                defaults: new { token = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Get AccountSales Pdf for printer",
                routeTemplate: "api/{controller}/{token}/{payoutGuid}/{orderPrinterGuid}",
                constraints: new { controller = "AccountSales" },
                defaults: new { token = RouteParameter.Optional }
            );

            // Get Restaurant Sales Report
            config.Routes.MapHttpRoute(
                name: "Get RestaurantSales Pdf for printer",
                routeTemplate: "api/{controller}/{token}/{payoutGuid}/{orderPrinterGuid}/{reportType}",
                constraints: new { controller = "RestaurantSales" },
                defaults: new { token = RouteParameter.Optional }
            );


            // Get Content Type
            config.Routes.MapHttpRoute(
                name: "Get Content Type",
                routeTemplate: "api/{controller}/{token}",
                constraints: new { controller = "ContentType" },
                defaults: new { token = RouteParameter.Optional }
            );

            // Get TagLists
            config.Routes.MapHttpRoute(
                name: "Get TagList",
                routeTemplate: "api/{controller}/{token}",
                defaults: new { controller = "TagList" },
                constraints: new { controller = "TagList" }
            );

            // Tag
            config.Routes.MapHttpRoute(
                name: "Post/Update/Delete Tag",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "Tag" },
                constraints: new { controller = "Tag" }
            );

            // PunchTicket
            config.Routes.MapHttpRoute(
                name: "Search PunchTicket",
                routeTemplate: "api/{controller}/{token}",
                defaults: new { controller = "PunchTicket" },
                constraints: new { controller = "PunchTicket" }
            );

            config.Routes.MapHttpRoute(
                name: "Post/Get PunchTicket",
                routeTemplate: "api/{controller}/{token}/{customerId}",
                defaults: new { controller = "PunchTicket" },
                constraints: new { controller = "PunchTicket" }
            );

            config.Routes.MapHttpRoute(
                name: "PunchTicket ActionType (1 = IncrementQuantity, 2 = DecrementQuantity, 3 = IncrementUsed, 4 = DecrementUsed)",
                routeTemplate: "api/{controller}/{token}/{customerId}/{punchTicketId}/{actionType}",
                defaults: new { controller = "PunchTicket" },
                constraints: new { controller = "PunchTicket" }
            );



            // Customer
            config.Routes.MapHttpRoute(
                name: "Search Customer",
                routeTemplate: "api/{controller}/{token}",
                defaults: new { controller = "Customer" },
                constraints: new { controller = "Customer" }
            );

            config.Routes.MapHttpRoute(
                name: "Get/Post/Put/(Delete) Customer",
                routeTemplate: "api/{controller}/{token}/{customerId}",
                defaults: new { controller = "Customer" },
                constraints: new { controller = "Customer" }
            );




            // Post Content Push
            config.Routes.MapHttpRoute(
                name: "Post Content Push",
                routeTemplate: "api/{controller}/{token}/{contentId}",
                defaults: new { controller = "ContentPush" },
                constraints: new { controller = "ContentPush" }
            );



            // Get PublicShopConfig Company
            config.Routes.MapHttpRoute(
                name: "GetPublicShopConfigCompany",
                routeTemplate: "api/{controller}/{identifier}",
                constraints: new { controller = "PublicShopConfig" },
                defaults: new { identifier = RouteParameter.Optional }
            );


            // Get PublicShopConfig
            config.Routes.MapHttpRoute(
                name: "GetPublicShopConfig",
                routeTemplate: "api/{controller}/{companyId}/{type}",
                defaults: new { controller = "PublicShopConfig" },
                constraints: new { controller = "PublicShopConfig" }
            );



            // Get CompanyInfo
            config.Routes.MapHttpRoute(
                name: "GetCompanyInfo",
                routeTemplate: "api/{controller}/{secret}/{companyId}",
                defaults: new { controller = "CompanyInfo" },
                constraints: new { controller = "CompanyInfo" }
            );

            // Get/Put Info
            config.Routes.MapHttpRoute(
                name: "Info",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "Info" },
                constraints: new { controller = "Info" }
            );

            // Get CompanyIdentifier
            config.Routes.MapHttpRoute(
                name: "GetCompanyIdentifier",
                routeTemplate: "api/{controller}/{secret}/{companyId}",
                defaults: new { controller = "CompanyIdentifier" },
                constraints: new { controller = "CompanyIdentifier" }
            );


            // Login
            config.Routes.MapHttpRoute(
                name: "Login",
                routeTemplate: "api/{controller}/{login}",
                defaults: new { controller = "Authorization" }//,
                //constraints: new { controller = "Authorization" }
            );

            // Get Content Template
            config.Routes.MapHttpRoute(
                name: "Get ContentTemplate",
                routeTemplate: "api/{controller}/{token}/{type}",
                defaults: new { controller = "ContentTemplate" },
                constraints: new { controller = "ContentTemplate" }
            );

            // Get Content
            config.Routes.MapHttpRoute(
                name: "Get Content",
                routeTemplate: "api/{controller}/{token}/{id}/{imageWidth}",
                defaults: new { controller = "Content" },
                constraints: new { controller = "Content" }
            );
            config.Routes.MapHttpRoute(
                name: "Get Content2",
                routeTemplate: "api/{controller}/{token}/{id}/{imageWidth}/{listType}",
                defaults: new { controller = "Content" },
                constraints: new { controller = "Content" }
            );

            // Post/Put/Delete Content
            config.Routes.MapHttpRoute(
                name: "Post/Put/Delete Content",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "Content" },
                constraints: new { controller = "Content" }
            );

            // Publish Content (Make Active)
            config.Routes.MapHttpRoute(
                name: "Publish Content",
                routeTemplate: "api/{controller}/{token}/{id}/{publish}",
                defaults: new { controller = "ContentPublish" },
                constraints: new { controller = "ContentPublish" }
            );

            // Get Simple Content
            config.Routes.MapHttpRoute(
                name: "Get Simple Content",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "SimpleContent" },
                constraints: new { controller = "SimpleContent" }
            );

            // Get List Content
            config.Routes.MapHttpRoute(
                name: "Get List Content",
                routeTemplate: "api/{controller}/{token}/{type}/{listType}",
                defaults: new { controller = "ContentList" },
                constraints: new { controller = "ContentList" }
            );

            // Get Company Simple Content
            config.Routes.MapHttpRoute(
                name: "Get Company Simple Content",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}/{includeCategories}/{imageWidth}",
                defaults: new { controller = "CompanySimpleContent" },
                constraints: new { controller = "CompanySimpleContent" }
            );
            config.Routes.MapHttpRoute(
                name: "Get Company Simple Content 2",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}/{includeCategories}/{imageWidth}/{priceType}",
                defaults: new { controller = "CompanySimpleContent" },
                constraints: new { controller = "CompanySimpleContent" }
            );

            // Get SubContent
            config.Routes.MapHttpRoute(
                name: "Get SubContent",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "SubContent" },
                constraints: new { controller = "SubContent" }
            );

            // Post/Put/Delete SubContent
            config.Routes.MapHttpRoute(
                name: "Post/Put/Delete SubContent",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "SubContent" },
                constraints: new { controller = "SubContent" }
            );

            // Get SubContentGroup
            config.Routes.MapHttpRoute(
                name: "Get SubContentGroup",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "SubContentGroup" },
                constraints: new { controller = "SubContentGroup" }
            );
            // Get Customer Content
            config.Routes.MapHttpRoute(
                name: "Get CustomerContent Public",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{contentTemplateTypeId}/{customerContentTypeId}/{list}/{imageWidth}",
                defaults: new { controller = "CustomerContent", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContent" }
            );

            config.Routes.MapHttpRoute(
                name: "Get CustomerContent Private",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{contentTemplateTypeId}/{customerContentTypeId}/{list}/{imageWidth}/{customerId}",
                defaults: new { controller = "CustomerContent", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContent" }
            );

            config.Routes.MapHttpRoute(
                name: "Get CustomerContent Private Get One",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{contentTemplateTypeId}/{customerContentTypeId}/{list}/{imageWidth}/{customerId}/{contentId}",
                defaults: new { controller = "CustomerContent", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContent" }
            );

            // Customer Content Search
            config.Routes.MapHttpRoute(
                name: "Post CustomerContentSearch Public",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{contentTemplateTypeId}/{customerContentTypeId}/{list}/{imageWidth}",
                defaults: new { controller = "CustomerContentSearch", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContentSearch" }
            );

            config.Routes.MapHttpRoute(
                name: "Post CustomerContentSearch Private",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{contentTemplateTypeId}/{customerContentTypeId}/{list}/{imageWidth}/{customerId}",
                defaults: new { controller = "CustomerContentSearch", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContentSearch" }
            );

            // Customer Content Redeem
            config.Routes.MapHttpRoute(
                name: "CustomerContentRedeem",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}/{contentId}",
                defaults: new { controller = "CustomerContentRedeem", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContentRedeem" }
            );

            // Customer Content View
            config.Routes.MapHttpRoute(
                name: "CustomerContentView",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}/{contentId}",
                defaults: new { controller = "CustomerContentView", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContentView" }
            );

            // Customer Content Favorite
            config.Routes.MapHttpRoute(
                name: "CustomerContentFavorite",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}/{contentId}/{favorite}",
                defaults: new { controller = "CustomerContentFavorite", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContentFavorite" }
            );

            // Customer Content Ongoing
            config.Routes.MapHttpRoute(
                name: "CustomerContentOngoing",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}/{contentId}",
                defaults: new { controller = "CustomerContentOngoing", customerId = RouteParameter.Optional },
                constraints: new { controller = "CustomerContentOngoing" }
            );

            // Get ContentCategories
            config.Routes.MapHttpRoute(
                name: "Get ContentCategories",
                routeTemplate: "api/{controller}/{token}/{id}/{imageWidth}",
                defaults: new { controller = "ContentCategory" },
                constraints: new { controller = "ContentCategory" }
            );

            // Get ContentParentCategories
            config.Routes.MapHttpRoute(
                name: "Get ContentParentCategories",
                routeTemplate: "api/{controller}/{token}/{contentTemplateId}",
                defaults: new { controller = "ContentParentCategory" },
                constraints: new { controller = "ContentParentCategory" }
            );

            // Post/Put/Delete ContentCategory
            config.Routes.MapHttpRoute(
                name: "Post/Put/Delete ContentCategories",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "ContentCategory" },
                constraints: new { controller = "ContentCategory" }
            );

            // ContentVariantTemplate
            config.Routes.MapHttpRoute(
            name: "Get/Post/Put/Delete ContentVaraintTemplate",
            routeTemplate: "api/{controller}/{token}/{id}",
            defaults: new { controller = "ContentVariantTemplate" },
            constraints: new { controller = "ContentVariantTemplate" }
            );

            // ContentVariant
            config.Routes.MapHttpRoute(
            name: "Post ContentVariant",
            routeTemplate: "api/{controller}/{token}/{contentId}/{contentVariantTemplateId}",
            defaults: new { controller = "ContentVariant" },
            constraints: new { controller = "ContentVariant" }
            );

            // Get/Post/Put/Delete OrderPrinterAvailability
            config.Routes.MapHttpRoute(
                name: "Get/Post/Put/Delete OrderPrinterAvailability",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "OrderPrinterAvailability" },
                constraints: new { controller = "OrderPrinterAvailability" }
            );

            config.Routes.MapHttpRoute(
                name: "Get/Post OrderPrinterAvailability with Type",
                routeTemplate: "api/{controller}/{token}/{id}/{orderPrinterAvailabilityType}",
                defaults: new { controller = "OrderPrinterAvailability" },
                constraints: new { controller = "OrderPrinterAvailability" }
            );




            // Get CompanyOrderPrinterAvailability
            config.Routes.MapHttpRoute(
                name: "Get CompanyOrderPrinterAvailability",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}",
                defaults: new { controller = "CompanyOrderPrinterAvailability" },
                constraints: new { controller = "CompanyOrderPrinterAvailability" }
            );

            // Get CompanyOrderPrinterAvailabilityRaw
            config.Routes.MapHttpRoute(
                name: "Get CompanyOrderPrinterAvailabilityRaw",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}",
                defaults: new { controller = "CompanyOrderPrinterAvailabilityRaw" },
                constraints: new { controller = "CompanyOrderPrinterAvailabilityRaw" }
            );

            // Get CustomerOrderPrinterStatus
            config.Routes.MapHttpRoute(
                name: "Get CustomerOrderPrinterStatus",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}",
                defaults: new { controller = "CustomerOrderPrinterStatus" },
                constraints: new { controller = "CustomerOrderPrinterStatus" }
            );

            config.Routes.MapHttpRoute(
                name: "Get CustomerOrderPrinterStatus Customer",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}/{customerId}",
                defaults: new { controller = "CustomerOrderPrinterStatus" },
                constraints: new { controller = "CustomerOrderPrinterStatus" }
            );

            // Get CustomerOrderPrinterAvailabilityForDelivery
            config.Routes.MapHttpRoute(
                name: "Get CustomerOrderPrinterAvailabilityForDelivery",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}",
                defaults: new { controller = "CustomerOrderPrinterAvailabilityForDelivery" },
                constraints: new { controller = "CustomerOrderPrinterAvailabilityForDelivery" }
            );

            // Get/Post/Delete OrderPrinterZipCode
            config.Routes.MapHttpRoute(
                name: "Get/Post OrderPrinterZipCode",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "OrderPrinterZipCode" },
                constraints: new { controller = "OrderPrinterZipCode" }
            );

            config.Routes.MapHttpRoute(
                name: "Delete OrderPrinterZipCode",
                routeTemplate: "api/{controller}/{token}/{id}/{zipCode}",
                defaults: new { controller = "OrderPrinterZipCode" },
                constraints: new { controller = "OrderPrinterZipCode" }
            );

            // Get/Post/Delete OrderPrinterPostal
            config.Routes.MapHttpRoute(
                name: "Get/Post OrderPrinterPostal",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "OrderPrinterPostal" },
                constraints: new { controller = "OrderPrinterPostal" }
            );

            config.Routes.MapHttpRoute(
                name: "Delete OrderPrinterPostal",
                routeTemplate: "api/{controller}/{token}/{id}/{postalId}",
                defaults: new { controller = "OrderPrinterPostal" },
                constraints: new { controller = "OrderPrinterPostal" }
            );



            // Get CustomerOrderStatus
            config.Routes.MapHttpRoute(
                name: "Get CustomerOrderStatus",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{orderId}",
                defaults: new { controller = "CustomerOrderStatus" },
                constraints: new { controller = "CustomerOrderStatus" }
            );
            // Post CustomerOrderStatus
            config.Routes.MapHttpRoute(
                name: "Post CustomerOrderStatus",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{orderId}/{status}",
                defaults: new { controller = "CustomerOrderStatus" },
                constraints: new { controller = "CustomerOrderStatus" }
            );

            // Order
            config.Routes.MapHttpRoute(
                name: "Get Orders",
                routeTemplate: "api/{controller}/{token}/{id}/{take}",
                defaults: new { controller = "Order" },
                constraints: new { controller = "Order" }
            );

            config.Routes.MapHttpRoute(
                name: "Get Order",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "Order" },
                constraints: new { controller = "Order" }
            );

            // Order Search
            config.Routes.MapHttpRoute(
                name: "Search Orders",
                routeTemplate: "api/{controller}/{token}/{take}",
                defaults: new { controller = "OrderSearch" },
                constraints: new { controller = "OrderSearch" }
            );

            config.Routes.MapHttpRoute(
                name: "Search Orders 2",
                routeTemplate: "api/{controller}/{token}/{id}/{take}",
                defaults: new { controller = "OrderSearch" },
                constraints: new { controller = "OrderSearch" }
            );

            // Order Stat
            config.Routes.MapHttpRoute(
                name: "Get OrderStat",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "OrderStat" },
                constraints: new { controller = "OrderStat" }
            );

            // Order Search Stat
            config.Routes.MapHttpRoute(
                name: "Get OrderSearchStat",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "OrderSearchStat" },
                constraints: new { controller = "OrderSearchStat" }
            );


            // Get SCM orders
            config.Routes.MapHttpRoute(
                name: "Get SCM Orders",
                routeTemplate: "api/{controller}/{token}/{id}/{scmStatusId}/{take}",
                defaults: new { controller = "OrderSCM" },
                constraints: new { controller = "OrderSCM" }
            );

            // Order SCM Update status
            config.Routes.MapHttpRoute(
                name: "Update SCM status",
                routeTemplate: "api/{controller}/{token}/{orderGuid}/{scmStatusId}",
                defaults: new { controller = "OrderSCM" },
                constraints: new { controller = "OrderSCM" }
            );

            // Get SCM order
            config.Routes.MapHttpRoute(
                name: "Get SCM Order",
                routeTemplate: "api/{controller}/{token}/{orderGuid}",
                defaults: new { controller = "OrderSCM" },
                constraints: new { controller = "OrderSCM" }
            );

            // Postal
            config.Routes.MapHttpRoute(
                name: "Search CompanyPostal",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{value}",
                defaults: new { controller = "CompanyPostal" },
                constraints: new { controller = "CompanyPostal" }
            );
            // Postal Group
            config.Routes.MapHttpRoute(
                name: "Search by CompanyPostalGroup",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{value}",
                defaults: new { controller = "CompanyPostalGroup" },
                constraints: new { controller = "CompanyPostalGroup" }
            );
            // DeliveryFee By PostalGroup
            config.Routes.MapHttpRoute(
                name: "Get Company DeliveryFee By PostalGroup",
                routeTemplate: "api/{controller}/{secret}/{orderPrinterGuid}/{postalGroupGuid}",
                defaults: new { controller = "CompanyPostalGroupDeliveryFee" },
                constraints: new { controller = "CompanyPostalGroupDeliveryFee" }
            );
            // CompanyOrderPrinter
            config.Routes.MapHttpRoute(
                name: "Search CompanyOrderPrinter",
                routeTemplate: "api/{controller}/{secret}/{companyId}",
                defaults: new { controller = "CompanyOrderPrinter" },
                constraints: new { controller = "CompanyOrderPrinter" }
            );

            config.Routes.MapHttpRoute(
                name: "Get CompanyOrderPrinter",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}",
                defaults: new { controller = "CompanyOrderPrinter" },
                constraints: new { controller = "CompanyOrderPrinter" }
            );

            // Post/Get/Delete CustomerCart
            config.Routes.MapHttpRoute(
                name: "CustomerCart",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}",
                defaults: new { controller = "CustomerCart" },
                constraints: new { controller = "CustomerCart" }
            );

            config.Routes.MapHttpRoute(
                name: "CustomerCart 2",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}/{postalId}",
                defaults: new { controller = "CustomerCart" },
                constraints: new { controller = "CustomerCart" }
            );

            // Customer Invoice (GET/PUT)
            config.Routes.MapHttpRoute(
                name: "Get/Put CustomerInvoice",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}",
                defaults: new { controller = "CustomerInvoice" },
                constraints: new { controller = "CustomerInvoice" }
            );

            // Post Customer Order (Create)
            config.Routes.MapHttpRoute(
                name: "Create CustomerOrder",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}/{id}",
                defaults: new { controller = "CustomerOrder" },
                constraints: new { controller = "CustomerOrder" }
            );

            // Get Customer orders
            config.Routes.MapHttpRoute(
                name: "Get CustomerOrder",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}",
                defaults: new { controller = "CustomerOrder" },
                constraints: new { controller = "CustomerOrder" }
            );

            // Get Customer orders
            config.Routes.MapHttpRoute(
                name: "Get CustomerOrderDetail",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{orderId}",
                defaults: new { controller = "CustomerOrderDetail" },
                constraints: new { controller = "CustomerOrderDetail" }
            );


            // Get Customer orders raw
            config.Routes.MapHttpRoute(
                name: "Get CustomerOrderRaw",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}",
                defaults: new { controller = "CustomerOrderRaw" },
                constraints: new { controller = "CustomerOrderRaw" }
            );

            config.Routes.MapHttpRoute(
                name: "Get CustomerOrderRaw Receipt",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}/{orderId}",
                defaults: new { controller = "CustomerOrderRaw" },
                constraints: new { controller = "CustomerOrderRaw" }
            );

            // Post Customer Instant Order (Create)
            config.Routes.MapHttpRoute(
                name: "Create CustomerInstantOrder",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}",
                defaults: new { controller = "CustomerInstantOrder" },
                constraints: new { controller = "CustomerInstantOrder" }
            );

            // Add CustomerContentAvailability
            config.Routes.MapHttpRoute(
                name: "Get CompanyCustomerContentAvailability",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}",
                defaults: new { controller = "CompanyCustomerContentAvailability" },
                constraints: new { controller = "CompanyCustomerContentAvailability" }
            );

            // Post Company Customer
            config.Routes.MapHttpRoute(
                name: "CompanyCustomer",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{customerId}",
                defaults: new { controller = "CompanyCustomer" },
                constraints: new { controller = "CompanyCustomer" }
            );

            // Get CompanyContent
            config.Routes.MapHttpRoute(
                name: "CompanyContent",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}/{imageWidth}",
                defaults: new { controller = "CompanyContent" },
                constraints: new { controller = "CompanyContent" }
            );
            config.Routes.MapHttpRoute(
                name: "CompanyContent 2",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}/{imageWidth}/{priceType}", //{includedSubContentOwing}
                defaults: new { controller = "CompanyContent" },
                constraints: new { controller = "CompanyContent" }
            );

            // Get CompanySubContent
            config.Routes.MapHttpRoute(
                name: "CompanyContentDetails",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{id}/{imageWidth}/{priceType}",
                defaults: new { controller = "CompanyContentDetails" },
                constraints: new { controller = "CompanyContentDetails" }
            );





            // Get CompanySimpleContentParentCategory
            config.Routes.MapHttpRoute(
                name: "CompanySimpleContentParentCategory",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{contentTemplateId}",
                defaults: new { controller = "CompanySimpleContentParentCategory" },
                constraints: new { controller = "CompanySimpleContentParentCategory" }
            );

            // Get CompanySimpleContentCategory
            config.Routes.MapHttpRoute(
                name: "CompanySimpleContentCategory",
                routeTemplate: "api/{controller}/{secret}/{companyId}/{contentParentCategoryId}",
                defaults: new { controller = "CompanySimpleContentCategory" },
                constraints: new { controller = "CompanySimpleContentCategory" }
            );


            // Customer Search
            config.Routes.MapHttpRoute(
                name: "Search Customers",
                routeTemplate: "api/{controller}/{token}/{take}",
                defaults: new { controller = "CustomerSearch" },
                constraints: new { controller = "CustomerSearch" }
            );

            // Payment
            config.Routes.MapHttpRoute(
                name: "Get Payments",
                routeTemplate: "api/{controller}/{token}/{orderPrinterId}/{take}",
                defaults: new { controller = "Payment" },
                constraints: new { controller = "Payment" }
            );

            config.Routes.MapHttpRoute(
                name: "Get Payment",
                routeTemplate: "api/{controller}/{token}/{id}",
                defaults: new { controller = "Payment" },
                constraints: new { controller = "Payment" }
            );

            // Dashboard (Global)
            config.Routes.MapHttpRoute(
                name: "Get Dashboard Global",
                routeTemplate: "api/{controller}/{token}/{type}",
                defaults: new { controller = "Dashboard" },
                constraints: new { controller = "Dashboard" }
            );

            // Dashboard (Menu)
            config.Routes.MapHttpRoute(
                name: "Get Dashboard ContentCategory",
                routeTemplate: "api/{controller}/{token}/{type}/{contentCategory}",
                defaults: new { controller = "Dashboard" },
                constraints: new { controller = "Dashboard" }
            );

            // User
            config.Routes.MapHttpRoute(
                name: "CRUD User",
                routeTemplate: "api/{controller}/{secret}/{companyId}",
                defaults: new { controller = "CompanyUser" },
                constraints: new { controller = "CompanyUser" }
            );



            // Partner Unique

            // MyClubCompany
            config.Routes.MapHttpRoute(
                name: "Post/Put/Delete MyClubCompany",
                routeTemplate: "api/{controller}/{secret}/{id}",
                defaults: new { controller = "MyClubCompany" },
                constraints: new { controller = "MyClubCompany" }
            );

            // ActivateMyClubCompany
            config.Routes.MapHttpRoute(
                name: "Get ActivateMyClubCompany",
                routeTemplate: "api/{controller}/{secret}/{id}",
                defaults: new { controller = "ActivateMyClubCompany" },
                constraints: new { controller = "ActivateMyClubCompany" }
            );


            // Enjoy Registration
            config.Routes.MapHttpRoute(
                name: "EnjoyRegistration",
                routeTemplate: "api/{controller}/{secret}/{companyId}",
                defaults: new { controller = "EnjoyRegistration" },
                constraints: new { controller = "EnjoyRegistration" }
            );
        }
    }
}

