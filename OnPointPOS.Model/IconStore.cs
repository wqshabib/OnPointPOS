using System;

namespace POSSUM.Model
{
    public enum IconType
    {
        Category = 0,
        Logo = 1,
        Background = 2
    }

    public class IconStore : BaseEntity
    {
        public IconStore()
        {
            Created = DateTime.Now;
        }

        public int Id { get; set; }
        public IconType Type { get; set; }
        public string Title { get; set; }
        public byte[] Photo { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}