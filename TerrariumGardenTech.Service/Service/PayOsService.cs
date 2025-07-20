using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Payment;
using TerrariumGardenTech.Service.ResponseModel.Order;

namespace TerrariumGardenTech.Service.Service;

public class PayOsService : IPayOsService
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IMapper _mapper;
    private readonly PayOS _payOS;
    private readonly UnitOfWork _unitOfWork;

    public PayOsService(IHttpContextAccessor httpContextAccessor,
        UnitOfWork unitOfWork, IConfiguration config, IMapper mapper)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _payOS = new PayOS(
            config["PayOS:ClientId"],
            config["PayOS:ApiKey"],
            config["PayOS:ChecksumKey"]
        );
        _config = config;
        _mapper = mapper;
    }

    public async Task<IBusinessResult> CreatePaymentLink(int orderId, string description)
    {
        var order = await _unitOfWork.OrderRepository.GetByIdWithOrderItemsAsync(orderId);
        if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

        var itemDatas = new List<ItemData>();
        var amount = 0;
        foreach (var orderItem in order.OrderItems)
        {
            itemDatas.Add(new ItemData(orderItem.TerrariumVariant.VariantName, orderItem.Quantity ?? 0,
                (int)(orderItem.UnitPrice ?? 0)));
            amount += (int)(orderItem.TotalPrice ?? 0);
        }

        var domain =
            $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/api";
        var successReturnUrl =
            $"{domain}/Payment/callback?orderId={orderId}&statusReturn={PaymentReturnModelStatus.Success}";
        var failReturnUrl = $"{domain}/Payment/callback?orderId={orderId}&statusReturn={PaymentReturnModelStatus.Fail}";
        var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

        var signatureData =
            $"amount={order.TotalAmount}&cancelUrl={failReturnUrl}&description={description}&orderCode={orderCode}&returnUrl={successReturnUrl}";
        var signature = CreateHmacSha256(signatureData, _config["PayOS:ChecksumKey"]);

        var paymentLinkRequest = new PaymentData(
            orderCode,
            amount,
            description,
            itemDatas,
            returnUrl: successReturnUrl,
            cancelUrl: failReturnUrl,
            expiredAt: (int)DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
            signature: signature
        );
        var response = await _payOS.createPaymentLink(paymentLinkRequest);
        return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Success", response.checkoutUrl);
    }

    public async Task<IBusinessResult> ProcessPaymentCallback(PaymentReturnModel returnModel)
    {
        try
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(returnModel.OrderId);
            if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

            order.PaymentStatus = returnModel.Status;
            await _unitOfWork.OrderRepository.UpdateAsync(order);
            await _unitOfWork.SaveAsync();
            var orderResponse = _mapper.Map<OrderResponse>(order);
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, orderResponse);
        }
        catch (Exception e)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, e.Message);
        }
    }

    private static string CreateHmacSha256(string data, string? key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}