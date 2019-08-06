﻿using System.IO;
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

            return View();
        }

        [HttpPost]
        public ActionResult Data(string key)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            using (SymmetricAlgorithm algorithm = GetSymmetricAlgorithm(key))
            {
                if (User.Identity.IsAuthenticated)
                {
                    var datas = context.Datas.Where(x => x.UserId == User.Identity.Name).ToList().Select(x => DecrypteText(algorithm, x.EncodingText));

                    return PartialView(datas.ToList());
                }
            }

            return new EmptyResult();
        }

        public ActionResult Create(string text, string key)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            using (SymmetricAlgorithm algorithm = GetSymmetricAlgorithm(key))
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

        private SymmetricAlgorithm GetSymmetricAlgorithm(string encryptionKey)
        {
            SymmetricAlgorithm algorithm = SymmetricAlgorithm.Create("AES");

            byte[] keyBytes = Encoding.ASCII.GetBytes(encryptionKey);

            algorithm.Key = keyBytes;

            return algorithm;
        }

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