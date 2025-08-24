using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.Security
{
    public static class PasswordValidator
    {
        public static (bool IsValid, string Error) Validate(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (false, "Mật khẩu không được để trống");

            // 1) ≥ 9 ký tự
            if (password.Length < 9)
                return (false, "Mật khẩu phải có ít nhất 9 ký tự");

            // 2) ít nhất 1 ký tự đặc biệt (kể cả '_')
            if (!Regex.IsMatch(password, @"[\W_]"))
                return (false, "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt");

            // 3) ít nhất 1 chữ in hoa
            if (!Regex.IsMatch(password, "[A-Z]"))
                return (false, "Mật khẩu phải chứa ít nhất 1 chữ in hoa");

            // 4) ít nhất 1 số
            if (!Regex.IsMatch(password, "[0-9]"))
                return (false, "Mật khẩu phải chứa ít nhất 1 chữ số");

            // 5) không chứa khoảng trắng
            if (Regex.IsMatch(password, @"\s"))
                return (false, "Mật khẩu không được chứa khoảng trắng");

            // 6) không chứa chuỗi số liền mạch tăng dần (>=3): 123, 456, 6789,...
            if (HasAscendingDigitRun(password, 3))
                return (false, "Mật khẩu không được chứa chuỗi số liền mạch như 123, 456...");

            return (true, "Mật khẩu hợp lệ");
        }

        private static bool HasAscendingDigitRun(string s, int runLength)
        {
            int run = 1;
            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsDigit(s[i]) && char.IsDigit(s[i - 1]) && (s[i] - s[i - 1]) == 1)
                {
                    run++;
                    if (run >= runLength) return true;
                }
                else run = 1;
            }
            return false;
        }
    }
}
