using Caliburn.Micro;
using Notifications.Wpf;
using Notifications.Wpf.Controls;
using POSSUM.Handlers;
using POSSUM.Model;
using POSSUM.Presenters.Login;
using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using NotificationManager = POSSUM.Handlers.NotificationManager;
using NotificationManager = Notifications.Wpf.NotificationManager;

namespace POSSUM.ViewModels
{
    public class NotificationViewModel : INotifyPropertyChanged
    {
        MainWindow _mainWindow;
        LoginPresenter _loginPresenter;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly INotificationManager _manager;

        public NotificationViewModel(INotificationManager manager, MainWindow mainWindow = null, LoginPresenter loginPresenter = null)
        {
            _manager = manager;
            AcceptCommand = new RelayCommand(AcceptOrder);
            RejectCommand = new RelayCommand(RejectOrder);
            CloseCommand = new RelayCommand(CloseWindow);
            _mainWindow = mainWindow;
            _loginPresenter = loginPresenter;
        }

        public async void AcceptOrder(object obj)
        {
            var customer = new Customer
            {
                Name = CustomerName,
                Phone = PhoneNo,
                Address1 = Address,
                Email = Email,
            };

            if (IsOnlineOrder)
            {
                _mainWindow.UpdateOnlineOrderStatus(this.OrderId, this.Id, true);
            }
            else if (IsPosMini)
            {
                //order.OrderLines = order.OrderLines;
                _mainWindow.UpdatePosMiniOrderStatus(this.OrderId, this.Id, true);
                Order.Status = OrderStatus.AssignedKitchenBar;
                MQTTHandler.SavePauseOrder(this.Order);
                _mainWindow.PostPosMiniOrderOrderToMqtt(this.OrderId);
            }
            else
            {
                Model.Order order = new Model.Order();
                order.Id = Guid.Parse(this.OrderId);
                order.DeliveryDate = DeliveryDate == null ? DateTime.Now : DeliveryDate;
                order.OrderLines = GetCartItems(this.CartItems);
                new DirectPrint().PrintFoodOrderBong(order, true, customer);
                _mainWindow.UpdateOrderStatus(this.OrderId, this.Id, true);
            }
        }

        public async void RejectOrder(object obj)
        {
            if (IsOnlineOrder)
            {
                _mainWindow.UpdateOnlineOrderStatus(this.OrderId, this.Id, false);
            }
            else if (IsPosMini)
            {
                _mainWindow.UpdatePosMiniOrderStatus(this.OrderId, this.Id, false);
                Order.Status = OrderStatus.Rejected;
                Order.Updated = 1;
                Order.Comments = Order.Comments + " Rejected from Terminal";
                MQTTHandler.SavePauseOrder(this.Order);
                _mainWindow.PostPosMiniOrderOrderToMqtt(this.OrderId);
            }
            else
                _mainWindow.UpdateOrderStatus(this.OrderId, this.Id, false);
        }

        public async void CloseWindow(object obj)
        {

        }

        public void ShowCloseButtons()
        {
            Title = "Order is cancelled";
            AcceptVisibility = Visibility.Collapsed;
            RejectVisibility = Visibility.Collapsed;
            CloseVisibility = Visibility.Visible;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _Title { get; set; }
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
                OnPropertyChanged("Title");
            }
        }
        public string Message { get; set; }
        public string Id { get; set; }
        public string OrderId { get; set; }
        public decimal OrderTotal { get; set; }
        public bool IsPosMini { get; set; }
        public bool IsOnlineOrder { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public decimal Tax { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public POSSUM.Model.Order Order { get; set; }
        public List<CartItem> CartItems { get; set; }
        public List<POSSUM.Model.OrderLine> OrderLines
        {
            get
            {
                return Order.OrderLines.Where(o => o.Active == 1 && o.ItemType != ItemType.Ingredient).Select(s =>
                  new OrderLine
                  {
                      Quantity = Math.Round(s.Quantity, 2),
                      UnitPrice = Math.Round(s.UnitPrice, 2),
                      Product = s.Product,
                      Ingredients = Order.OrderLines.Where(o => o.GroupKey == s.Id && o.Active == 1 && o.ItemType == ItemType.Ingredient).Select(i =>
                            new OrderLine
                            {
                                Quantity = Math.Round(i.Quantity, 2),
                                UnitPrice = Math.Round(i.UnitPrice, 2),
                                Product = i.Product,
                            }).ToList()
                  }).ToList();
            }
        }
        public List<OrderLine> GetCartItems(List<CartItem> cartItems)
        {

            List<OrderLine> cartitems = new List<OrderLine>();
            foreach (var items in cartItems)
            {
                var newCrtitem = new OrderLine
                {
                    Id = Guid.Parse(items.CartId),
                    ItemId = Guid.Parse(items.CartId),
                    Description = items.Title,
                    UnitPrice = Convert.ToDecimal(items.Price),
                    TaxPercent = items.TaxPercent,
                    Quantity = Convert.ToInt16(items.Quantity),
                    Product = new Product()
                    {
                        Bong = true,
                        Description = items.Title,
                    },
                    IngredientItems = GetIngredientsItems(items.CartSubItems)
                };
                cartitems.Add(newCrtitem);

            }
            return cartitems;
        }
        public List<OrderLine> GetIngredientsItems(List<CartSubItem> cartItems)
        {

            List<OrderLine> cartitems = new List<OrderLine>();
            foreach (var items in cartItems)
            {
                var newCrtitem = new OrderLine
                {
                    Id = Guid.Parse(items.Id),
                    ItemId = Guid.Parse(items.Id),
                    Description = items.Title,
                    UnitPrice = Convert.ToDecimal(items.Price),
                    TaxPercent = items.TaxPercent,
                    Quantity = Convert.ToInt16(items.Quantity)
                };
                cartitems.Add(newCrtitem);

            }
            return cartitems;
        }

        public ICommand AcceptCommand { get; set; }
        public ICommand RejectCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        private Visibility _AcceptVisibility { get; set; }
        private Visibility _RejectVisibility { get; set; }
        private Visibility _CloseVisibility { get; set; }
        private Visibility _DeliveryDateVisibility { get; set; }
        private Visibility _CustomerNameVisibility { get; set; }
        private Visibility _CustomerPhoneVisibility { get; set; }
        private Visibility _CustomerEmailVisibility { get; set; }
        private Visibility _CustomerAddressVisibility { get; set; }

        public Visibility AcceptVisibility
        {
            get
            {
                return _AcceptVisibility;
            }
            set
            {
                _AcceptVisibility = value;
                OnPropertyChanged("AcceptVisibility");
            }
        }

        public Visibility RejectVisibility
        {
            get
            {
                return _RejectVisibility;
            }
            set
            {
                _RejectVisibility = value;
                OnPropertyChanged("RejectVisibility");
            }
        }

        public Visibility CloseVisibility
        {
            get
            {
                return _CloseVisibility;
            }
            set
            {
                _CloseVisibility = value;
                OnPropertyChanged("CloseVisibility");
            }
        }

        public Visibility DeliveryDateVisibility
        {
            get
            {
                return _DeliveryDateVisibility;
            }
            set
            {
                _DeliveryDateVisibility = value;
                OnPropertyChanged("DeliveryDateVisibility");
            }
        }

        public Visibility CustomerNameVisibility
        {
            get
            {
                return _CustomerNameVisibility;
            }
            set
            {
                _CustomerNameVisibility = value;
                OnPropertyChanged("CustomerNameVisibility");
            }
        }

        public Visibility CustomerPhoneVisibility
        {
            get
            {
                return _CustomerPhoneVisibility;
            }
            set
            {
                _CustomerPhoneVisibility = value;
                OnPropertyChanged("CustomerPhoneVisibility");
            }
        }

        public Visibility CustomerEmailVisibility
        {
            get
            {
                return _CustomerEmailVisibility;
            }
            set
            {
                _CustomerEmailVisibility = value;
                OnPropertyChanged("CustomerEmailVisibility");
            }
        }

        public Visibility CustomerAddressVisibility
        {
            get
            {
                return _CustomerAddressVisibility;
            }
            set
            {
                _CustomerAddressVisibility = value;
                OnPropertyChanged("CustomerAddressVisibility");
            }
        }

        public class RelayCommand : ICommand
        {
            private Action<object> execute;

            private Predicate<object> canExecute;

            private event EventHandler CanExecuteChangedInternal;

            public RelayCommand(Action<object> execute)
                : this(execute, DefaultCanExecute)
            {
            }

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                if (execute == null)
                {
                    throw new ArgumentNullException("execute");
                }

                if (canExecute == null)
                {
                    throw new ArgumentNullException("canExecute");
                }

                this.execute = execute;
                this.canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                    this.CanExecuteChangedInternal += value;
                }

                remove
                {
                    CommandManager.RequerySuggested -= value;
                    this.CanExecuteChangedInternal -= value;
                }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                this.execute(parameter);
            }

            public void OnCanExecuteChanged()
            {
                EventHandler handler = this.CanExecuteChangedInternal;
                if (handler != null)
                {
                    handler.Invoke(this, EventArgs.Empty);
                }
            }

            public void Destroy()
            {
                this.canExecute = _ => false;
                this.execute = _ => { return; };
            }

            private static bool DefaultCanExecute(object parameter)
            {
                return true;
            }
        }
    }


    public class Order
    {
        public string OrderId { get; set; }
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public bool Status { get; set; }
        public int OrderStatus { get; set; }
        public List<CartItem> CartItems { get; set; }
        public decimal Tax { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    public class CartItem// : List<CartSubItem>
    {
        public List<CartSubItem> CartSubItems = new List<CartSubItem>();
        public string CartId { get; set; }
        public string Title { get; set; }
        public string ContentVariant { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }
        public int ItemStatus { get; set; }
        public decimal ItemDisount { get; set; }
        public decimal TaxPercent { get; set; }
        public int Quantity { get; set; }
        public string IngredientMode { get; set; }
        public Guid ProductId { get; set; }
    }

    public class CartSubItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal Quantity { get; set; }
        public string IngredientMode { get; set; }
        public Guid ProductId { get; set; }
    }

    public class PossumAlive
    {
        public string Message { get; set; }
        public bool IsAlive { get; set; }
        public Guid Id { get; set; }
        public string Terminal { get; set; }
        public string OutletId { get; set; }
    }

    public class MobileOrderViewModel
    {
        public POSSUM.Model.Order Order { get; set; }
        public decimal AccountAmount { get; set; }
        public decimal TipAmount { get; set; }
        public string PaymentRefType { get; set; }

    }

}