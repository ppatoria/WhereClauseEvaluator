﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace SqlUtil
{
    public enum NodeType
    {
        ColumnOperand,
        ConstantOperand,
        BinaryOperator,
        UnaryOperator
    };

    public interface INodeInfo<T>
    {
        NodeType NodeType { get; }
        T Data { get; }
    }
    public class BinaryOperator : INodeInfo<string>
    {
        public BinaryOperator(string value)
        {
            Data = value;
        }
        public NodeType NodeType => NodeType.BinaryOperator;

        public string Data { get; }
    }
    public class UnaryOperator : INodeInfo<string>
    {
        public UnaryOperator(string value)
        {
            Data = value;
        }
        public NodeType NodeType => NodeType.UnaryOperator;

        public string Data { get; }
    }

    public class ConstantOperand : INodeInfo<string>
    {
        public ConstantOperand(string value)
        {
            Data = value;
        }
        public NodeType NodeType => NodeType.ConstantOperand;

        public string Data { get; }
    }
    public class ConstantOperandOfList : INodeInfo<IList<string>>
    {
        public ConstantOperandOfList(IList<string> value)
        {
            Data = value;
        }
        public NodeType NodeType => NodeType.ConstantOperand;

        public IList<string> Data { get; }
    }

    public class ColumnOperand : IEquatable<ConstantOperand>, INodeInfo<KeyValuePair<string,string>>
    {
        
        public ColumnOperand(string name, string value)
        {
            Data = new KeyValuePair<string, string>(name, value);
        }


        public KeyValuePair<string,string> Data { get; set; }

        public NodeType NodeType =>  NodeType.ColumnOperand;


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object other)
        {
            return Data.Equals(other);
        }
        public bool Equals(ConstantOperand other)
        {
            return Data.Value.Equals(other.Data);
        }

        public static bool operator ==(ColumnOperand lhs, ConstantOperand rhs)
        {
            return lhs.Equals(rhs);
        }
        public static bool operator !=(ColumnOperand lhs, ConstantOperand rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static bool operator >(ColumnOperand lhs, ConstantOperand rhs)
        {
            return
                double.Parse(lhs.Data.Value)
                >
                double.Parse(rhs.Data);
        }
        public static bool operator <(ColumnOperand lhs, ConstantOperand rhs)
        {
            return
                double.Parse(lhs.Data.Value)
                <
                double.Parse(rhs.Data);
        }

        public static bool operator <=(ColumnOperand lhs, ConstantOperand rhs)
        {
            return
                double.Parse(lhs.Data.Value)
                <=
                double.Parse(rhs.Data);
        }
        public static bool operator >=(ColumnOperand lhs, ConstantOperand rhs)
        {
            return
                double.Parse(lhs.Data.Value)
                >=
                double.Parse(rhs.Data);
        }

        public static implicit operator string(ColumnOperand field)
        {
            return field.Data.Value;
        }

        public static implicit operator ColumnOperand(string value)
        {
            return new ColumnOperand(string.Empty,value);
        }
    }

}
