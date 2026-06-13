using System;
using System.Collections.Generic;
using System.Text;

namespace Combince.Shared.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// String ifadeyi boşluklarından arındırır ve Türkçe/Kültür bağımsız kurallara göre güvenli bir şekilde küçük harfe çevirir.
        /// </summary>
        public static string ToLowerScope(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // Kültür uyuşmazlıklarını (I -> ı, i -> İ krizlerini) önlemek için invariant kullanıyoruz
            return value.Trim().ToLowerInvariant();
        }
    }
}
