using POSSUM.Model;
using POSSUM.Presenters.FoodTables;
using POSSUM.Res;
using POSSUM.Views.Sales;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace POSSUM.Views.FoodTables
{

    public partial class TableWindow : Window, IFoodTableView
    {
        //public string SelectedTableName = "Select Table";
        public bool isTakeaway = false;
        public bool IsNewOrder = false;
        FoodTablePresenter presenter;
        public FoodTable SelectedTable { get; set; }
        
        public List<FoodTable> tables = new List<FoodTable>();
        public TableWindow()
        {
            InitializeComponent();
            //SelectedTableName = UI.Sales_SelectTableButton;
            presenter = new FoodTablePresenter(this);
            presenter.LoadFloor();
            if (Defaults.ShowTableGrid)
            {
                TableCanvas.Visibility = Visibility.Collapsed;
                btnArrangeLayout.Visibility = Visibility.Collapsed;
                TablesOuterGrid.Visibility = Visibility.Visible;
                presenter.LoadTablesClick();
            }
            else
            {
                TableCanvas.Visibility = Visibility.Visible;
                btnArrangeLayout.Visibility = Visibility.Visible;
                TablesOuterGrid.Visibility = Visibility.Collapsed;
                DisplayTables(FloorId);
            }
        }
        public TableWindow(bool takeway)
        {
            InitializeComponent();
            TakeawayButton.Visibility = Visibility.Visible;
            presenter = new FoodTablePresenter(this);
            presenter.LoadFloor();
            if (Defaults.ShowTableGrid)
            {
                TableCanvas.Visibility = Visibility.Collapsed;
                btnArrangeLayout.Visibility = Visibility.Collapsed;
                TablesOuterGrid.Visibility = Visibility.Visible;
                presenter.LoadTablesClick();
            }
            else
            {
                TableCanvas.Visibility = Visibility.Visible;
                btnArrangeLayout.Visibility = Visibility.Visible;
                TablesOuterGrid.Visibility = Visibility.Collapsed;
                DisplayTables(FloorId);
            }
        }

        public void GetSelectedFloorColorBrush(Button btnFloor)
        {
            try
            {


                var defaultbrush = (Brush)new BrushConverter().ConvertFromString("#FFDCDEDE");
                var defaultForeground = (Brush)new BrushConverter().ConvertFromString("#000000");
                var defaulColor = new SolidColorBrush(((SolidColorBrush)defaultbrush).Color);
                btnFloor.Background = defaulColor;
                btnFloor.Foreground = defaultForeground;
                btnFloor.Background = defaulColor;
                btnFloor.Foreground = defaultForeground;
                var aa = STKFloors.Children;
                foreach (var a in aa)
                {
                    var btn = (a as Button);
                    if (btn != null)
                    {
                        if (btn.Name == btnFloor.Name)
                        {
                            var brush = (Brush)new BrushConverter().ConvertFromString("#FF007ACC");
                            btn.Background = new SolidColorBrush(((SolidColorBrush)brush).Color);
                            btn.Foreground = (Brush)new BrushConverter().ConvertFromString("#FFFFFF");
                        }
                        else
                        {

                            btn.Background = defaulColor;
                            btn.Foreground = defaultForeground;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }
        private void FillTablesListBox()
        {
            tables = presenter.GetTables(GetFloorId());
            var totalCustomers = tables.Count;
            if (totalCustomers < 42)
            {
                var addmoreItems = 42 - totalCustomers;
                for (var i = 0; i < addmoreItems; i++)
                {
                    tables.Add(new FoodTable
                    {
                        Id = 0,
                        Name = "",
                        ColorCode = "#FFDCDEDE"
                    });
                }
            }
            else if (totalCustomers > 42 && totalCustomers < 84)
            {
                var addmoreCustomers = 84 - totalCustomers;
                for (var i = 0; i < addmoreCustomers; i++)
                {
                    tables.Add(new FoodTable
                    {
                        Id = 0,
                        Name = "",
                        ColorCode = "#FFDCDEDE"
                    });
                }
            }
            else if (totalCustomers > 84 && totalCustomers < 126)
            {
                var addmoreCustomers = 126 - totalCustomers;
                for (var i = 0; i < addmoreCustomers; i++)
                {
                    tables.Add(new FoodTable
                    {
                        Id = 0,
                        Name = "",
                        ColorCode = "#FFDCDEDE"
                    });
                }
            }


            AddTablesToGrid(tables);
          
        }
        
        private void AddTablesToGrid(List<FoodTable> foodTables)
        {
            TableViewGrid.Children.Clear();
           // Style style = Application.Current.FindResource("KeyBoradButton") as Style;


            if (foodTables.Count < 7)
                TableViewGrid.Columns = foodTables.Count;
            else
                TableViewGrid.Columns = 7;
            foreach (var foodTable in foodTables)
            {
                var btn1 = new Button();
            //    btn1.Style = style;
                btn1.Background = Utilities.GetColorBrush(foodTable.ColorCode);
                btn1.Content = GetButtonContent(foodTable);
                btn1.Width = 109;
                btn1.Height = 51;
                btn1.Tag = foodTable.Id;
                btn1.DataContext = foodTable;
                TableViewGrid.Children.Add(btn1);
                btn1.Click += ButtonTable_Click;
            }
        }
        private StackPanel GetButtonContent(FoodTable foodTable)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            TextBlock txtBlock = new TextBlock();
            txtBlock.TextWrapping = TextWrapping.Wrap;
            txtBlock.Text = foodTable.Name;
            txtBlock.Width = 100;
            txtBlock.TextAlignment = TextAlignment.Center;
            txtBlock.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(txtBlock);

            return stackPanel;
        }
        private void ButtonTable_Click(object sender, RoutedEventArgs e)
        {

            var back = "" + (sender as Button).Content;
            if (back == "Back")
            {
                try
                {
                    var uc = new UCSale();
                    App.MainWindow.AddControlToMainCanvas(uc);
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    //LogWriter.LogWrite(ex);
                    MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var customer = (FoodTable)((Button)sender).DataContext;

                if (customer.Id == 0)
                    this.DialogResult = false;
                else
                {
                    SelectedTable = customer;
                    IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
                    this.DialogResult = true;
                }
            }

        }

        public void SetCustomerResult(List<Customer> customers)
        {

        }

        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void TakeawyButton_Click(object sender, RoutedEventArgs e)
        {
            isTakeaway = true;
            IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
            this.DialogResult = true;
        }

        public void SetFloors(List<Floor> floors)
        {
            STKFloors.Children.Clear();
            if (floors.Count > 1)
            {
                var brush = (Brush)new BrushConverter().ConvertFromString("#FF007ACC");
                var background = new SolidColorBrush(((SolidColorBrush)brush).Color);
                var foreground = (Brush)new BrushConverter().ConvertFromString("#FFFFFF");
                STKFloors.Visibility = Visibility.Visible;
                STKFloors.Width = 200;
                foreach (var floor in floors)
                {
                    Button floorButton = new Button();
                    floorButton.Name = "btnFloor" + floor.Id;
                    if (floor.Id == 1)
                    {
                        floorButton.Background = background;
                        floorButton.Foreground = foreground;
                    }
                    floorButton.Height = 40;
                    floorButton.Content = floor.Name;
                    floorButton.DataContext = floor;
                    floorButton.Click += FloorButton_Click;
                    STKFloors.Children.Add(floorButton);
                }
            }
            else
            {
                this.Width = this.Width - 225;
                STKFloors.Visibility = Visibility.Collapsed;
            }
        }
        public string GetKeyword()
        {
            return "";
        }
        int FloorId = 1;
        private void FloorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var floor = (sender as Button).DataContext as Floor;
                if (floor != null)
                {

                    FloorId = floor.Id;
                    if (Defaults.ShowTableGrid)
                        FillTablesListBox();
                    else
                        DisplayTables(FloorId);

                }
                GetSelectedFloorColorBrush((sender as Button));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
           
        }
        private void DisplayTables(int floorId)
        {
            btnArrangeLayout.Visibility = Visibility.Visible;
            tables = presenter.GetTables(floorId);
            TableCanvas.Children.Clear();
            ArrangeTable.Children.Clear();
            foreach (var table in tables)
            {
                Button btn = new Button();
                Canvas.SetLeft(btn, table.PositionY);
                Canvas.SetTop(btn, table.PositionX);
                btn.DataContext = table;
                if (table.OrderCount > 0)
                {
                    btn.Template = CreateTemplate("FoodTableRes.png", table.Name);
                }
                else
                {

                    btn.Template = CreateTemplate("FoodTable.png", table.Name);
                }
                btn.Click += Btn_Click;
                // btn.TouchUp += Btn_Click;
                TableCanvas.Children.Add(btn);


                var thumb = new MyThumb();

                thumb.DataContext = table;
                Canvas.SetLeft(thumb, table.PositionY);
                Canvas.SetTop(thumb, table.PositionX);
                if (table.OrderCount > 0)
                    thumb.Template = CreateTemplate("FoodTableRes.png", table.Name);
                else
                    thumb.Template = CreateTemplate("FoodTable.png", table.Name);

                // thumb.Template = CreateTemplate(table.ImageUrl, table.Name);
                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += Thumb_DragCompleted;
                thumb.DragStarted += Thumb_DragStarted;
                /*
                var template = CreateTemplate("FoodTable.png", table.Name);
                ContentControl cntrl = new ContentControl();
                cntrl.DataContext = table;
                cntrl.Height = 40;
                cntrl.Width = 60;
                cntrl.SetValue(Selector.IsSelectedProperty, true);
                cntrl.Style = Application.Current.FindResource("DesignerItemStyle") as Style;
                cntrl.Padding = new Thickness(5);
                Button bb = new Button();
                bb.Template = template;
                cntrl.Content = bb;
                Canvas.SetLeft(cntrl, table.PositionY);
                Canvas.SetTop(cntrl, table.PositionX);
               */
                ArrangeTable.Children.Add(thumb);
            }
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
           
        }

        private void Thumb_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var thumb = e.Source as MyThumb;
            //var transform = thumb.RenderTransform as RotateTransform;
            ////transform.Angle += 90;
            //thumb.rotateTransform = thumb.designerItem.RenderTransform as RotateTransform;
            //if (thumb.rotateTransform == null)
            //{
            //    thumb.designerItem.RenderTransform = new RotateTransform(0);
            //    thumb.initialAngle = 0;
            //}
            //else
            //{
            //    thumb.initialAngle = thumb.rotateTransform.Angle;
            //}
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var customer = (FoodTable)((Button)sender).DataContext;

            if (customer != null)
            {
                SelectedTable = customer;
                IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
                this.DialogResult = true;
            }
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var thumb = e.Source as MyThumb;
            FoodTable table = thumb.DataContext as FoodTable;
            var left = Canvas.GetLeft(thumb);// + e.HorizontalChange;
            var top = Canvas.GetTop(thumb);// + e.VerticalChange;
            table.PositionX = Convert.ToInt16(top);
            table.PositionY = Convert.ToInt16(left);
            presenter.UpdateLocation(table);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = e.Source as MyThumb;

            var left = Canvas.GetLeft(thumb) + e.HorizontalChange;
            var top = Canvas.GetTop(thumb) + e.VerticalChange;

            Canvas.SetLeft(thumb, left);
            Canvas.SetTop(thumb, top);
        }

        private static ControlTemplate CreateTemplate(string imageUrl, string name)
        {
            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = "roundTable.png";
            imageUrl = @"/POSSUM;component/images/" + imageUrl;

            string template =
        "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
             "<StackPanel>" +
                  "<Image HorizontalAlignment=\"Center\"   Source=\"" + imageUrl + "\" Stretch=\"Fill\" Width=\"65\" Height=\"45\"/>" +      
                "<TextBlock HorizontalAlignment=\"Center\" Text = \"" + name + "\" />" +
             "</StackPanel>" +
         "</ControlTemplate>";

     //string img= "<Image HorizontalAlignment=\"Center\"   Source=\"" + imageUrl + "\" Stretch=\"Fill\" Width=\"65\" Height=\"45\">" +
     // "<Image.RenderTransform>"+
     //   "<RotateTransform Angle = \"45\" />" +
     //"</Image.RenderTransform >" +
     // "</Image>";
            return (ControlTemplate)XamlReader.Parse(template);

        }
  
        private void popupTable_Loaded(object sender, RoutedEventArgs e)
        {
            PopupTable.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
        }

        private void ArrangeLayout_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = true;
        }

        private void OKLayout_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = false;
            DisplayTables(FloorId);
        }

        private void CancelLayout_Click(object sender, RoutedEventArgs e)
        {
            PopupTable.IsOpen = false;
        }

        private void Button_TouchLeave(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Button _button = (Button)sender as Button;
            if (_button != null && e.TouchDevice.Captured == _button)
            {
                _button.ReleaseTouchCapture(e.TouchDevice);
            }
            var back = "" + (sender as Button).Content;
            if (back == "Back")
            {
                try
                {
                    var uc = new UCSale();
                    App.MainWindow.AddControlToMainCanvas(uc);
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var customer = (FoodTable)((Button)sender).DataContext;

                if (customer.Id == 0)
                    this.DialogResult = false;
                else
                {
                    SelectedTable = customer;
                    IsNewOrder = chkNewOrder.IsChecked == true ? true : false;
                    this.DialogResult = true;
                }
            }
        }

        public void SetFoodTablesResult(List<FoodTable> tables)
        {
            this.tables = tables;
            FillTablesListBox();
        }

        public Order GetCurrentOrder()
        {
            throw new NotImplementedException();
        }

        public int GetFloorId()
        {
            return FloorId;
        }

        public void SetOrderMaster(Order order)
        {
            throw new NotImplementedException();
        }

        public List<Order> GetSelectedOrders()
        {
            throw new NotImplementedException();
        }

        public FoodTable GetSelectedTable()
        {
            throw new NotImplementedException();
        }

        public void SetTableOrderResult(List<Order> orders)
        {
            throw new NotImplementedException();
        }

        public void NewRecord(bool res)
        {
            throw new NotImplementedException();
        }
    }
    public class MyThumb : Thumb
    {
        public RotateTransform rotateTransform;
        public ContentControl designerItem;
        public double initialAngle;
    }

    public class RotateThumb : Thumb
    {
        private Point centerPoint;
        private Vector startVector;
        private double initialAngle;
        private Canvas designerCanvas;
        private ContentControl designerItem;
        private RotateTransform rotateTransform;

        public RotateThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.RotateThumb_DragDelta);
            DragStarted += new DragStartedEventHandler(this.RotateThumb_DragStarted);
        }

        private void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.designerItem = DataContext as ContentControl;

            if (this.designerItem != null)
            {
                this.designerCanvas = VisualTreeHelper.GetParent(this.designerItem) as Canvas;

                if (this.designerCanvas != null)
                {
                    this.centerPoint = this.designerItem.TranslatePoint(
                        new Point(this.designerItem.Width * this.designerItem.RenderTransformOrigin.X,
                                  this.designerItem.Height * this.designerItem.RenderTransformOrigin.Y),
                                  this.designerCanvas);

                    Point startPoint = Mouse.GetPosition(this.designerCanvas);
                    this.startVector = Point.Subtract(startPoint, this.centerPoint);

                    this.rotateTransform = this.designerItem.RenderTransform as RotateTransform;
                    if (this.rotateTransform == null)
                    {
                        this.designerItem.RenderTransform = new RotateTransform(0);
                        this.initialAngle = 0;
                    }
                    else
                    {
                        this.initialAngle = this.rotateTransform.Angle;
                    }
                }
            }
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.designerItem != null && this.designerCanvas != null)
            {
                Point currentPoint = Mouse.GetPosition(this.designerCanvas);
                Vector deltaVector = Point.Subtract(currentPoint, this.centerPoint);
              
                double angle = Vector.AngleBetween(this.startVector, deltaVector);

                RotateTransform rotateTransform = this.designerItem.RenderTransform as RotateTransform;
                rotateTransform.Angle = this.initialAngle + Math.Round(angle, 0);
                this.designerItem.InvalidateMeasure();
                var context=  (this.designerItem).DataContext as FoodTable;
                if (context != null)
                {
                    // presenter = new CustomerPresenter();
                    //context.Angle = rotateTransform.Angle;
                  
                }

            }
        }
    }

    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ContentControl designerItem = DataContext as ContentControl;

            if (designerItem != null)
            {
                Point dragDelta = new Point(e.HorizontalChange, e.VerticalChange);

                RotateTransform rotateTransform = designerItem.RenderTransform as RotateTransform;
                if (rotateTransform != null)
                {
                    dragDelta = rotateTransform.Transform(dragDelta);
                }

                Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) + dragDelta.X);
                Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) + dragDelta.Y);
            }
        }
    }
}
