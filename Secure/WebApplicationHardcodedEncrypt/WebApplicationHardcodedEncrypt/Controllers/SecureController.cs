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
        public ActionResult Index()
        {
            using (var context = new ApplicationDbContext())
            {
                using (var algorithm = GetSymmetricAlgorithm(context))
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        var datas = context.Datas
                            .Where(x => x.UserId == User.Identity.Name)
                            .ToList()
                            .Select(x => DecrypteText(algorithm, x.EncodingText))
                            .ToList();

                        return View(datas);
                    }
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Create(string text)
        {
            using (var context = new ApplicationDbContext())
            { using (var algorithm = GetSymmetricAlgorithm(context))
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
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Getting an object of a cryptographic class
        /// </summary>
        /// <returns></returns>
        private SymmetricAlgorithm GetSymmetricAlgorithm(ApplicationDbContext context)
        {
            var algorithm = new AesCng();

            var keyBytes = Encoding.ASCII.GetBytes(context.Settings.FirstOrDefault(x => x.Key == "EncryptionKey")?.Value ?? "");

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
            var crypt = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, crypt, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(text);
                    }
                }

                var encrypted = ms.ToArray();

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
            int arrayIvSize = 16;

            var bytesIv = new byte[arrayIvSize];

            for (int i = shifr.Length - arrayIvSize, j = 0; i < shifr.Length; i++, j++)
            {
                bytesIv[j] = shifr[i];
            }
            aesAlgorithm.IV = bytesIv;

            var mess = new byte[shifr.Length - arrayIvSize];

            for (int i = 0; i < shifr.Length - arrayIvSize; i++)
            {
                mess[i] = shifr[i];
            }

            var data = mess;

            var crypt = aesAlgorithm.CreateDecryptor(aesAlgorithm.Key, aesAlgorithm.IV);

            using (var ms = new MemoryStream(data))
            {
                using (var cs = new CryptoStream(ms, crypt, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}