/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;

namespace ASC.ActiveDirectory.Base.Expressions
{
    public class Expression : ICloneable
    {
        private readonly Op _op;
        private bool _negative;
        private readonly string _attributeName;
        private readonly string _attributeValue;

        private const string EQUIAL = "=";
        private const string APPROXIMATELY_EQUIAL = "~=";
        private const string GREATER = ">";
        private const string GREATER_OR_EQUAL = ">=";
        private const string LESS = "<";
        private const string LESS_OR_EQUAL = "<=";

        internal Expression()
        {
        }

        /// <summary>
        /// To specify unary operations
        /// </summary>
        /// <param name="op">Operator</param>
        /// <param name="attrbuteName">Attribute name</param>
        public Expression(string attrbuteName, Op op)
        {
            if (op != Op.Exists && op != Op.NotExists)
                throw new ArgumentException("op");

            if (string.IsNullOrEmpty(attrbuteName))
                throw new ArgumentException("attrbuteName");

            _op = op;
            _attributeName = attrbuteName;
            _attributeValue = "*";
        }

        /// <summary>
        /// To specify binary operations
        /// </summary>
        /// <param name="op">Operator</param>
        /// <param name="attrbuteName">Attribute name</param>
        /// <param name="attrbuteValue">Attribute value</param>
        public Expression(string attrbuteName, Op op, string attrbuteValue)
        {
            if (op == Op.Exists || op == Op.NotExists)
                throw new ArgumentException("op");

            if (string.IsNullOrEmpty(attrbuteName))
                throw new ArgumentException("attrbuteName");

            _op = op;
            _attributeName = attrbuteName;
            _attributeValue = attrbuteValue;
        }

        /// <summary>
        /// Expression as a string
        /// </summary>
        /// <returns>Expression string</returns>
        public override string ToString()
        {
            string sop;
            switch (_op)
            {
                case Op.NotExists:
                case Op.Exists:
                case Op.Equal:
                case Op.NotEqual:
                    sop = EQUIAL;
                    break;
                case Op.Greater:
                    sop = GREATER;
                    break;
                case Op.GreaterOrEqual:
                    sop = GREATER_OR_EQUAL;
                    break;
                case Op.Less:
                    sop = LESS;
                    break;
                case Op.LessOrEqual:
                    sop = LESS_OR_EQUAL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var expressionString = "({0}{1}{2}{3})";
            expressionString = string.Format(expressionString,
                //positive or negative
                (((int) _op & 0x010000) == 0x010000 || _negative) ? "!" : "", _attributeName, sop, _attributeValue);

            return expressionString;
        }

        /// <summary>
        /// Negation
        /// </summary>
        /// <returns>Self</returns>
        public Expression Negative()
        {
            _negative = !_negative;
            return this;
        }

        /// <summary>
        /// Existence
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <returns>New Expression</returns>
        public static Expression Exists(string attrbuteName)
        {
            return new Expression(attrbuteName, Op.Exists);
        }

        /// <summary>
        /// Non-Existence
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <returns>New Expression</returns>
        public static Expression NotExists(string attrbuteName)
        {
            return new Expression(attrbuteName, Op.NotExists);
        }

        /// <summary>
        /// Equality
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <param name="attrbuteValue"></param>
        /// <returns>New Expression</returns>
        public static Expression Equal(string attrbuteName, string attrbuteValue)
        {
            return new Expression(attrbuteName, Op.Equal, attrbuteValue);
        }

        /// <summary>
        /// Not equality
        /// </summary>
        /// <param name="attrbuteName"></param>
        /// <param name="attrbuteValue"></param>
        /// <returns></returns>
        public static Expression NotEqual(string attrbuteName, string attrbuteValue)
        {
            return new Expression(attrbuteName, Op.NotEqual, attrbuteValue);
        }

        public static Expression Parse(string origin)
        {
            string spliter = null;
            var op = Op.Equal;

            var index = origin.IndexOf(EQUIAL, StringComparison.Ordinal);

            if (index > -1)
            {
                spliter = EQUIAL;
                op = Op.Equal;
            }
            else if ((index = origin.IndexOf(GREATER, StringComparison.Ordinal)) > -1)
            {
                spliter = GREATER;
                op = Op.Greater;
            }
            else if ((index = origin.IndexOf(GREATER_OR_EQUAL, StringComparison.Ordinal)) > -1)
            {
                spliter = GREATER_OR_EQUAL;
                op = Op.GreaterOrEqual;
            }
            else if ((index = origin.IndexOf(LESS, StringComparison.Ordinal)) > -1)
            {
                spliter = LESS;
                op = Op.Less;
            }
            else if ((index = origin.IndexOf(LESS_OR_EQUAL, StringComparison.Ordinal)) > -1)
            {
                spliter = LESS_OR_EQUAL;
                op = Op.LessOrEqual;
            }
            else if ((index = origin.IndexOf(APPROXIMATELY_EQUIAL, StringComparison.Ordinal)) > -1)
            {
                spliter = APPROXIMATELY_EQUIAL;
                op = Op.Exists;
            }

            if (string.IsNullOrEmpty(spliter))
                return null;

            var attributeName = origin.Substring(0, index);
            var attributeValue = origin.Substring(index + 1);

            if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
                return null;

            return new Expression(attributeName, op, attributeValue);
        }

        #region ICloneable Members
        /// <summary>
        /// ICloneable implemetation
        /// </summary>
        /// <returns>Clone object</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
