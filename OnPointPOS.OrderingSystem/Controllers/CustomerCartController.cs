using ML.Common.Handlers.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;
using Models;
using System.Web.Http.Cors;

namespace ML.Rest2.Controllers
{
    //[EnableCors("*", "*", "*")]
    // [DisableCors]
    [EnableCors("*", "*", "PUT, POST, DELETE, GET")]

    public class CustomerCartController : ApiController
    {
        List<Cart> _mCarts = new List<Cart>();
        Cart _mCart = new Cart();

        public CustomerCartController()
        {
            _mCarts.Add(_mCart);
        }

        /// <summary>
        /// Add an item to cart
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// id = CustomerId (string)
        /// <returns></returns>
        public HttpResponseMessage Post(string secret, string companyId, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mCart.Status = RestStatus.NotFormData;
                _mCart.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
            {
                _mCart.Status = RestStatus.ParameterError;
                _mCart.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
            }

            CartItem cartItem = new CartItem();
            _mCart.CartItems.Add(cartItem);

            _mCart.TotalQuantity = 1;
            _mCart.TotalAmount = Convert.ToDecimal(35.0);
            _mCart.DeliveryFee = Convert.ToDecimal(0.0);
            _mCart.CustomerPaymentFee = Convert.ToDecimal(0.0);

            // Success
            _mCart.Status = RestStatus.Success;
            _mCart.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        }

        public HttpResponseMessage Get(string secret, string companyId, string id)
        {
            return Get(secret, companyId, id, Guid.Empty.ToString());
        }

        /// <summary>
        /// Get the cart
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="customerId"></param>
        /// id = CustomerId (string)
        /// postalId can also be ZipCode (ie: 12345)
        /// <returns></returns>        

        public HttpResponseMessage Get(string secret, string companyId, string id, string postalId)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
            {
                _mCart.Status = RestStatus.ParameterError;
                _mCart.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
            }


            // Get Cart
            CartItem cartItem = new CartItem();

            cartItem.CartId = "b9c3ec2f-a441-46c4-bd23-93cda5b8c050";
            cartItem.Title = "Margherita ";
            cartItem.ContentVariant = string.Empty;
            cartItem.Price = Convert.ToDecimal(10.0);
            cartItem.VAT = Convert.ToDecimal(0.0);
            _mCart.CartItems.Add(cartItem);

            CartSubItem cartSubItem = new CartSubItem();
            cartSubItem.Text = "+Vitloksås";
            cartSubItem.Price = Convert.ToDecimal(20.0);
            cartSubItem.VAT = Convert.ToDecimal(0.0);
            cartItem.CartSubItems.Add(cartSubItem);




            _mCart.TotalQuantity = 1;
            _mCart.TotalAmount = Convert.ToDecimal(35.0);
            _mCart.DeliveryFee = Convert.ToDecimal(0.0);
            _mCart.CustomerPaymentFee = Convert.ToDecimal(5.0);


            // Success
            _mCart.Status = RestStatus.Success;
            _mCart.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        }

        public HttpResponseMessage Delete(string secret, string companyId, string id)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
            {
                _mCart.Status = RestStatus.ParameterError;
                _mCart.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
            }

            Guid guidCompanyGuid = new Guid(companyId);
            // Totals
            _mCart.TotalAmount = Convert.ToDecimal(5.0);
            _mCart.TotalQuantity = 0;

            _mCart.CustomerPaymentFee = Convert.ToDecimal(0.0);
            _mCart.DeliveryFee = Convert.ToDecimal(0.0);
            // Success
            _mCart.Status = RestStatus.Success;
            _mCart.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        }

        //public HttpResponseMessage GetOld(string secret, string companyId, string id, string postalId)
        //{
        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
        //    {
        //        _mCart.Status = RestStatus.ParameterError;
        //        _mCart.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCart.Status = RestStatus.AuthenticationFailed;
        //        _mCart.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    // Lookup customer
        //    DB.CustomerRepository customerRepository = new DB.CustomerRepository();
        //    DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(new Guid(companyId), id);
        //    if (customer == null)
        //    {
        //        _mCart.Status = RestStatus.NotExisting;
        //        _mCart.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    // Get Cart
        //    List<DB.Receipt> receipts = new DB.CartService().GetCartReceiptData(customer.CustomerGuid);
        //    foreach (DB.Receipt receipt in receipts)
        //    {
        //        CartItem cartItem = new CartItem();
        //        cartItem.CartId = receipt.CartGuid.ToString();
        //        cartItem.Title = receipt.Title;
        //        cartItem.ContentVariant = receipt.ContentVariant;
        //        cartItem.Price = receipt.Price;
        //        cartItem.VAT = receipt.VAT;
        //        _mCart.CartItems.Add(cartItem);

        //        foreach (DB.ReceiptItem receiptItem in receipt)
        //        {
        //            CartSubItem cartSubItem = new CartSubItem();
        //            if (receiptItem.Strip && !receiptItem.Extra)
        //            {
        //                cartSubItem.Text = string.Format("-{0}", receiptItem.Text);
        //            }
        //            else if (!receiptItem.Strip && receiptItem.Extra)
        //            {
        //                cartSubItem.Text = string.Format("+{0}", receiptItem.Text);
        //            }
        //            else if (!receiptItem.Strip && !receiptItem.Extra)
        //            {
        //                cartSubItem.Text = string.Format("+{0}", receiptItem.Text);
        //            }
        //            cartSubItem.Price = receiptItem.Price;
        //            cartSubItem.VAT = receiptItem.VAT;
        //            cartItem.CartSubItems.Add(cartSubItem);
        //        }
        //    }

        //    // Totals
        //    DB.CartService cartService = new DB.CartService();
        //    _mCart.TotalQuantity = cartService.GetTotalQuantity(customer.CustomerGuid);
        //    if (ML.Common.Text.IsGuid(postalId))
        //    {
        //        _mCart.TotalAmount = cartService.GetTotals(customer.CustomerGuid, new Guid(postalId), customer.CompanyGuid);
        //    }
        //    else
        //    {
        //        _mCart.TotalAmount = cartService.GetTotalsByZipCode(customer.CustomerGuid, Convert.ToInt32(postalId), customer.CompanyGuid);
        //    }
        //    _mCart.DeliveryFee = cartService.DeliveryFee;
        //    _mCart.CustomerPaymentFee = cartService.CustomerPaymentFee;

        //    //if (cartService.CustomerPaymentFee > 0)
        //    //{
        //    //    _mCart.TotalAmount -= cartService.CustomerPaymentFee;
        //    //}

        //    // Success
        //    _mCart.Status = RestStatus.Success;
        //    _mCart.StatusText = "Success";
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //}



        /// <summary>
        /// Remove a cartitem / all cartitems from cart
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// id = CartId (Guid) or CustomerId to empty cart.
        /// <returns></returns>
        //public HttpResponseMessage Delete(string secret, string companyId, string id)
        //{
        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
        //    {
        //        _mCart.Status = RestStatus.ParameterError;
        //        _mCart.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    Guid guidCompanyGuid = new Guid(companyId);

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCart.Status = RestStatus.AuthenticationFailed;
        //        _mCart.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    // Remove Cart
        //    DB.tCart cart = new DB.CartRepository().GetByCartGuid(new Guid(id));
        //    if (cart != null)
        //    {
        //        // Remove item from Cart
        //        if (new DB.CartService().RemoveFromCart(cart.CartGuid) < 0)
        //        {
        //            _mCart.Status = RestStatus.GenericError;
        //            _mCart.StatusText = "Generic Error";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //        }

        //        // Totals
        //        _mCart.TotalAmount = new DB.CartService().GetTotals(cart.CustomerGuid, Guid.Empty, guidCompanyGuid);
        //        _mCart.TotalQuantity = new DB.CartService().GetTotalQuantity(cart.CustomerGuid);
        //    }
        //    else
        //    {
        //        // Remove Cart
        //        DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, id);
        //        if (customer != null)
        //        {
        //            new DB.CartService().EmptyCart(customer.CustomerGuid);
        //        }

        //        // Totals
        //        _mCart.TotalAmount = new DB.CartService().GetTotals(customer.CustomerGuid, Guid.Empty, guidCompanyGuid);
        //        _mCart.TotalQuantity = new DB.CartService().GetTotalQuantity(customer.CustomerGuid);
        //    }

        //    // Success
        //    _mCart.Status = RestStatus.Success;
        //    _mCart.StatusText = "Success";
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //}

        //public HttpResponseMessage GetOld(string secret, string companyId, string id, string postalId)
        //{
        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
        //    {
        //        _mCart.Status = RestStatus.ParameterError;
        //        _mCart.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCart.Status = RestStatus.AuthenticationFailed;
        //        _mCart.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    // Lookup customer
        //    DB.CustomerRepository customerRepository = new DB.CustomerRepository();
        //    DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(new Guid(companyId), id);
        //    if (customer == null)
        //    {
        //        _mCart.Status = RestStatus.NotExisting;
        //        _mCart.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    // Get Cart
        //    List<DB.Receipt> receipts = new DB.CartService().GetCartReceiptData(customer.CustomerGuid);
        //    foreach (DB.Receipt receipt in receipts)
        //    {
        //        CartItem cartItem = new CartItem();
        //        cartItem.CartId = receipt.CartGuid.ToString();
        //        cartItem.Title = receipt.Title;
        //        cartItem.ContentVariant = receipt.ContentVariant;
        //        cartItem.Price = receipt.Price;
        //        cartItem.VAT = receipt.VAT;
        //        _mCart.CartItems.Add(cartItem);

        //        foreach (DB.ReceiptItem receiptItem in receipt)
        //        {
        //            CartSubItem cartSubItem = new CartSubItem();
        //            if (receiptItem.Strip && !receiptItem.Extra)
        //            {
        //                cartSubItem.Text = string.Format("-{0}", receiptItem.Text);
        //            }
        //            else if (!receiptItem.Strip && receiptItem.Extra)
        //            {
        //                cartSubItem.Text = string.Format("+{0}", receiptItem.Text);
        //            }
        //            else if (!receiptItem.Strip && !receiptItem.Extra)
        //            {
        //                cartSubItem.Text = string.Format("+{0}", receiptItem.Text);
        //            }
        //            cartSubItem.Price = receiptItem.Price;
        //            cartSubItem.VAT = receiptItem.VAT;
        //            cartItem.CartSubItems.Add(cartSubItem);
        //        }
        //    }

        //    // Totals
        //    DB.CartService cartService = new DB.CartService();
        //    _mCart.TotalQuantity = cartService.GetTotalQuantity(customer.CustomerGuid);
        //    if (ML.Common.Text.IsGuid(postalId))
        //    {
        //        _mCart.TotalAmount = cartService.GetTotals(customer.CustomerGuid, new Guid(postalId), customer.CompanyGuid);
        //    }
        //    else
        //    {
        //        _mCart.TotalAmount = cartService.GetTotalsByZipCode(customer.CustomerGuid, Convert.ToInt32(postalId), customer.CompanyGuid);
        //    }
        //    _mCart.DeliveryFee = cartService.DeliveryFee;
        //    _mCart.CustomerPaymentFee = cartService.CustomerPaymentFee;

        //    //if (cartService.CustomerPaymentFee > 0)
        //    //{
        //    //    _mCart.TotalAmount -= cartService.CustomerPaymentFee;
        //    //}

        //    // Success
        //    _mCart.Status = RestStatus.Success;
        //    _mCart.StatusText = "Success";
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //}

        ///hello world
        //public HttpResponseMessage PostOld(string secret, string companyId, string id)
        //{
        //    if (!Request.Content.IsFormData())
        //    {
        //        _mCart.Status = RestStatus.NotFormData;
        //        _mCart.StatusText = "Not FormData";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
        //    {
        //        _mCart.Status = RestStatus.ParameterError;
        //        _mCart.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCart.Status = RestStatus.AuthenticationFailed;
        //        _mCart.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }

        //    // Lookup customer
        //    DB.CustomerRepository customerRepository = new DB.CustomerRepository();
        //    DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(new Guid(companyId), id);
        //    if (customer == null)
        //    {
        //        // Create new customer
        //        new DB.CustomerService().AddUpdateCustomer(
        //            new Guid(companyId)
        //            , string.Empty
        //            , Customer.Enums.CustomerType.Api
        //            , string.Empty
        //            , string.Empty
        //            , id
        //            );

        //        // Reget in context
        //        customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(new Guid(companyId), id);
        //    }


        //    // Prepare
        //    System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

        //    //StringBuilder sb = new StringBuilder();
        //    //foreach (var item in dic.AllKeys)
        //    //{
        //    //    sb.Append(item + ": " + dic.Get(item) + " ");
        //    //}
        //    //new ML.Email.Email().SendDebug("CustomerCartControllers.Post", "Collection: " + sb.ToString());

        //    // ContentGuid
        //    if (!ML.Common.Text.IsGuidNotEmpty(dic["ContentId"]))
        //    {
        //        _mCart.Status = RestStatus.DataMissing;
        //        _mCart.StatusText = "Data Missing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //    }
        //    Guid guidContentGuid = new Guid(dic["ContentId"]);

        //    // ContentVariantGuid
        //    Guid guidContentVariantGuid = Guid.Empty;
        //    if (ML.Common.Text.IsGuidNotEmpty(dic["ContentVariantId"]))
        //    {
        //        guidContentVariantGuid = new Guid(dic["ContentVariantId"]);
        //    }

        //    // subContentGuids
        //    string[] subContentGuidsSplit = null;
        //    if (!string.IsNullOrEmpty(dic["SubContentIds"]))
        //    {
        //        subContentGuidsSplit = dic["SubContentIds"].Split(',');
        //    }
        //    //try
        //    //{
        //    //    new ML.Email.Email().SendDebug("SubContentIds", dic["SubContentIds"]);
        //    //}
        //    //catch { }

        //    // includedSubContentGuids
        //    string[] includedSubContentGuidsSplit = null;
        //    if (!string.IsNullOrEmpty(dic["IncludedSubContentIds"]))
        //    {
        //        includedSubContentGuidsSplit = dic["IncludedSubContentIds"].Split(',');
        //    }
        //    //try
        //    //{
        //    //    new ML.Email.Email().SendDebug("IncludedSubContentIds", dic["IncludedSubContentIds"]);
        //    //}
        //    //catch { }

        //    // contentSpecialGuids
        //    string[] contentSpecialGuidsSplit = null;
        //    if (!string.IsNullOrEmpty(dic["ContentSpecialIds"]))
        //    {
        //        contentSpecialGuidsSplit = dic["ContentSpecialIds"].Split(',');
        //    }
        //    //try
        //    //{
        //    //    new ML.Email.Email().SendDebug("ContentSpecialIds", dic["ContentSpecialIds"]);
        //    //}
        //    //catch { }

        //    bool bSubContentOwing = false;
        //    DB.tContent content = new DB.ContentRepository().GetContent(guidContentGuid);
        //    if (content != null)
        //    {
        //        try
        //        {
        //            bSubContentOwing = (bool)content.IncludedSubContentOwing;
        //        }
        //        catch { }
        //    }
        //    //if (!string.IsNullOrEmpty(dic["OwingIds"]))
        //    //{
        //    //    bSubContentOwing = true;
        //    //}

        //    Guid guidPostalGuid = string.IsNullOrEmpty(dic["PostalId"]) ? Guid.Empty : new Guid(dic["PostalId"]);
        //    //try
        //    //{
        //    //    new ML.Email.Email().SendDebug("CustomerCartController:guidPostalGuid", guidPostalGuid.ToString());
        //    //}
        //    //catch { }


        //    // Handle Price Type
        //    DB.ContentRepository.PriceType priceType = DB.ContentRepository.PriceType.Regular;
        //    if (!string.IsNullOrEmpty(dic["PriceType"]))
        //    {
        //        priceType = (DB.ContentRepository.PriceType)Enum.Parse(typeof(DB.ContentRepository.PriceType), dic["PriceType"]);
        //    }
        //    //try
        //    //{
        //    //    new ML.Email.Email().SendDebug("CustomerCartController:guidPostalGuid", guidPostalGuid.ToString());
        //    //}
        //    //catch { }

        //    // Add main item
        //    Guid guidCartGuid = Guid.NewGuid();
        //    int intRc = new DB.CartService().AddToCart(guidCartGuid, customer.CustomerGuid, guidContentGuid, Guid.Empty, DB.CartService.Quantity.One, Guid.Empty, priceType, Guid.Empty, bSubContentOwing);

        //    if (intRc == 0 && guidContentVariantGuid != Guid.Empty)
        //    {
        //        intRc = new DB.CartService().AddToCart(Guid.NewGuid(), customer.CustomerGuid, guidContentGuid, guidCartGuid, DB.CartService.Quantity.One, guidContentVariantGuid, priceType, Guid.Empty, false);
        //    }

        //    // Add sub items
        //    if (intRc == 0)
        //    {
        //        // Additional sub content
        //        if (subContentGuidsSplit != null)
        //        {
        //            foreach (string strSubContentGuid in subContentGuidsSplit)
        //            {
        //                new DB.CartService().AddToCart(Guid.NewGuid(), customer.CustomerGuid, new Guid(strSubContentGuid), guidCartGuid, DB.CartService.Quantity.One, Guid.Empty, priceType, Guid.Empty, false);
        //            }
        //        }


        //        //try
        //        //{
        //        //    new ML.Email.Email().SendDebug("includedSubContentGuidsSplit 1", includedSubContentGuidsSplit.Count().ToString());
        //        //}
        //        //catch { }

        //        // Included sub content
        //        if (includedSubContentGuidsSplit != null)
        //        {
        //            //try
        //            //{
        //            //    new ML.Email.Email().SendDebug("includedSubContentGuidsSplit 2", includedSubContentGuidsSplit.Count().ToString());
        //            //}
        //            //catch { }

        //            // Handle Content Special
        //            foreach (string strIncludedSubContentGuid in includedSubContentGuidsSplit)
        //            {
        //                Guid guidContentSpecialGuid = Guid.Empty;

        //                if (contentSpecialGuidsSplit != null)
        //                {
        //                    foreach (string strContentSpecialGuid in contentSpecialGuidsSplit)
        //                    {
        //                        string[] contentSpecialGuidsSplit2 = strContentSpecialGuid.Split('x');
        //                        if (contentSpecialGuidsSplit2[0] == strIncludedSubContentGuid)
        //                        {
        //                            guidContentSpecialGuid = new Guid(contentSpecialGuidsSplit2[1]);
        //                            break;
        //                        }
        //                    }
        //                }

        //                new DB.CartService().AddToCart(Guid.NewGuid(), customer.CustomerGuid, new Guid(strIncludedSubContentGuid), guidCartGuid, DB.CartService.Quantity.Zero, Guid.Empty, priceType, guidContentSpecialGuid, bSubContentOwing);
        //            }
        //        }
        //    }

        //    // Totals
        //    //_mCart.TotalAmount = new DB.CartService().GetTotals(customer.CustomerGuid);
        //    //_mCart.TotalQuantity = new DB.CartService().GetTotalQuantity(customer.CustomerGuid);

        //    DB.CartService cartService = new DB.CartService();
        //    _mCart.TotalQuantity = cartService.GetTotalQuantity(customer.CustomerGuid);
        //    _mCart.TotalAmount = cartService.GetTotals(customer.CustomerGuid, guidPostalGuid, customer.CompanyGuid);
        //    _mCart.DeliveryFee = cartService.DeliveryFee;

        //    // Success
        //    _mCart.Status = RestStatus.Success;
        //    _mCart.StatusText = "Success";
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCarts));
        //}
    }
}
