using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PFSign.Data;
using PFSign.ViewModels;
using System;
using System.Collections.Generic;
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
            DateTime _date = date?.Date ?? DateTime.UtcNow.Date;
            return View(await (from s in _context.SignIn
                               where s.Time >= _date
                               && s.Time < _date.AddDays(1)
                               orderby s.Time descending
                               select s)
                               .AsNoTracking()
                               .ToListAsync());
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
                                    select s)
                                    .AsNoTracking()
                                    .ToListAsync();
            if (signInList.Count() == 0)
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
                signIn.Time = DateTime.UtcNow;
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

        //#region 删除
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var signIn = await _context.SignIn
        //        .AsNoTracking()
        //        .SingleOrDefaultAsync(m => m.ID == id);
        //    if (signIn == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(signIn);
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var signIn = await _context.SignIn.SingleOrDefaultAsync(m => m.ID == id);
        //    _context.SignIn.Remove(signIn);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //#endregion

        public IActionResult Stats(DateTime? start = null, DateTime? end = null)
        {
            ViewBag.Start = start?.Date;
            ViewBag.End = end?.Date ?? DateTime.UtcNow.Date;
            return View();
        }

        public async Task<object> SignInStats(DateTime? start = null, DateTime? end = null)
        {
            DateTime
                _start = start?.Date ?? DateTime.MinValue,
                _end = end?.Date ?? DateTime.UtcNow.Date;
            return await (from s in _context.SignIn
                          where s.Time >= _start
                          && s.Time <= _end.AddDays(1)
                          group s by s.Name into g
                          select new
                          {
                              Name = g.Key,
                              Num = g.Count()
                          })
                          .AsNoTracking()
                          .ToListAsync();
        }

        public async Task<IActionResult> Summary(DateTime? date = null)
        {
            //若没有指定日期则当天
            DateTime day = date?.Date ?? DateTime.UtcNow.Date;
            //获取本周开始时间,作为上周五时间末尾
            DateTime weekEnd = day.AddDays(-((int.Parse((day.DayOfWeek.ToString("d"))) + 2) % 7));
            //上周开始时间
            DateTime weekStart = weekEnd.AddDays(-7);
            //上上周开始时间，用于对比两周签到情况
            DateTime lastWeekStart = weekEnd.AddDays(-14);

            //上周签到信息
            var signed = await (from s in _context.SignIn
                                where s.Time < weekEnd
                                && s.Time >= weekStart
                                select s)
                                .AsNoTracking()
                                .ToListAsync();
            //本周签到次数
            int thisWeekCount = signed.Count();
            //上周签到次数
            int lastWeekCount = await (from s in _context.SignIn
                                       where s.Time < weekStart
                                       && s.Time >= lastWeekStart
                                       select s)
                                       .AsNoTracking()
                                       .CountAsync();
            //上周每个人签到的次数,并降序
            var people = from s in signed
                         group s by s.Name into g
                         select new
                         {
                             Name = g.Key,
                             Num = g.Count(),
                         } into p
                         orderby p.Num descending
                         select p;

            var timestats = from h in Enumerable.Range(0, 24)
                            let t = from s in signed
                                    group s by s.Time.ToLocalTime().Hour into g
                                    orderby g.Key
                                    select g
                            select new
                            {
                                Hour = h,
                                Num = t.SingleOrDefault(x => x.Key == h)?.Count() ?? 0
                            };
                             

            //上周最大签到数
            int maxCount = people.Count() == 0 ? 0 : people.Max(x => x.Num);

            var model = new SummaryViewModel()
            {
                MaxCount = maxCount,
                MaxSigned = from p in people
                             where p.Num == maxCount
                             select p.Name,
                BadSigned = (from p in people
                             where p.Num < 4
                             select p).ToDictionary(x => x.Name, x => x.Num),
                TimeStats = timestats,
                ThisWeekCount = thisWeekCount,
                LastWeekCount = lastWeekCount,
                StartDate = weekStart,
                EndDate = weekEnd
            };

            return View(model);
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
                && e.Time < DateTime.Today.AddDays(1));
        }
        #endregion
    }
}
