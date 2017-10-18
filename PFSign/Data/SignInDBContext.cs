using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace PFSign.Data
{
    public class SignInDBContext : DbContext
    {
        public SignInDBContext(DbContextOptions<SignInDBContext> options)
            :base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<SignIn> SignIn { get; set; }
    }

    public class SignIn
    {
        public int ID { get; set; }
        [Display(Name = "姓名")]
        [StringLength(6, ErrorMessage = "{0}不超过{1}个字符！")]
        [Required(ErrorMessage = "姓名必填！")]
        [Remote("PersonExists", "SignIn", ErrorMessage = "你今天已签到！")]
        public string Name { get; set; }
        [Display(Name = "签到时间")]
        [DataType(DataType.DateTime)]
        public DateTime Time { get; set; }
        [Display(Name = "座位号")]
        [Required(ErrorMessage = "签到座位必填！")]
        [Range(1,36,ErrorMessage = "座位号在1~36之间！")]
        [Remote("SeatExists", "SignIn", ErrorMessage = "该座位已有人！")]
        public int Seat { get; set; }
    }
}
