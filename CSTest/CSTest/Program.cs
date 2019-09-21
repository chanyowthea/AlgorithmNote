using GCommon;
using SharpZipLibUse;
using System;
using System.Collections.Generic;
using System.IO;
using Compression;

namespace CSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Class1.Start();

            string str = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<localization>
  <l id='900_PLATFORM_INVALID_MSDK' type='text' en='mSDK authentication error' zh-tw='驗證失敗' th='mSDK ผิดพลาด' vn='Lỗi xác thực mSDK' ind='Error otentikasi mSDK' zh-cn='验证失败' pt-br='Erro de autenticação do mSDK' es='Error de autenticación de mSDK' ru='Ошибка идентификации mSDK' ko='mSDK 권한 오류' de='mSDK-Authentifizierungsfehler' fr='mSDK authentication error' tr='mSDK kimlik doğrulama başarısız' ja='認証失敗' zh_HK='1000' ar='خطأ في مصادقة mSDK' />
  <l id='BR_ACCOUNT_CHOOSE_INVALID_REGION' type='text' en='Invalid region, please contact customer service.' zh-tw='區域錯誤，請聯繫客服人員。' th='ภูมิภาคผิดพลาด กรุณาติดต่อฝ่ายบริการลูกค้า' vn='Khu vực không hợp lệ, vui lòng liên hệ với dịch vụ chăm sóc khách hàng.' ind='Region tidak valid, silahkan kontak customer service.' zh-cn='Invalid Region. Please contact customer service' pt-br='Região inválida, entre em contato com o suporte ao cliente.' es='Región inválida' ru='Неверный регион. Пожалуйста, обратитесь в центр техподдержки.' ko='유효하지 않은 지역. 고객 센터로 문의 주시기 바랍니다.' de='Ungültige Region, bitte kontaktieren Sie den Kundenservice.' fr='Région invalide, veuillez contacter le service client.' tr='Geçersiz bölge, lütfen müşteri hizmetlerine bağlan.' ja='地域判定無効。カスタマーサービスに連絡してください' zh_HK='1000' ar='إقليم غير صالح، يرجى الاتصال بخدمة العملاء.' />
  </localization>";
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);
            Console.WriteLine("origin=" + byteArray.Length);
            var dest = CompressionHelper.Compress(byteArray);
            Console.WriteLine("dest=" + dest.Length);
            var dec = CompressionHelper.DeCompress(dest);
            Console.WriteLine("dec=" + dec.Length);
            Console.WriteLine(System.Text.Encoding.Default.GetString(dec));
            Console.ReadKey();
        }
    }
}
