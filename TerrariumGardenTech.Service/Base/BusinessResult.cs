using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.Base
{
    public interface IBusinessResult
    {
        int Status { get; set; }
        string? Message { get; set; }
        object? Data { get; set; }

    }

    public class BusinessResult : IBusinessResult
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }

        public BusinessResult()
        {
            Status = -1;
            Message = "Action fail";
        }

        public BusinessResult(int status, string message)
        {
            Status = status;
            Message = message;
        }

        public BusinessResult(int status, string message, object data)
        {
            Status = status;
            Message = message;
            Data = data;
        }
    }
}
