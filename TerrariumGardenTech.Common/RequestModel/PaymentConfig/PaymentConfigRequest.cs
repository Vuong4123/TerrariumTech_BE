using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.PaymentConfig;

public class PaymentConfigRequest
{
    public decimal DepositPercent { get; set; }
    public decimal FullPaymentDiscountPercent { get; set; }
    public decimal FreeshipAmount { get; set; }
    public decimal OrderAmount { get; set; }
    public string Description { get; set; } = string.Empty;
}
