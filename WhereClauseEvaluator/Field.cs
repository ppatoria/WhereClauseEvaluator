using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlUtil
{
    public class Field : IEquatable<string>
    {
        public Field(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public Field(string value)
        {
            Name = string.Empty;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object other)
        {
            return Value.Equals(other);
        }
        public bool Equals(string other)
        {
            return Value.Equals(other);
        }
        public bool Equals(Field other)
        {
            return Value.Equals(other.Value);
        }

        public static bool operator ==(Field lhs, string rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator ==(Field lhs, Field rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(Field lhs, string rhs)
        {
            return !lhs.Equals(rhs);
        }
        public static bool operator !=(Field lhs, Field rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static bool operator >(Field lhs, string rhs)
        {
            return
                double.Parse(lhs.Value)
                >
                double.Parse(rhs);
        }
        public static bool operator >(Field lhs, Field rhs)
        {
            return
                double.Parse(lhs.Value)
                >
                double.Parse(rhs.Value);
        }
        public static bool operator <(Field lhs, string rhs)
        {
            return
                double.Parse(lhs.Value)
                <
                double.Parse(rhs);
        }
        public static bool operator <(Field lhs, Field rhs)
        {
            return
                double.Parse(lhs.Value)
                <
                double.Parse(rhs.Value);
        }

        public static bool operator <=(Field lhs, string rhs)
        {
            return
                double.Parse(lhs.Value)
                <=
                double.Parse(rhs);
        }
        public static bool operator <=(Field lhs, Field rhs)
        {
            return
                double.Parse(lhs.Value)
                <=
                double.Parse(rhs.Value);
        }
        public static bool operator >=(Field lhs, string rhs)
        {
            return
                double.Parse(lhs.Value)
                >=
                double.Parse(rhs);
        }
        public static bool operator >=(Field lhs, Field rhs)
        {
            return
                double.Parse(lhs.Value)
                >=
                double.Parse(rhs.Value);
        }

        public static implicit operator string(Field field)
        {
            return field.Value;
        }

        public static implicit operator Field(string value)
        {
            return new Field(string.Empty,value);
        }
    }

}
