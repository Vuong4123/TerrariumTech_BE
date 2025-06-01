using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common
{
    public class Const
    {
        #region Error Codes

        public static int ERROR_EXCEPTION = -4;

        #endregion

        #region Success Codes

        public static int SUCCESS_CREATE_CODE = 201;
        public static string SUCCESS_CREATE_MSG = "Save data success";
        public static int SUCCESS_READ_CODE = 200;
        public static string SUCCESS_READ_MSG = "Get data success";
        public static int SUCCESS_UPDATE_CODE = 200;
        public static string SUCCESS_UPDATE_MSG = "Update data success";
        public static int SUCCESS_DELETE_CODE = 200;
        public static string SUCCESS_DELETE_MSG = "Delete data success";

        #endregion

        #region Fail code

        public static int FAIL_CREATE_CODE = -1;
        public static string FAIL_CREATE_MSG = "Save data fail";
        public static int FAIL_READ_CODE = -1;
        public static string FAIL_READ_MSG = "Get data fail";
        public static int FAIL_UPDATE_CODE = -1;
        public static string FAIL_UPDATE_MSG = "Update data fail";
        public static int FAIL_DELETE_CODE = -1;
        public static string FAIL_DELETE_MSG = "Delete data fail";

        #endregion

        #region Warning Code

        public static int WARNING_NO_DATA_CODE = 4;
        public static string WARNING_NO_DATA_MSG = "No data";

        #endregion

        #region Authentication & Authorization Codes

        public static int FAIL_LOGIN_CODE = -10;
        public static string FAIL_LOGIN_MSG = "Tên đăng nhập hoặc mật khẩu không đúng";

        public static int UNAUTHORIZED_CODE = 401;
        public static string UNAUTHORIZED_MSG = "Chưa xác thực, vui lòng đăng nhập";

        public static int FORBIDDEN_CODE = 403;
        public static string FORBIDDEN_MSG = "Bạn không có quyền truy cập tài nguyên này";

        public static int TOKEN_EXPIRED_CODE = -11;
        public static string TOKEN_EXPIRED_MSG = "Token đã hết hạn";

        #endregion
    }
}
