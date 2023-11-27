using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    [Serializable]
    public class Content : Result
    {
        public string ContentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public DateTime PublishDateTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime FreezeDateTime { get; set; }
        public DateTime TerminationDateTime { get; set; }
        public string Map { get; set; }
        public string Identifier { get; set; }
        public decimal Price { get; set; }
        public decimal DeliveryPrice { get; set; }
        public decimal VAT { get; set; }
        public int NoOfFreeSubContent { get; set; }
        public bool OpenNotRedeemable { get; set; }
        public string ContentType { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<PreviewUrl> PreviewUrl { get; set; }
        public int SortOrder { get; set; }
        public int Scale { get; set; }
        

       // public DB.ContentRepository.ContentListType ContentListType { get; set; }
        public List<dynamic> ContentDynamic { get; set; }
        public List<ContentCustom> ContentCustom { get; set; }
        public List<ContentCategory> ContentCategory { get; set; }
        public List<ContentVariant> ContentVariant { get; set; }
        public List<ContentSubContent> ContentSubContent { get; set; }
        public List<ContentPush> ContentPush { get; set; }

        public List<Company> Companies { get; set; }
    }

    public class PreviewUrl
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    [Serializable]
    public class ContentCustom
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }

    [Serializable]
    public class ContentList : Result
    {
        public string ContentId { get; set; }
        public string Name { get; set; }
    }


    [Serializable]
    public class SimpleContentList : Result
    {
        public string ContentCategoryName { get; set; }
        public string ContentCategoryDescription { get; set; }
        public List<SimpleContent> SimpleContent { get; set; }
        public int OrderPrinterAvailabilityType { get; set; }
        public bool Available { get; set; }
        public bool Orderable { get; set; }
    }

    [Serializable]
    public class SimpleContent
    {
        public string ContentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string IncludedSubContent { get; set; }
        public List<ContentCustom> ContentCustom { get; set; }
        public List<SimpleContentVariant> SimpleContentVariant { get; set; }
        public int Scale { get; set; }
        public bool Available { get; set; }
        public bool Orderable { get; set; }
        public string Identifier { get; set; }
        public decimal DiscountAmount { get; internal set; }
        public int StockQty { get; internal set; }
    }

    [Serializable]
    public class CompanyContent : Result
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }

        public string Variants { get; set; }
        public string SubContents { get; set; }

        public string Images { get; set; }


        //public List<ContentCustom> ContentCustom { get; set; }
    }

    [Serializable]
    public class CompanyContentDetail : Result
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }
        public int NoOfFreeSubContent { get; set; }
        

        public List<ContentDetailVariant> ContentDetailVariants { get; set; }
        public List<SubContentIncluded> SubContentIncludeds { get; set; }
        public List<SubContentExcluded> SubContentExcludeds { get; set; }
        public string SubContentGroupName { get; set; }
        public List<SubContentGrouped> SubContentGroupeds { get; set; }

        //public string Images { get; set; }
    }

    [Serializable] 
    public class ContentDetailVariant
    {
        public string ContentVariantId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool Default { get; set; }
    }

    [Serializable]
    public class SubContentIncluded
    {
        public string ContentId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Owing { get; set; }
        public string SpecialId { get; set; }
        public string SpecialName { get; set; }
        public bool NoOfFreeSubContent { get; set; }
    }

    [Serializable]
    public class SubContentExcluded
    {
        public string ContentId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    [Serializable]
    public class SubContentGrouped
    {
        public string ContentId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Max { get; set; }
        public bool Default { get; set; }
        public bool Mandatory { get; set; }
    }




}