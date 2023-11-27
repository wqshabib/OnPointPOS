namespace POSSUM.Integration.CashDrawers.Dummy
{
    public class DummyCashDrawer : ICashDrawer
    {
        private bool dummy_status;

        public void Open()
        {
            dummy_status = true;
        }

        public bool IsOpen()
        {
            return dummy_status;
        }

        public void Close()
        {
            dummy_status = false;
        }
    }
}