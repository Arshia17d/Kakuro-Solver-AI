// کلاس‌های داده‌ای
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Kakuro
{
    public enum Model_Type
    {
        white,
        Black,
        Data
    }

    public class Model
    {
        public int Row { get; set; } // شماره سطر
        public int Col { get; set; } // شماره ستون
        public Model_Type Type { get; set; } // نوع خانه (W / B / D) [W for white and B for Black and D for Data cell]
        public int Value { get; set; } // مقدار خانه سفید
        public int KeyTop { get; set; } // کلید بالا
        public int KeyLeft { get; set; } // کلید پایین

        // سازنده

        public Model(int row, int col, Model_Type type)
        {
            Type = type;
            Row = row;
            Col = col;
            Value = 0;
            KeyTop = 0;
            KeyLeft = 0;
        }

        public override string ToString()
        {
            if (Type == Model_Type.Black)
                return "Black";
            if (Type == Model_Type.Data)
                return "Data";
            return Value.ToString();
        }
    }
}
