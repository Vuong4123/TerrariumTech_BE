using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.PaymentConfig;

public class PaymentConfigResponse
{
    public decimal DepositPercent { get; set; }
    public decimal FullPaymentDiscountPercent { get; set; }
    public decimal FreeshipAmount { get; set; }
    public decimal OrderAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
