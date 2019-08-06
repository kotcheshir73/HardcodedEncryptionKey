using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using WebApplicationHardcodedEncrypt.Models;

namespace WebApplicationHardcodedEncrypt.Controllers
{
    public class SecureController : Controller
    {
        private readonly string encryptionKey = "bGFrZHNsamthbGtqbGtzZGZrZGZzZGZzZGRyd2Vyd3I=";

        public ActionResult Index()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            using (SymmetricAlgorithm algorithm = GetSymmetricAlgorithm())
            {
                if (User.Identity.IsAuthenticated)
                {
                    var datas = context.Datas.Where(x => x.UserId == User.Identity.Name).ToList().Select(x => DecrypteText(algorithm, x.EncodingText));

                    return View(datas.ToList());
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(string text)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            using (SymmetricAlgorithm algorithm = GetSymmetricAlgorithm())
            {
                if (User.Identity.IsAuthenticated)
                {
                    context.Datas.Add(new DataModel
                    {
                        UserId = User.Identity.Name,
                        EncodingText = EncryptText(algorithm, text)
                    });

                    context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Getting an object of a cryptographic class
        /// </summary>
        /// <returns></returns>
        private SymmetricAlgorithm GetSymmetricAlgorithm()
        {
            SymmetricAlgorithm algorithm = SymmetricAlgorithm.Create("AES");

            byte[] keyBytes = Encoding.ASCII.GetBytes(Encoding.UTF8.GetString(Convert.FromBase64String(encryptionKey)));

            algorithm.Key = keyBytes;

            return algorithm;
        }

        /// <summary>
        /// Text encryption
        /// </summary>
        /// <param name="aesAlgorithm">cryptographic class</param>
        /// <param name="text">text</param>
        /// <returns></returns>
        private byte[] EncryptText(SymmetricAlgorithm aesAlgorithm, string text)
        {
            ICryptoTransform crypt = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, crypt, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(text);
                    }
                }
                byte[] encrypted = ms.ToArray();

                return encrypted.Concat(aesAlgorithm.IV).ToArray();
            }
        }

        /// <summary>
        /// Text decryption
        /// </summary>
        /// <param name="aesAlgorithm">cryptographic class</param>
        /// <param name="text">text</param>
        /// <returns></returns>
        private string DecrypteText(SymmetricAlgorithm aesAlgorithm, byte[] shifr)
        {
            byte[] bytesIv = new byte[16];

            for (int i = shifr.Length - 16, j = 0; i < shifr.Length; i++, j++)
            {
                bytesIv[j] = shifr[i];
            }
            aesAlgorithm.IV = bytesIv;

            byte[] mess = new byte[shifr.Length - 16];

            for (int i = 0; i < shifr.Length - 16; i++)
            {
                mess[i] = shifr[i];
            }

            byte[] data = mess;

            ICryptoTransform crypt = aesAlgorithm.CreateDecryptor(aesAlgorithm.Key, aesAlgorithm.IV);
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (CryptoStream cs = new CryptoStream(ms, crypt, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}