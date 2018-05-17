using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.UI
{
    public interface IDialog
    {
        bool Visible { get; set; }

        void Draw();
    }
}
