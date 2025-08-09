using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Net.payOS.Types;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Config;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;

    public VnPayService(IHttpContextAccessor httpContextAccessor,
        UnitOfWork unitOfWork, IConfiguration config, IMapper mapper)
    {
        _configuration = config;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper;
    }

    public async Task<IBusinessResult> CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
    {
        try
        {
            var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(model.OrderId);
            if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

            // Kiểm tra nếu không có order items
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                return new BusinessResult(Const.NOT_FOUND_CODE, "No items found in order.");
            }

            var itemDatas = new List<ItemData>();
            var amount = 0;

            foreach (var orderItem in order.OrderItems)
            {
                // Kiểm tra và xử lý Accessory
                if (orderItem.AccessoryId.HasValue)
                {
                    itemDatas.Add(new ItemData(orderItem.Accessory.Name, orderItem.Quantity ?? 0,
                        (int)(orderItem.UnitPrice ?? 0)));
                    amount += (int)(orderItem.TotalPrice ?? 0);
                }

                // Kiểm tra và xử lý TerrariumVariant
                if (orderItem.TerrariumVariantId.HasValue)
                {
                    itemDatas.Add(new ItemData(orderItem.TerrariumVariant.VariantName, orderItem.Quantity ?? 0,
                        (int)(orderItem.UnitPrice ?? 0)));
                    amount += (int)(orderItem.TotalPrice ?? 0);
                }
            }


            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack =
                $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/api/Payment/vn-pay/callback";

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());
            // pay.AddRequestData("vnp_OrderId", model.OrderId.ToString());

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            if (string.IsNullOrEmpty(paymentUrl))
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Fail", paymentUrl);
            }

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Success", paymentUrl);
        }
        catch (Exception e)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "Fail", e.Message);

        }
    }

    public async Task<IBusinessResult> PaymentExecute(IQueryCollection collections)
    {
        var pay = new VnPayLibrary();
        var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

        // Kiểm tra nếu không thành công
        if (!response.Success)
        {
            return new BusinessResult(Const.FAIL_READ_CODE, null);
        }

        if (!int.TryParse(response.OrderId, out var orderId))
        {
            return new BusinessResult(Const.FAIL_READ_CODE, null);
        }

        var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
        if (order == null)
        {
            return new BusinessResult(Const.NOT_FOUND_CODE, null);
        }

        order.PaymentStatus = response.Success ? "PAID" : "FAILED";
        order.TransactionId = response.TransactionId;
        await _unitOfWork.Order.UpdateAsync(order);
        if (order.Payment == null || !order.Payment.Any())
        {
            order.Payment = new List<Payment>();
        }

        order.Payment.Add(new Payment
        {
            OrderId = order.OrderId,
            PaymentMethod = response.PaymentMethod,
            PaymentAmount = response.Amount / 100, // Convert from cents to the correct currency unit
            Status = response.Success ? "PAID" : "FAILED",
            PaymentDate = response.PaymentDate ?? DateTime.UtcNow, // Use payment date if available, otherwise use current date
        });
        await _unitOfWork.SaveAsync();

        var orderResponse = _mapper.Map<OrderResponse>(order);
        return new BusinessResult(
            response.Success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE,
            null,
            orderResponse
        );
    }


}