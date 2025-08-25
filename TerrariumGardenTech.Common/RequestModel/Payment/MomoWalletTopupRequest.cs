using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Payment
{
	public class MomoWalletTopupRequest
	{
		public int UserId { get; set; }
		public long Amount { get; set; } // VND, MoMo expects integer VND amount
		public string? Description { get; set; }
	}
}


