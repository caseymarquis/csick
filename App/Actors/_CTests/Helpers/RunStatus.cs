﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSick.Actors._CTests.Helpers {
    public enum RunStatus {
        WaitingOnParent,
        Scheduled,
        Running,
        Paused,
    }
}
