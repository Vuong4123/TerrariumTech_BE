using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Address;
using TerrariumGardenTech.Common.ResponseModel.Address;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class AddressService(UnitOfWork _unitOfWork, IUserContextService userContextService) : IAddressService
{
    public async Task<IBusinessResult> GetAllAddresses()
    {
        // Lấy tất cả các địa chỉ từ cơ sở dữ liệu
        var addresses = await _unitOfWork.Address.GetAllAsync();

        // Kiểm tra nếu có dữ liệu
        if (addresses != null && addresses.Any())
        {
            // Ánh xạ Address thành AddressResponse
            var addressResponses = addresses.Select(t => new AddressResponse
            {
                Id = t.AddressId,
                TagName = t.TagName,
                UserId = t.UserId,
                ReceiverAddress = t.ReceiverAddress,
                ReceiverName = t.ReceiverName,
                ReceiverPhone = t.ReceiverPhone,
                IsDefault = t.IsDefault
            }).ToList();

            // Trả về kết quả với mã thành công
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, addressResponses);
        }

        // Trả về lỗi nếu không có dữ liệu
        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }

    public async Task<IBusinessResult> GetAddressById(int id)
    {
        // Lấy địa chỉ theo ID từ cơ sở dữ liệu
        var address = await _unitOfWork.Address.GetByIdAsync(id);

        // Kiểm tra nếu có dữ liệu
        if (address != null)
        {
            // Ánh xạ Address thành AddressResponse
            var addressResponse = new AddressResponse
            {
                Id = address.AddressId,
                TagName = address.TagName,
                UserId = address.UserId,
                ReceiverAddress = address.ReceiverAddress,
                ReceiverName = address.ReceiverName,
                ReceiverPhone = address.ReceiverPhone,
                IsDefault = address.IsDefault
            };

            // Trả về kết quả với mã thành công
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, addressResponse);
        }

        // Trả về lỗi nếu không có dữ liệu
        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }
    public async Task<IBusinessResult> GetAddressesByUserId(int userId)
    {
        // Lấy các địa chỉ theo userId từ cơ sở dữ liệu
        var addresses = await _unitOfWork.Address.GetByUserIdAsync(userId);

        // Kiểm tra nếu không có dữ liệu
        if (addresses == null || !addresses.Any())
        {
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }

        // Ánh xạ địa chỉ thành AddressResponse
        var addressResponses = addresses.Select(t => new AddressResponse
        {
            Id = t.AddressId,
            TagName = t.TagName,
            UserId = t.UserId,
            ReceiverAddress = t.ReceiverAddress,
            ReceiverName = t.ReceiverName,
            ReceiverPhone = t.ReceiverPhone,
            IsDefault = t.IsDefault
        }).ToList();

        // Trả về kết quả với dữ liệu đã ánh xạ
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, addressResponses);
    }

    public async Task<IBusinessResult> UpdateAddress(AddressUpdateRequest addressUpdateRequest)
    {
        try
        {
            var result = -1;
            var address = await _unitOfWork.Address.GetByIdAsync(addressUpdateRequest.Id);
            if (address != null)
            {
                _unitOfWork.Address.Context().Entry(address).CurrentValues.SetValues(addressUpdateRequest);
                result = await _unitOfWork.Address.UpdateAsync(address);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, address);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> CreateAddress(AddressCreateRequest addressCreateRequest)
    {
        try
        {
            var GetCurrentUser = userContextService.GetCurrentUser();
            var address = new Address
            {
                UserId = GetCurrentUser,
                TagName = addressCreateRequest.TagName,
                ReceiverAddress = addressCreateRequest.ReceiverAddress,
                ReceiverName = addressCreateRequest.ReceiverName,
                ReceiverPhone = addressCreateRequest.ReceiverPhone,
                IsDefault = addressCreateRequest.IsDefault,
            };
            var result = await _unitOfWork.Address.CreateAsync(address);
            if (address != null)
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, address);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteAddressById(int id)
    {
        var address = await _unitOfWork.Address.GetByIdAsync(id);
        if (address != null)
        {
            var result = await _unitOfWork.Address.RemoveAsync(address);
            if (result) return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);

            return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
        }

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }
    
    public async Task<IBusinessResult> Save(Address address)
    {
        try
        {
            var result = -1;
            var addressEntity = _unitOfWork.Address.GetByIdAsync(address.AddressId);
            if (addressEntity != null)
            {
                result = await _unitOfWork.Address.UpdateAsync(address);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, address);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            // Create new terrarium if it does not exist
            result = await _unitOfWork.Address.CreateAsync(address);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, address);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }
}