using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwap.TMB {
    public class TmbFile {
        /*
         * C012 (4 bytes)
         * length (including C012) (4 bytes)
         * <----- offset is from here
         * contents
         * 
         * 12 = vfx (12 before)
         * 63 = sound (12 before)
         * 10 = pap (24 before)
         */
    }
}
