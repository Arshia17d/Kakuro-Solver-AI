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
        public int RightKey { get; set; } // کلید بالا
        public int BottonKey { get; set; } // کلید پایین
        public List<int> Domain { get; set; }

        // سازنده

        public Model(int row, int col, Model_Type type)
        {
            Type = type;
            Row = row;
            Col = col;
            Value = 0;
            RightKey = 0;
            BottonKey = 0;
            Domain = Enumerable.Range(1, 9).ToList();
        }

        public override string ToString()
        {
            if (Type == Model_Type.Black)
                return "B";
            if (Type == Model_Type.Data)
                return "I";
            return (Value == 0) ? "0" : Value.ToString();
        }

        public void SetValue(int value)
        {
            if (9 > Value || Value <= 0)
            {
                return;
            }
            Value = value;
        }

        public void SetKey(int bottonKey, int rightKey)
        {
            BottonKey = bottonKey;
            RightKey = rightKey;
        }
    }

    public class Entry
    {
        public List<Model> Cells { get; set; }
        public int Key { get; set; }

        public Entry()
        {
            Cells = new List<Model>();
        }
    }
}
