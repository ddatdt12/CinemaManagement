﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.DTOs
{
    public class ProductBillInfoDTO
    {
        public ProductBillInfoDTO()
        {

        }
        public int BillId { get; set; }
        public int ProductId { get; set; }
        public Nullable<int> Amount { get; set; }
        public decimal PricePerItem { get; set; }

        public virtual BillDTO Bill { get; set; }
        public virtual ProductDTO Product { get; set; }
    }
}