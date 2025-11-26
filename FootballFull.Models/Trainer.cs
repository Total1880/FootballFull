using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballFull.Models
{
    public class Trainer
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }

        /// <summary>
        /// 1-5: hoe goed tactisch
        /// </summary>
        public int TacticalSkill { get; set; }

        /// <summary>
        /// 1-5: hoe goed in motivatie
        /// </summary>
        public int Motivation { get; set; }
    }
}
