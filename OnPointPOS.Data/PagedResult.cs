﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSSUM.Data
{
    public class PagedResult<TEntity>
    {
        public IList<TEntity> Results { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }       
    }
}
