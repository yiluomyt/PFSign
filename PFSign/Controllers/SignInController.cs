using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PFSign.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PFSign.Controllers
{
    public class SignInController : Controller
    {
        private readonly SignInDBContext _context;

        public SignInController(SignInDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 返回某日的签到信息
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(DateTime? date = null)
        {
            ViewBag.Date = date;
            DateTime _date = (date?.Date) ?? DateTime.Today;
            return View(await (from s in _context.SignIn
                               where s.Time >= _date
                               && s.Time < _date.AddDays(1)
                               orderby s.Time descending
                               select s).ToListAsync());
        }

        /// <summary>
        /// 返回某人的签到信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(string name)
        {
            if (name == null)
            {
                return NotFound();
            }

            var signInList = await (from s in _context.SignIn
                                    where s.Name == name
                                    orderby s.Time descending
                                    select s).ToListAsync();
            if (signInList == null)
            {
                return NotFound();
            }

            return View(signInList);
        }

        #region 创建
        public IActionResult Create()
        {
            Request.Cookies.TryGetValue("name", out string name);
            Request.Cookies.TryGetValue("seat", out string seat);
            int.TryParse(seat, out int seatnum);
            if (!String.IsNullOrWhiteSpace(name))
            {
                SignIn model = new SignIn()
                {
                    Name = name,
                    Seat = seatnum
                };
                return View(model);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Seat")] SignIn signIn)
        {
            if (ModelState.IsValid)
            {
                signIn.Time = DateTime.Now;
                _context.Add(signIn);
                await _context.SaveChangesAsync();
                CookieOptions cookieOptions = new CookieOptions()
                {
                    HttpOnly = true,
                    Expires = DateTime.Today.AddMonths(1),
                };
                Response.Cookies.Append("name", signIn.Name, cookieOptions);
                Response.Cookies.Append("seat", signIn.Seat.ToString(), cookieOptions);
                return RedirectToAction(nameof(Index));
            }
            return View(signIn);
        } 
        #endregion

        #region 删除
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var signIn = await _context.SignIn
                .SingleOrDefaultAsync(m => m.ID == id);
            if (signIn == null)
            {
                return NotFound();
            }

            return View(signIn);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var signIn = await _context.SignIn.SingleOrDefaultAsync(m => m.ID == id);
            _context.SignIn.Remove(signIn);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        public IActionResult Stats(DateTime? start = null, DateTime? end = null)
        {
            ViewBag.End = end ?? DateTime.Today;
            return View();
        }

        public async Task<object> SignInStats(DateTime? start = null, DateTime? end = null)
        {
            DateTime 
                _start = start ?? DateTime.MinValue,
                _end = end ?? DateTime.MaxValue;
            return await (from s in _context.SignIn
                          where s.Time >= _start
                          && s.Time <= _end.AddDays(1)
                          group s by s.Name into g
                          select new
                          {
                              Name = g.Key,
                              Num = g.Count()
                          }).ToListAsync();
        }

        #region 检验API
        public async Task<bool> SeatExists(int seat)
        {
            return !await _context.SignIn.AnyAsync(
                e => e.Seat == seat 
                && e.Time >= DateTime.Today
                && e.Time < DateTime.Today.AddDays(1));
        }

        public async Task<bool> PersonExists(string name)
        {
            return !await _context.SignIn.AnyAsync(
                e => e.Name == name 
                && e.Time >= DateTime.Today
                && e.Time <  DateTime.Today.AddDays(1));
        }
        #endregion
    }
}
