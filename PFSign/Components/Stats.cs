using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PFSign.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PFSign.Components
{
    public class SeatStats : ViewComponent
    {
        private readonly SignInDBContext _context;

        public SeatStats(SignInDBContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await (from s in _context.SignIn
                               where s.Time >= DateTime.Today
                               && s.Time < DateTime.Today.AddDays(1)
                               select s).ToDictionaryAsync(x => x.Seat, x => x.Name));
        }
    }
}
