using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class Subscription
    {

        [Key]
        public Guid SubscriptionId { get; set; }

        public int id { get; set; }

        public int parent_id { get; set; }

        public string status { get; set; }

        public string currency { get; set; }

        public string version { get; set; }

        public bool prices_include_tax { get; set; }

        public DateTime date_created { get; set; }

        public DateTime date_modified { get; set; }

        public string discount_total { get; set; }

        public string discount_tax { get; set; }

        public string shipping_total { get; set; }

        public string shipping_tax { get; set; }

        public string cart_tax { get; set; }

        public string total { get; set; }

        public string total_tax { get; set; }

        public int customer_id { get; set; }

        public string order_key { get; set; }

        public string payment_method { get; set; }

        public string payment_method_title { get; set; }

        public string customer_ip_address { get; set; }

        public string customer_user_agent { get; set; }

        public string created_via { get; set; }

        public string customer_note { get; set; }

        public object date_completed { get; set; }

        public object date_paid { get; set; }

        public string number { get; set; }

        public DateTime date_created_gmt { get; set; }

        public DateTime? date_modified_gmt { get; set; }

        public DateTime? date_completed_gmt { get; set; }

        public DateTime? date_paid_gmt { get; set; }

        public string billing_period { get; set; }

        public string billing_interval { get; set; }

        public DateTime start_date_gmt { get; set; }

        public string trial_end_date_gmt { get; set; }

        public string next_payment_date_gmt { get; set; }

        public string last_payment_date_gmt { get; set; }

        public string cancelled_date_gmt { get; set; }

        public string end_date_gmt { get; set; }

        public string resubscribed_from { get; set; }

        public string resubscribed_subscription { get; set; }
        public int ProductId { get; set; }






    }




}