using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PFSign.ViewModels
{
    public class SummaryViewModel
    {
        [Display(Name = "签到最多次数")]
        public int MaxCount { get; set; }
        [Display(Name = "签到次数最多的人")]
        public IEnumerable<string> MaxSigned { get; set; }
        [Display(Name = "各时间段签到统计")]
        public object TimeStats { get; set; }
        [Display(Name = "本周签到次数")]
        public int ThisWeekCount { get; set; }
        [Display(Name = "上周签到次数")]
        public int LastWeekCount { get; set; }
        [Display(Name = "签到次数不足的人")]
        public Dictionary<string, int> BadSigned { get; set; }
        [Display(Name = "开始日期")]
        public DateTime StartDate { get; set; }
        [Display(Name = "结束日期")]
        public DateTime EndDate { get; set; }
    }
}
