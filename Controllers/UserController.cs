using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using VinaShoseShop.Models;  // Thêm dòng này

namespace VinaShoseShop.Controllers
{
    public class UserController : Controller
    {
		ApplicationDbcontext db = new ApplicationDbcontext();

        public ActionResult dangky()
        {
            Nguoidung nguoidung = new Nguoidung();  
            return View(nguoidung);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult dangky(Nguoidung nguoidung)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    nguoidung.Matkhau = GetMD5(nguoidung.Matkhau);
                    db.Nguoidungs.Add(nguoidung);
                    db.SaveChanges();
                    return RedirectToAction("Dangnhap");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Đã có lỗi xảy ra: " + ex.Message;
                }
            }
            return View("Dangky");
        }

        public ActionResult Dangnhap()
        {
			LoginModel loginModel = new LoginModel();   
            return View(loginModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Dangnhap(FormCollection userlog)
        {
            LoginModel model = new LoginModel();
            if (ModelState.IsValid)
            {
                string userMail = userlog["userMail"];
                string password = userlog["password"];
                var f_password = GetMD5(password);

                var islogin = db.Nguoidungs.SingleOrDefault(x => x.Email.Equals(userMail) && x.Matkhau.Equals(f_password));
                if (islogin != null)
                {
                    Session["use"] = islogin;
                    if (userMail == "admin@gmail.com" || userMail == "admin2@gmail.com")
                    {
                        return RedirectToAction("Index", "Admin/Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
					ViewBag.Chophep = false;
					ViewBag.Fail = "Đăng nhập thất bại: Tài khoản và mật khẩu của bạn không chính xác xin vui lòng thử lại!";
					return View(model);
                }
            }
            return View(model);
        }

        public ActionResult DangXuat()
        {
            Session["use"] = null;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Thongtin()
        {
            var user = Session["use"] as Nguoidung;
            if (user == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            return View(user);
        }

        public ActionResult Edit()
        {
            var user = Session["use"] as Nguoidung;
            if (user == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Nguoidung user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Nguoidungs.FirstOrDefault(u => u.Manguoidung == user.Manguoidung);
                if (existingUser != null)
                {
                    existingUser.Hoten = user.Hoten;
                    existingUser.Dienthoai = user.Dienthoai;
                    existingUser.Diachi = user.Diachi;
                    db.SaveChanges();
                    Session["use"] = existingUser;
                    return RedirectToAction("Thongtin");
                }
            }
            return View(user);
        }

        public ActionResult Donhang()
        {
            var user = Session["use"] as Nguoidung;
            if (user == null)
            {
                return RedirectToAction("Dangnhap", "User");
            }
			var orders = db.Donhangs.Where(d => d.Manguoidung == user.Manguoidung).ToList();

			var userList = new List<Nguoidung>();
			foreach (var u in orders)
			{
				var nguoiDung = db.Nguoidungs.FirstOrDefault(
					p => p.Manguoidung == u.Manguoidung);
				userList.Add(nguoiDung);
			}

			ViewBag.UserList = userList;

            return View(orders);
        }

        [HttpPost]
        public ActionResult NhanHang(int id)
        {
            var order = db.Donhangs.FirstOrDefault(d => d.Madon == id);
            if (order != null && order.Tinhtrang == 2)
            {
                order.Tinhtrang = 3;
                db.SaveChanges();
            }
            return RedirectToAction("Donhang");
        }

        [HttpPost]
        public ActionResult Huydon(int id)
        {
            var order = db.Donhangs.FirstOrDefault(d => d.Madon == id);
            if (order != null && order.Tinhtrang == 1)
            {
                order.Tinhtrang = 4;
                db.SaveChanges();
            }
            return RedirectToAction("Donhang");
        }

        private string GetMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

		public ActionResult Detaildonhang(int? Madon)
		{
			var ctDonhangsQuery = db.Chitietdonhangs.AsQueryable();

			// Kiểm tra nếu Madon được truyền từ DonhangsController
			if (Madon != null)
			{
				ctDonhangsQuery = ctDonhangsQuery.Where(x => x.Madon == Madon);
			}

			// Order and include related entities
			var ctDonhangs = ctDonhangsQuery
							 .OrderBy(x => x.Madon)
							 .Include(x => x.Sanpham)
							 .ToList();

			// Extract the products from the loaded Chitietdonhangs
			var products = ctDonhangs.Select(ct => ct.Sanpham).Where(sp => sp != null).ToList();

			ViewBag.SanPham = products;
			return View(ctDonhangs);
		}




	}
}
