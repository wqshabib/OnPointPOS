using POSSUM.Model;

namespace POSSUM.Events
{
    public delegate void UploadProductEventHandler(object sender, Product product,int categoryId);
    public delegate void DownloadProductEventHandler(object sender);

}
